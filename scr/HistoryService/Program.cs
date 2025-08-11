using HistoryService.Consumers;
using HistoryService.Data;
using HistoryService.Helpers;
using MassTransit;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var jwtConfig = builder.Configuration.GetSection("Jwt");
var rabbitMqConfig = builder.Configuration.GetSection("RabbitMqConfig");

var jwksUrl = jwtConfig["JwksUrl"];
var keys = await JwtHelper.FetchSigningKeysFromJwks(jwksUrl);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtConfig["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keys,

            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        };

        options.TokenValidationParameters.RoleClaimType = "role";
    });

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));

builder.Services.AddSingleton<HistoryContext>();

builder.Services.AddControllers();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TransactionCreatedConsumer>();
    x.AddConsumer<AccountActionDoneConsumer>();
    x.AddConsumer<TransactionCompletedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("history", false));

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.ReceiveEndpoint("history-transaction-created", e =>
        {
            e.ConfigureConsumer<TransactionCreatedConsumer>(ctx);
            e.UseMessageRetry(r => r.Interval(3, 500));
        });

        cfg.ReceiveEndpoint("history-account-action-done", e =>
        {
            e.ConfigureConsumer<AccountActionDoneConsumer>(ctx);
            e.UseMessageRetry(r => r.Interval(3, 500));
        });

        cfg.ReceiveEndpoint("history-transaction-completed", e =>
        {
            e.ConfigureConsumer<TransactionCompletedConsumer>(ctx);
            e.UseMessageRetry(r => r.Interval(3, 500));
        });

        cfg.Host(rabbitMqConfig["Host"], "/", h =>
        {
            h.Username(rabbitMqConfig["Login"]);
            h.Password(rabbitMqConfig["Password"]);
        });
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Admin");
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var historyContext = scope.ServiceProvider.GetRequiredService<HistoryContext>();
    await historyContext.DropCollectionAsync("HistoryEvents");
    await historyContext.DropDatabaseAsync();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
