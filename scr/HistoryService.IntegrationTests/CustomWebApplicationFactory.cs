using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using HistoryService.Consumers;
using HistoryService.Data;
using HistoryService.Helpers;
using HistoryService.IntegrationTests;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mongo2Go;
using MongoDB.Driver;
using Npgsql;

namespace History.IntegrationTests
{
    public class CustomWebApplicationFactory(GlobalJwksServer jwks) : WebApplicationFactory<HistoryService.Program>
    {
        private readonly GlobalJwksServer _jwks = jwks;
        private readonly MongoDbRunner _mongoRunner = MongoDbRunner.Start(singleNodeReplSet: true);

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationTests");

            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                // Downloading all settings + IntegrationTests settings
                configBuilder.AddJsonFile("appsettings.json", optional: false)
                             .AddJsonFile("appsettings.IntegrationTests.json", optional: false)
                             .AddEnvironmentVariables();
            });

            builder.ConfigureServices(services =>
            {
                // Get configuration
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                FakeTokenHelper.Init(config);

                // Remove previous MassTransit configuration
                var massTransitDescriptors = services
                    .Where(d => d.ServiceType.Namespace != null &&
                                d.ServiceType.Namespace.StartsWith("MassTransit"))
                    .ToList();

                foreach (var descr in massTransitDescriptors)
                    services.Remove(descr);

                // Adding Jwt authentication with keys from Jwks mock-server + adding fake kety
                var keys = JwtHelper.FetchSigningKeysFromJwks(config["Jwt:JwksUrl"]).GetAwaiter().GetResult(); ;

                // Adding public key FakeTokenHelper for fake oken verifying
                keys = keys.Concat(new[] { FakeTokenHelper.GetPublicKey() });

                services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = config["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = config["Jwt:Audience"],
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKeys = keys,
                        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                    };
                });

                services.AddMassTransitTestHarness(x =>
                {
                    x.AddConsumer<TransactionCreatedConsumer>();
                    x.AddConsumer<TransactionCompletedConsumer>();
                    x.AddConsumer<AccountActionDoneConsumer>();
                });

                // Видаляємо оригінальний HistoryContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(HistoryContext));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Додаємо тестовий HistoryContext з Mongo2Go
                services.AddSingleton<HistoryContext>(_ =>
                {
                    var settings = MongoClientSettings.FromConnectionString(_mongoRunner.ConnectionString);
                    var client = new MongoClient(settings);
                    return new HistoryContext(Options.Create(new MongoDbSettings
                    {
                        ConnectionString = _mongoRunner.ConnectionString,
                        DatabaseName = $"tests-history-{Guid.NewGuid()}"
                    }));
                });
            });
        }

        public async Task InitializeAsync() => await Task.CompletedTask;

        public async Task DisposeAsync()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<HistoryContext>();

            // remove db after ending
            await db.DropCollectionAsync("HistoryEvents");
            await db.DropDatabaseAsync();

            base.Dispose();
            _mongoRunner?.Dispose();
        }

        public HistoryContext GetDbContext()
        {
            var scope = Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<HistoryContext>();
        }
    }
}