using AccountService.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AccountService.Data;
using System.IdentityModel.Tokens.Jwt;
using AccountService.Data.Models;
using System.Net;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.IntegrationTests
{
    [Collection("Jwks server collection")]
    public class AccountControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _config;
        private readonly GlobalJwksServer _jwks;
        private readonly CustomWebApplicationFactory _factory;

        public AccountControllerTests(GlobalJwksServer jwks)
        {
            _jwks = jwks;
            _factory = new CustomWebApplicationFactory(jwks);
            _client = _factory.CreateClient();
            _config = _factory.Services.GetRequiredService<IConfiguration>();
        }

        [Fact]
        public async Task CreateAccount_AsAdmin_ReturnsOk()
        {
             var token = FakeTokenHelper.GenerateToken(Guid.NewGuid().ToString(), role: "Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var request = new CreateAccountRequest
            {
                InitialBalance = 1000,
                UserId = Guid.NewGuid()
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/v1/accounts", content);

            // check for correct response
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().Contain("balance");

            // check for correct writing to the DB(Admin can change started balance, user - no)
            var db = _factory.GetDbContext();
            var account = db.Accounts.Where(a => a.UserId == request.UserId)?.FirstOrDefault();
            account.Should().NotBeNull();
            account.Balance.Should().Be(1000);
        }

        [Fact]
        public async Task CreateAccount_AsUser_ReturnsOk()
        {
            var token = FakeTokenHelper.GenerateToken(Guid.NewGuid().ToString(), role: "User");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;

            var request = new CreateAccountRequest
            {
                InitialBalance = 1000,
                UserId = Guid.NewGuid()
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/v1/accounts", content);

            // check for correct response

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().Contain("balance");

            // check for correct writing to the DB(Admin can change started balance, user - no)
            // Also user can create account only by himself(CreateAccountRequest.UserId will not work for role User)
            var db = _factory.GetDbContext();
            var account = db.Accounts.Where(a => a.UserId == Guid.Parse(userId))?.FirstOrDefault();
            account.Should().NotBeNull();
            account.Balance.Should().Be(0);
        }

        [Fact]
        public async Task GetAccount_AsAdmin_ReturnsAccount()
        {
            var token = FakeTokenHelper.GenerateToken(Guid.NewGuid().ToString(), role: "Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;

            var anotherUserId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var AnotherAccountId = Guid.NewGuid();

            // Adding 2 accounts to DB
            var db = _factory.GetDbContext();
            db.Accounts.Add(new Account
            {
                Id = accountId,
                UserId = Guid.Parse(userId),
                Balance = 1234
            });
            db.Accounts.Add(new Account
            {
                Id = AnotherAccountId,
                UserId = anotherUserId,
                Balance = 4321
            });
            await db.SaveChangesAsync();


            var response = await _client.GetAsync($"/api/v1/accounts/{accountId}");

            // 2 request should returns OK and correct data, because Admin user has access for any user account info

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var accountDto = JsonSerializer.Deserialize<AccountsDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            accountDto.Should().NotBeNull();
            accountDto.Balance.Should().Be(1234);

            response = await _client.GetAsync($"/api/v1/accounts/{AnotherAccountId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content = await response.Content.ReadAsStringAsync();
            accountDto = JsonSerializer.Deserialize<AccountsDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            accountDto.Should().NotBeNull();
            accountDto.Balance.Should().Be(4321);
        }

        [Fact]
        public async Task GetAccount_AsUser_ReturnsAccount()
        {
            var token = FakeTokenHelper.GenerateToken(Guid.NewGuid().ToString(), "User");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;

            var anotherUserId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var AnotherAccountId = Guid.NewGuid();

            // Adding 2 account to DB
            var db = _factory.GetDbContext();
            db.Accounts.Add(new Account
            {
                Id = accountId,
                UserId = Guid.Parse(userId),
                Balance = 1234
            });
            db.Accounts.Add(new Account
            {
                Id = AnotherAccountId,
                UserId = anotherUserId,
                Balance = 4321
            });
            await db.SaveChangesAsync();


            var response = await _client.GetAsync($"/api/v1/accounts/{accountId}");


            // one request is Ok, because it is account created for logged in user
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var accountDto = JsonSerializer.Deserialize<AccountsDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            accountDto.Should().NotBeNull();
            accountDto.Balance.Should().Be(1234);

            // this request is Forbidden, because user try to access someone else's account
            response = await _client.GetAsync($"/api/v1/accounts/{AnotherAccountId}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            content = await response.Content.ReadAsStringAsync();
        }

        [Fact]
        public async Task GetAllAccount_AsAdmin_ReturnsAllExistedAccounts()
        {
            var token = FakeTokenHelper.GenerateToken(Guid.NewGuid().ToString(), role: "Admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;

            var anotherUserId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var AnotherAccountId = Guid.NewGuid();

            // Adding 2 accounts to DB
            var db = _factory.GetDbContext();
            db.Accounts.Add(new Account
            {
                Id = accountId,
                UserId = Guid.Parse(userId),
                Balance = 1234
            });
            db.Accounts.Add(new Account
            {
                Id = AnotherAccountId,
                UserId = anotherUserId,
                Balance = 4321
            });
            await db.SaveChangesAsync();


            var response = await _client.GetAsync($"/api/v1/accounts?getAllAccounts=true");

            // check if request returns all accounts(should return all existed accounts)
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var accountsDto = JsonSerializer.Deserialize<List<AccountsDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            accountsDto.Should().NotBeNull();
            accountsDto.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetAllAccount_AsUser_ReturnsAllExistedAccounts()
        {
            var token = FakeTokenHelper.GenerateToken(Guid.NewGuid().ToString(), role: "User");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;

            var anotherUserId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var AnotherAccountId = Guid.NewGuid();

            // Adding 2 accounts to DB
            var db = _factory.GetDbContext();
            db.Accounts.Add(new Account
            {
                Id = accountId,
                UserId = Guid.Parse(userId),
                Balance = 1234
            });
            db.Accounts.Add(new Account
            {
                Id = AnotherAccountId,
                UserId = anotherUserId,
                Balance = 4321
            });
            await db.SaveChangesAsync();

            // check that getAllAccounts is not working for User role
            var response = await _client.GetAsync($"/api/v1/accounts?getAllAccounts=true");

            // check if request returns only user accounts(should be 1 here)
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var accountsDto = JsonSerializer.Deserialize<List<AccountsDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            accountsDto.Should().NotBeNull();
            accountsDto.Count.Should().Be(1);
        }

        [Fact]
        public async Task GetAccount_WithInvalidId_ReturnsBadRequest()
        {
            var token = FakeTokenHelper.GenerateToken(Guid.NewGuid().ToString(), role: "User");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;

            var response = await _client.GetAsync($"/api/v1/accounts/invalid-guid");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var problem = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            problem.Title.Should().Be("Bad Request");
        }


    }
}
