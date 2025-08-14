using FluentAssertions;
using MassTransit.Middleware.Outbox;
using MassTransit.Testing;
using Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TransactionService.Data;
using TransactionService.Data.Models;
using TransactionService.DTOs;

namespace TransactionService.IntegrationTests
{
    [Collection("Jwks server collection")]
    public class TransactionControllerTests(GlobalJwksServer jwks) : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly GlobalJwksServer _jwks = jwks;

        private HttpClient CreateAuthorizedClient(out string userId, out CustomWebApplicationFactory factory)
        {
            userId = Guid.NewGuid().ToString();
            var role = "User";

            factory = new CustomWebApplicationFactory(_jwks);
            var client = factory.CreateClient();
            var token = FakeTokenHelper.GenerateToken(userId, role);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        [Fact]
        public async Task Deposit_ShouldCreateTransaction_AndPublishEvent()
        {
            var client = CreateAuthorizedClient(out var userId, out var factory);
            var accountNumber = Guid.NewGuid().ToString();
            decimal amount = 100;

            var request = new DepositRequest { AccountNumber = accountNumber, Amount = amount };

            var response = await client.PostAsJsonAsync("/api/v1/transactions/deposit", request);
            response.EnsureSuccessStatusCode();

            var transaction = await response.Content.ReadFromJsonAsync<TransactionDto>();
            transaction.Should().NotBeNull();
            transaction!.Amount.Should().Be(amount);
            transaction.TransactionStatus.Should().Be(TransactionStatusEnum.Pending);
            transaction.ToAccount.Should().Be(accountNumber);
            transaction.PerformedBy.Should().Be(userId);

            var harness = factory.Services.GetTestHarness();
            (await harness.Published.Any<TransactionCreated>()).Should().BeTrue();
        }

        [Fact]
        public async Task Deposit_ShouldReturnBadRequest_WhenAmountIsZeroOrNegative()
        {
            var client = CreateAuthorizedClient(out var userId, out var factory);
            var accountNumber = Guid.NewGuid().ToString();

            var request = new DepositRequest { AccountNumber = accountNumber, Amount = 0 };
            var response = await client.PostAsJsonAsync("/api/v1/transactions/deposit", request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Withdraw_ShouldCreateTransaction_AndPublishEvent()
        {
            var client = CreateAuthorizedClient(out var userId, out var factory);
            var accountNumber = Guid.NewGuid().ToString();

            var request = new WithdrawRequest { AccountNumber = accountNumber, Amount = 50 };
            var response = await client.PostAsJsonAsync("/api/v1/transactions/withdraw", request);
            response.EnsureSuccessStatusCode();

            var transaction = await response.Content.ReadFromJsonAsync<TransactionDto>();
            transaction.Should().NotBeNull();
            transaction!.TransactionType.Should().Be(TransactionTypeEnum.Withdraw);
            transaction.FromAccount.Should().Be(accountNumber);
            transaction.PerformedBy.Should().Be(userId);

            var harness = factory.Services.GetTestHarness();
            (await harness.Published.Any<TransactionCreated>()).Should().BeTrue();
        }

        [Fact]
        public async Task Withdraw_ShouldReturnBadRequest_WhenAmountIsZeroOrNegative()
        {
            var client = CreateAuthorizedClient(out var userId, out var factory);
            var accountNumber = Guid.NewGuid().ToString();

            var request = new WithdrawRequest { AccountNumber = accountNumber, Amount = -5 };
            var response = await client.PostAsJsonAsync("/api/v1/transactions/withdraw", request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }


        [Fact]
        public async Task Transfer_ShouldCreateTransaction_AndPublishEvent()
        {
            var client = CreateAuthorizedClient(out var userId, out var factory);
            var accountNumber = Guid.NewGuid().ToString();
            var relatedAccountNumber = Guid.NewGuid().ToString();

            var request = new TransferRequest
            {
                FromAccountNumber = accountNumber,
                ToAccountNumber = relatedAccountNumber,
                Amount = 200
            };

            var response = await client.PostAsJsonAsync("/api/v1/transactions/transfer", request);
            response.EnsureSuccessStatusCode();

            var transaction = await response.Content.ReadFromJsonAsync<TransactionDto>();
            transaction.Should().NotBeNull();
            transaction.TransactionType.Should().Be(TransactionTypeEnum.Transfer);
            transaction.FromAccount.Should().Be(accountNumber);
            transaction.ToAccount.Should().Be(relatedAccountNumber);

            var harness = factory.Services.GetTestHarness();
            (await harness.Published.Any<TransactionCreated>()).Should().BeTrue();
        }

        [Fact]
        public async Task Transfer_ShouldReturnBadRequest_WhenAmountIsZeroOrNegative()
        {
            var client = CreateAuthorizedClient(out var userId, out var factory);
            var accountNumber = Guid.NewGuid().ToString();
            var relatedAccountNumber = Guid.NewGuid().ToString();

            var request = new TransferRequest { FromAccountNumber = accountNumber, ToAccountNumber = relatedAccountNumber, Amount = 0 };
            var response = await client.PostAsJsonAsync("/api/v1/transactions/transfer", request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Transfer_ShouldReturnBadRequest_WhenDestinationAccountIsMissing()
        {
            var client = CreateAuthorizedClient(out var userId, out var factory);
            var accountNumber = Guid.NewGuid().ToString();

            var request = new TransferRequest { FromAccountNumber = accountNumber, Amount = 100 };
            var response = await client.PostAsJsonAsync("/api/v1/transactions/transfer", request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetTransaction_ShouldReturnTransaction_WhenExists()
        {
            var client = CreateAuthorizedClient(out var userId, out var factory);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();

            var tx = new Transaction
            {
                TransactionId = Guid.NewGuid(),
                TransactionTypeEnum = TransactionTypeEnum.Deposit,
                TransactionStatusEnum = TransactionStatusEnum.Pending,
                Amount = 10,
                ToAccount = Guid.NewGuid().ToString(),
                PerformedBy = userId
            };
            db.Transactions.Add(tx);
            db.SaveChanges();

            var response = await client.GetAsync($"/api/v1/transactions/{tx.TransactionId}");
            response.EnsureSuccessStatusCode();

            var transaction = await response.Content.ReadFromJsonAsync<TransactionDto>();
            transaction.Should().NotBeNull();
            transaction.PerformedBy.Should().Be(userId);
            transaction!.TransactionId.Should().Be(tx.TransactionId);
        }

        [Fact]
        public async Task GetTransaction_ShouldReturnNotFound_WhenNotExists()
        {
            var client = CreateAuthorizedClient(out var userId, out var factory);
            var response = await client.GetAsync($"/api/v1/transactions/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAll_ShouldReturnListOfTransactions()
        {
            var client = CreateAuthorizedClient(out var userId, out var factory);
            var response = await client.GetAsync("/api/v1/transactions");
            response.EnsureSuccessStatusCode();

            var transactions = await response.Content.ReadFromJsonAsync<TransactionDto[]>();
            transactions.Should().NotBeNull();
        }
    }
}
