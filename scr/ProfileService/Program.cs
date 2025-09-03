using Common.Logging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProfileService.Consumers;
using ProfileService.Data;
using ProfileService.Helpers;

var builder = WebApplication.CreateBuilder(args);

LoggingSetup.ConfigureLogging(builder);

var rabbitMqConfig = builder.Configuration.GetSection("RabbitMqConfig");

builder.Services.AddDbContext<ProfileDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserCreatedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("profile", false));

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.ReceiveEndpoint("profile-user-created", e =>
        {
            e.ConfigureConsumer<UserCreatedConsumer>(ctx);
            e.UseMessageRetry(r => r.Interval(3, 500));
        });

        cfg.Host(rabbitMqConfig["Host"], "/", h =>
        {
            h.Username(rabbitMqConfig["Login"]);
            h.Password(rabbitMqConfig["Password"]);
        });

        var observer = ctx.GetRequiredService<CorrelationConsumeObserver>();
        cfg.ConnectConsumeObserver(observer);
    });
});

var app = builder.Build();

LoggingSetup.UseCorrelationLogging(app);

await InitializeDatabase.DropDatabasesAsync(app.Services);
await InitializeDatabase.InitDbAsync(app.Services);
await InitializeDatabase.SeedContactTypesAsync(app.Services);
await InitializeDatabase.SeedAddressTypesAsync(app.Services);

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
