using Common.Logging;
using Mapster;
using MapsterMapper;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProfileService.Authorization;
using ProfileService.Consumers;
using ProfileService.Data;
using ProfileService.Helpers;

var builder = WebApplication.CreateBuilder(args);

LoggingSetup.ConfigureLogging(builder);

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


builder.Services.AddDbContext<ProfileDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// автоматично застосує всі класи IRegister з твого проекту
TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);

builder.Services.AddControllers();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireUserId", policy =>
        policy.Requirements.Add(new RequireUserIdRequirement()));
});
builder.Services.AddSingleton<IAuthorizationHandler, RequireUserIdHandler>();

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
await InitializeDatabase.SeedLanguagesAsync(app.Services);
await InitializeDatabase.SeedTimezonesAsync(app.Services);
await InitializeDatabase.SeedCountriesAsync(app.Services);

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
