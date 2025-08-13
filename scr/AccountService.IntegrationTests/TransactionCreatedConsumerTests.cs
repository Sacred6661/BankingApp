using System;
using System.Threading.Tasks;
using Xunit;
using MassTransit;
using FluentAssertions;
using Messaging;
using AccountService.Data;
using Microsoft.Extensions.DependencyInjection;
using AccountService.Data.Models;
using MassTransit.Testing;

namespace AccountService.IntegrationTests
{
    [Collection("Jwks server collection")]
    public class TransactionCreatedConsumerTests(GlobalJwksServer jwks) : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly GlobalJwksServer _jwks = jwks;

        [Fact]
        public async Task Consumer_Processes_Valid_TransactionCreated_Message_Deposit()
        {
            await using var factory = new CustomWebApplicationFactory(_jwks);
            var harness = factory.Services.GetTestHarness();

            await harness.Start(); // Run bus in memory

            var bus = factory.Services.GetRequiredService<IBus>();

            var message = new TransactionCreated
            {
                TransactionId = Guid.NewGuid().ToString(),
                AccountNumber = Guid.NewGuid().ToString(),
                Amount = "100",
                TransactionType = (int)TransactionTypeEnum.Deposit,
                PerformedBy = Guid.NewGuid().ToString(),
                TransactionStatus = (int)TransactionStatusEnum.Pending,
                Details = "Transaction created",
                PerformedByService = "TransactionService"
            };

            var db = factory.GetDbContext();
            db.Accounts.Add(new Account
            {
                Id = Guid.Parse(message.AccountNumber),
                UserId = Guid.Parse(message.PerformedBy),
                Balance = 0
            });
            await db.SaveChangesAsync();

            await bus.Publish(message);

            // waiting for consumer to consume message
            (await harness.Consumed.Any<TransactionCreated>()).Should().BeTrue();

            db = factory.GetDbContext();

            var account = await db.Accounts.FindAsync(Guid.Parse(message.AccountNumber));
            account.Balance.Should().Be(100);

            await harness.Stop();
        }

        [Fact]
        public async Task Consumer_Processes_Valid_TransactionCreated_Message_Withdraw()
        {
            await using var factory = new CustomWebApplicationFactory(_jwks);

            var harness = factory.Services.GetTestHarness();

            await harness.Start();

            var bus = factory.Services.GetRequiredService<IBus>();

            var message = new TransactionCreated
            {
                TransactionId = Guid.NewGuid().ToString(),
                AccountNumber = Guid.NewGuid().ToString(),
                Amount = "100",
                TransactionType = (int)TransactionTypeEnum.Withdraw,
                PerformedBy = Guid.NewGuid().ToString(),
                TransactionStatus = (int)TransactionStatusEnum.Pending,
                Details = "Transaction created",
                PerformedByService = "TransactionService"
            };

            var db = factory.GetDbContext();
            db.Accounts.Add(new Account
            {
                Id = Guid.Parse(message.AccountNumber),
                UserId = Guid.Parse(message.PerformedBy),
                Balance = 200
            });
            await db.SaveChangesAsync();

            await bus.Publish(message);

            (await harness.Consumed.Any<TransactionCreated>()).Should().BeTrue();

            db = factory.GetDbContext();

            var account = await db.Accounts.FindAsync(Guid.Parse(message.AccountNumber));
            account.Balance.Should().Be(100);

            await harness.Stop();
        }

        [Fact]
        public async Task Consumer_Processes_Valid_TransactionCreated_Message_Transfer()
        {
            await using var factory = new CustomWebApplicationFactory(_jwks);

            var harness = factory.Services.GetTestHarness();

            await harness.Start();

            var bus = factory.Services.GetRequiredService<IBus>();

            var message = new TransactionCreated
            {
                TransactionId = Guid.NewGuid().ToString(),
                AccountNumber = Guid.NewGuid().ToString(),
                RelatedAccountNumber = Guid.NewGuid().ToString(),
                Amount = "100",
                TransactionType = (int)TransactionTypeEnum.Withdraw,
                PerformedBy = Guid.NewGuid().ToString(),
                TransactionStatus = (int)TransactionStatusEnum.Pending,
                Details = "Transaction created",
                PerformedByService = "TransactionService"
            };

            var db = factory.GetDbContext();
            db.Accounts.Add(new Account
            {
                Id = Guid.Parse(message.AccountNumber),
                UserId = Guid.Parse(message.PerformedBy),
                Balance = 200
            });

            db.Accounts.Add(new Account
            {
                Id = Guid.Parse(message.RelatedAccountNumber),
                UserId = Guid.Parse(message.PerformedBy),
                Balance = 0
            });
            await db.SaveChangesAsync();

            await bus.Publish(message);

            (await harness.Consumed.Any<TransactionCreated>()).Should().BeTrue();

            db = factory.GetDbContext();

            var account = await db.Accounts.FindAsync(Guid.Parse(message.AccountNumber));
            account.Balance.Should().Be(100);

            var relatedAccount = await db.Accounts.FindAsync(Guid.Parse(message.AccountNumber));
            relatedAccount.Balance.Should().Be(100);

            await harness.Stop();
        }
    }
}
