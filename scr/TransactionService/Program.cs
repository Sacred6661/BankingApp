using AutoMapper;
using MassTransit;
using Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TransactionService.Authorization;
using TransactionService.Consumers;
using TransactionService.Data;
using TransactionService.Helpers;
using TransactionService.Mapping;

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

builder.Services.AddDbContext<TransactionsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var loggerFactory = LoggerFactory.Create(builder => {
    builder.AddConsole();
});

var mapperConfig = new MapperConfiguration(cfg =>
{
    cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
}, loggerFactory);

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
    x.AddConsumer<AccountActionDoneConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("transaction", false));

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.ReceiveEndpoint("transaction-account-action-done", e =>
        {
            e.ConfigureConsumer<AccountActionDoneConsumer>(ctx);
            e.UseMessageRetry(r => r.Interval(3, 500));
        });

        cfg.Host(rabbitMqConfig["Host"], "/", h =>
        {
            h.Username(rabbitMqConfig["Login"]);
            h.Password(rabbitMqConfig["Password"]);
        });

        cfg.Publish<TransactionCreated>(x =>
        {
            x.ExchangeType = "fanout";
            x.Durable = true;
            x.AutoDelete = false;
        });

        cfg.Publish<TransactionCreated>(x => x.ExchangeType = "fanout");
    });
});

var app = builder.Build();

await InitializeDatabase.DropDatabasesAsync(app.Services);
await InitializeDatabase.InitDbAsync(app.Services);

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


namespace TransactionService
{
    public partial class Program { }
}
