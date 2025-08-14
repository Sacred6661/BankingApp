using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using MassTransit;
using Messaging;
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
using Npgsql;
using TransactionService.Consumers;
using TransactionService.Data;
using TransactionService.Helpers;

namespace TransactionService.IntegrationTests
{
    public class CustomWebApplicationFactory(GlobalJwksServer jwks) : WebApplicationFactory<TransactionService.Program>
    {
        private readonly GlobalJwksServer _jwks = jwks;

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
                    x.AddConsumer<AccountActionDoneConsumer>().Endpoint(e => e.Name = "account-action-done");
                });

                // Remove existed dbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TransactionsDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                var originalConnection = config.GetConnectionString("DefaultConnection");

                // change database name to "test_{originalDbName}_{Guid.NewGuid()}"
                var builderCs = new NpgsqlConnectionStringBuilder(originalConnection);
                var originalDbName = builderCs.Database;
                builderCs.Database = $"test_{originalDbName}_{Guid.NewGuid()}";

                var testConnection = builderCs.ConnectionString;

                // register new dbContext with test connection
                services.AddDbContext<TransactionsDbContext>(options =>
                    options.UseNpgsql(testConnection));

                // run migrations before tests
                var sp2 = services.BuildServiceProvider();
                using var scope2 = sp2.CreateScope();
                var db = scope2.ServiceProvider.GetRequiredService<TransactionsDbContext>();

                db.Database.Migrate();
            });
        }

        public async Task InitializeAsync() => await Task.CompletedTask;

        public async Task DisposeAsync()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();

            // remove db after ending
            await db.Database.EnsureDeletedAsync();
        }

        public TransactionsDbContext GetDbContext()
        {
            var scope = Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();
        }
    }
}