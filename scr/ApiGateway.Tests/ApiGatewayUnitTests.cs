using ApiGateway.IntegrationTests;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;

namespace ApiGateway.Tests
{
    [Collection("Jwks server collection")]
    public class ApiGatewayUnitTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly IConfigurationRoot _config;
        private readonly ServiceProvider _provider;
        private readonly ServiceCollection _services = new ServiceCollection();

        public ApiGatewayUnitTests()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.IntegrationTests.json", optional: false)
                .Build();

            _services.AddLogging();
            _services.AddReverseProxy().LoadFromConfig(_config.GetSection("ReverseProxy"));
            _provider = _services.BuildServiceProvider();
        }

        [Fact]
        public void Should_Load_ReverseProxy_Config_From_AppSettings()
        {
            var configSnapshot = _provider.GetRequiredService<IProxyConfigProvider>().GetConfig();

            // Act
            var routes = configSnapshot.Routes;

            var expectedTransforms = new Dictionary<string, (string removePrefix, string prefix)>
            {
                { "accountService", ("/accounts", "/api/v1/accounts") },
                { "transactionServce", ("/transactions", "/api/v1/transactions") },
                { "historyService", ("/history", "/api/v1/history") },
                { "authService", ("/auth", "/api/v1/auth") }
            };


            // Act & Assert
            foreach (var expected in expectedTransforms)
            {
                var route = routes.FirstOrDefault(r => r.RouteId == expected.Key);

                route.Should().NotBeNull();
                route.Transforms.Should().Contain(t =>
                    t.ContainsKey("PathRemovePrefix") && t["PathRemovePrefix"] == expected.Value.removePrefix);

                route.Transforms.Should().Contain(t =>
                    t.ContainsKey("PathPrefix") && t["PathPrefix"] == expected.Value.prefix);
            }
        }

        [Fact]
        public void Routes_Should_Contain_All_Services()
        {
            var routes = _provider.GetRequiredService<IProxyConfigProvider>().GetConfig().Routes;
            var expectedRoutes = new[] { "authService", "accountService", "transactionServce", "historyService" };

            foreach (var route in expectedRoutes)
                routes.Should().Contain(r => r.RouteId == route);
        }

        [Fact]
        public void ServicesRoute_Should_Have_AuthenticatedUsersOnly_Policy()
        {
            var routesWithPolicyId = new[] {  "accountService", "transactionServce", "historyService" };


            foreach(var routeId in routesWithPolicyId)
            {
                var route = _provider.GetRequiredService<IProxyConfigProvider>()
                    .GetConfig().Routes.First(r => r.RouteId.ToLower().Contains(routeId.ToLower()));

                route.AuthorizationPolicy.Should().Be("AuthenticatedUsersOnly");
            }
        }

        [Fact]
        public void Jwt_Should_Have_Issuer_And_Audience_NotNull()
        {
            var jwtSection = _config.GetSection("Jwt");
            jwtSection["Issuer"].Should().NotBeNull();
            jwtSection["Audience"].Should().NotBeNull();
            jwtSection["JwksUrl"].Should().NotBeNull();
        }

        //check DI - Authentication register
        [Fact]
        public void Should_Register_Authentication_Services()
        {
            var services = new ServiceCollection();
            services.AddAuthentication("Bearer");

            var provider = services.BuildServiceProvider();
            var authService = provider.GetService<Microsoft.AspNetCore.Authentication.IAuthenticationService>();

            authService.Should().NotBeNull();
        }
    }
}
