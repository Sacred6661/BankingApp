using AccountService.IntegrationTests;
using ApiGateway.IntegrationTests;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ApiGateway.Tests
{
    [Collection("Jwks server collection")]
    public class ApiGatewayIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _clientUnauthorized;
        private readonly HttpClient _clientAuthorized;
        private readonly IConfiguration _config;
        private readonly GlobalJwksServer _jwks;
        private readonly CustomWebApplicationFactory _factory;

        public ApiGatewayIntegrationTests(GlobalJwksServer jwks)
        {
            _jwks = jwks;
            _factory = new CustomWebApplicationFactory(jwks);

            _clientUnauthorized = _factory.CreateClient();
            _clientAuthorized = _factory.CreateClient();
            var token = FakeTokenHelper.GenerateToken(Guid.NewGuid().ToString(), "Admin");
            _clientAuthorized.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            _config = _factory.Services.GetRequiredService<IConfiguration>();
        }

        [Fact]
        public async Task AccountsRoute_Should_Return_401_If_No_Token()
        {
            var response = await _clientUnauthorized.GetAsync("/accounts");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AccountsRoute_Should_Return_OK_If_Valid_Token()
        {
            var response = await _clientAuthorized.GetAsync("/accounts/");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task All_Routes_Should_Proxy_To_Correct_Services()
        {
            var routesToTest = new Dictionary<string, string>
            {
                { $"/accounts/{Guid.NewGuid()}", "accounts" },
                { $"/transactions/deposit", "transactions" },
                { $"/history/tarnsaction/{Guid.NewGuid()}", "history" },
                { $"auth/login", "auth" }
            };

            // Act & Assert
            foreach (var route in routesToTest)
            {
                var response = await _clientAuthorized.GetAsync(route.Key);
                var body = await response.Content.ReadAsStringAsync();

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                body.Contains($"OK from {route.Value}");
                body.Contains($"/api/v1/{route.Value}");
            }
        }

        [Fact]
        public async Task UnknownRoute_Should_Return_404()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/unknown/path");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
