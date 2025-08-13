using History.IntegrationTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MassTransit.Testing;
using MassTransit;
using Messaging;
using Microsoft.Extensions.DependencyInjection;
using HistoryService.Data.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace HistoryService.IntegrationTests
{
    [Collection("Jwks server collection")]
    public class ConsumersTests(GlobalJwksServer jwks) : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly GlobalJwksServer _jwks = jwks;

        [Fact]
        public async Task Consumer_Processes_Valid_TransactionCreated_Message()
        {
            var userId = Guid.NewGuid().ToString();
            var transactionId = Guid.NewGuid().ToString();
            var accountNumber = Guid.NewGuid().ToString();

            await using var factory = new CustomWebApplicationFactory(_jwks);
            var harness = factory.Services.GetTestHarness();

            await harness.Start(); // Run bus in memory

            var bus = factory.Services.GetRequiredService<IBus>();

            var message = new TransactionCreated
            {
                TransactionId = transactionId,
                AccountNumber = accountNumber,
                Amount = "100",
                TransactionType = (int)TransactionTypeEnum.Deposit,
                PerformedBy = userId,
                TransactionStatus = (int)TransactionStatusEnum.Pending,
                Details = "Transaction created",
                PerformedByService = "TransactionService"
            };

            await bus.Publish(message);

            // waiting for consumer to consume message
            (await harness.Consumed.Any<TransactionCreated>()).Should().BeTrue();

            var db = factory.GetDbContext();
            var events = await db.HistoryEvents.Find(e => e.TransactionId == transactionId).ToListAsync();
            events.Should().NotBeNull();
            events.Count.Should().Be(1);

            var oneEvent = events?.FirstOrDefault();
            oneEvent.Should().NotBeNull();
            oneEvent.EventType.Should().Be((int)HistoryEventTypeEnum.TransactionCreated);
            oneEvent.TransactionType.Should().Be(message.TransactionType);
            oneEvent.TransactionStatus.Should().Be(message.TransactionStatus);
            oneEvent.PerformedBy.Should().Be(message.PerformedBy);
            oneEvent.AccountNumber.Should().Be(message.AccountNumber);
            oneEvent.TransactionId.Should().Be(message.TransactionId);
        }

        [Fact]
        public async Task Consumer_Processes_Valid_TransactionCompleted_Message()
        {
            var userId = Guid.NewGuid().ToString();
            var transactionId = Guid.NewGuid().ToString();
            var accountNumber = Guid.NewGuid().ToString();

            await using var factory = new CustomWebApplicationFactory(_jwks);
            var harness = factory.Services.GetTestHarness();

            await harness.Start(); // Run bus in memory

            var bus = factory.Services.GetRequiredService<IBus>();

            var message = new TransactionCompleted
            {
                TransactionId = transactionId,
                AccountNumber = accountNumber,
                Amount = "100",
                TransactionType = (int)TransactionTypeEnum.Deposit,
                PerformedBy = userId,
                TransactionStatus = (int)TransactionStatusEnum.Accepted,
                Details = "Transaction Completed and accepted",
                PerformedByService = "TransactionService"
            };

            await bus.Publish(message);

            // waiting for consumer to consume message
            (await harness.Consumed.Any<TransactionCompleted>()).Should().BeTrue();

            var db = factory.GetDbContext();
            var events = await db.HistoryEvents.Find(e => e.TransactionId == transactionId).ToListAsync();
            events.Should().NotBeNull();
            events.Count.Should().Be(1);

            var oneEvent = events?.FirstOrDefault();
            oneEvent.Should().NotBeNull();
            oneEvent.EventType.Should().Be((int)HistoryEventTypeEnum.TransactionCompleted);
            oneEvent.TransactionType.Should().Be(message.TransactionType);
            oneEvent.TransactionStatus.Should().Be(message.TransactionStatus);
            oneEvent.PerformedBy.Should().Be(message.PerformedBy);
            oneEvent.AccountNumber.Should().Be(message.AccountNumber);
            oneEvent.TransactionId.Should().Be(message.TransactionId);
        }

        [Fact]
        public async Task Consumer_Processes_Valid_AccountActionDone_Deposit_Message()
        {
            var userId = Guid.NewGuid().ToString();
            var transactionId = Guid.NewGuid().ToString();
            var accountNumber = Guid.NewGuid().ToString();

            await using var factory = new CustomWebApplicationFactory(_jwks);
            var harness = factory.Services.GetTestHarness();

            await harness.Start(); // Run bus in memory

            var bus = factory.Services.GetRequiredService<IBus>();

            var message = new AccountActionDone
            {
                TransactionId = transactionId,
                AccountNumber = accountNumber,
                Amount = "100",
                TransactionType = (int)TransactionTypeEnum.Deposit,
                PerformedBy = userId,
                TransactionStatus = (int)TransactionStatusEnum.Accepted,
                Details = $"Deposit to the account {accountNumber}",
                PerformedByService = "AccountService"
            };

            await bus.Publish(message);

            // waiting for consumer to consume message
            (await harness.Consumed.Any<AccountActionDone>()).Should().BeTrue();

            var db = factory.GetDbContext();
            var events = await db.HistoryEvents.Find(e => e.TransactionId == transactionId).ToListAsync();
            events.Should().NotBeNull();
            events.Count.Should().Be(1);

            var oneEvent = events?.FirstOrDefault();
            oneEvent.Should().NotBeNull();
            oneEvent.EventType.Should().Be((int)HistoryEventTypeEnum.MoneyDeposited);
            oneEvent.TransactionType.Should().Be(message.TransactionType);
            oneEvent.TransactionStatus.Should().Be(message.TransactionStatus);
            oneEvent.PerformedBy.Should().Be(message.PerformedBy);
            oneEvent.AccountNumber.Should().Be(message.AccountNumber);
            oneEvent.TransactionId.Should().Be(message.TransactionId);
            oneEvent.PerformedByService.Should().Be(message.PerformedByService);
        }


        [Fact]
        public async Task Consumer_Processes_Valid_AccountActionDone_Withdraw_Message()
        {
            var userId = Guid.NewGuid().ToString();
            var transactionId = Guid.NewGuid().ToString();
            var accountNumber = Guid.NewGuid().ToString();

            await using var factory = new CustomWebApplicationFactory(_jwks);
            var harness = factory.Services.GetTestHarness();

            await harness.Start(); // Run bus in memory

            var bus = factory.Services.GetRequiredService<IBus>();

            var message = new AccountActionDone
            {
                TransactionId = transactionId,
                AccountNumber = accountNumber,
                Amount = "100",
                TransactionType = (int)TransactionTypeEnum.Withdraw,
                PerformedBy = userId,
                TransactionStatus = (int)TransactionStatusEnum.Accepted,
                Details = $"Withdraw from the account {accountNumber}",
                PerformedByService = "AccountService"
            };

            await bus.Publish(message);

            // waiting for consumer to consume message
            (await harness.Consumed.Any<AccountActionDone>()).Should().BeTrue();

            var db = factory.GetDbContext();
            var events = await db.HistoryEvents.Find(e => e.TransactionId == transactionId).ToListAsync();
            events.Should().NotBeNull();
            events.Count.Should().Be(1);

            var oneEvent = events?.FirstOrDefault();
            oneEvent.Should().NotBeNull();
            oneEvent.EventType.Should().Be((int)HistoryEventTypeEnum.MoneyWithdraw);
            oneEvent.TransactionType.Should().Be(message.TransactionType);
            oneEvent.TransactionStatus.Should().Be(message.TransactionStatus);
            oneEvent.PerformedBy.Should().Be(message.PerformedBy);
            oneEvent.AccountNumber.Should().Be(message.AccountNumber);
            oneEvent.TransactionId.Should().Be(message.TransactionId);
            oneEvent.PerformedByService.Should().Be(message.PerformedByService);
        }
    }
}