using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using AccountService.IntegrationTests;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiGateway.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<ApiGateway.Program>
    {
        private readonly GlobalJwksServer _jwks;

        public CustomWebApplicationFactory(GlobalJwksServer jwks)
        {
            _jwks = jwks;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationTests");
            
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                builder.UseEnvironment("IntegrationTests");

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

                // Adding Jwt authentication with keys from Jwks mock-server + adding fake kety
                var keys = AccountService.Helpers.JwtHelper.FetchSigningKeysFromJwks(config["Jwt:JwksUrl"]).GetAwaiter().GetResult(); ;

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
            });
        }
    }
}