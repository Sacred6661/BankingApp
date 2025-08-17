using AccountService.Data;
using AccountService.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using AccountService.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Diagnostics;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;
using AccountService.Authorization;
using Microsoft.AspNetCore.Authorization;
using MassTransit;
using AccountService.Consumers;
using Common.Logging;

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

builder.Services.AddDbContext<AccountDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var configExpr = new MapperConfigurationExpression();
configExpr.AddProfile<MappingProfile>();

var loggerFactory = LoggerFactory.Create(builder => {
    builder.AddConsole(); 
});

var mapperConfig = new MapperConfiguration(configExpr, loggerFactory);
var mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddControllers();

builder.Services.AddHttpClient();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireUserId", policy =>
        policy.Requirements.Add(new RequireUserIdRequirement()));
});
builder.Services.AddSingleton<IAuthorizationHandler, RequireUserIdHandler>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TransactionCreatedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("account", false));

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.ReceiveEndpoint("account-transaction-created", e =>
        {
            e.ConfigureConsumer<TransactionCreatedConsumer>(ctx);
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

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

namespace AccountService
{
    public partial class Program { }
}


