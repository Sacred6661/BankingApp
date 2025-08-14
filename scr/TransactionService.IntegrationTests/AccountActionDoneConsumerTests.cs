using FluentAssertions;
using MassTransit.Testing;
using Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.Data;
using TransactionService.Data.Models;

namespace TransactionService.IntegrationTests
{
    [Collection("Jwks server collection")]
    public class AccountActionDoneConsumerTests(GlobalJwksServer _jwks) : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly string _userId = Guid.NewGuid().ToString();
        private readonly string _accountNumber = Guid.NewGuid().ToString();

        [Fact]
        public async Task Consumer_ShouldUpdateTransactionStatusToAccepted_AndSendTransactionCompleted()
        {
            var factory = new CustomWebApplicationFactory(_jwks);
            var harness = factory.Services.GetTestHarness();

            try
            {
                var db = factory.GetDbContext();

                var tx = new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    TransactionTypeEnum = TransactionTypeEnum.Deposit,
                    TransactionStatusEnum = TransactionStatusEnum.Pending,
                    Amount = 50,
                    ToAccount = "ACC123",
                    PerformedBy = "user123"
                };
                db.Transactions.Add(tx);
                db.SaveChanges();

                var msg = new AccountActionDone
                {
                    TransactionId = tx.TransactionId.ToString(),
                    AccountNumber = _accountNumber,
                    Amount = "50",
                    PerformedBy = _userId,
                    TransactionStatus = (int)TransactionStatusEnum.Accepted,
                    TransactionType = (int)TransactionTypeEnum.Deposit
                };

                var endpoint = await harness.Bus.GetSendEndpoint(new Uri("queue:account-action-done"));
                await endpoint.Send(msg);

                (await harness.Consumed.Any<AccountActionDone>()).Should().BeTrue();
                (await harness.Sent.Any<TransactionCompleted>()).Should().BeTrue();

                db = factory.GetDbContext();
                var updatedTx = db.Transactions.Find(tx.TransactionId);
                updatedTx.TransactionStatusEnum.Should().Be(TransactionStatusEnum.Accepted);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Fact]
        public async Task Consumer_ShouldUpdateTransactionStatusToRejected_WhenStatusIsNotAccepted()
        {
            var factory = new CustomWebApplicationFactory(_jwks);
            var harness = factory.Services.GetTestHarness();

            try
            {
                var db = factory.GetDbContext();

                var tx = new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    TransactionTypeEnum = TransactionTypeEnum.Deposit,
                    TransactionStatusEnum = TransactionStatusEnum.Pending,
                    Amount = 50,
                    ToAccount = _accountNumber,
                    PerformedBy = _userId
                };
                db.Transactions.Add(tx);
                db.SaveChanges();

                var msg = new AccountActionDone
                {
                    TransactionId = tx.TransactionId.ToString(),
                    AccountNumber = _accountNumber,
                    Amount = "50",
                    PerformedBy = _userId,
                    TransactionStatus = (int)TransactionStatusEnum.Rejected,
                    TransactionType = (int)TransactionTypeEnum.Deposit,
                    Details = "Insufficient funds"
                };

                var endpoint = await harness.Bus.GetSendEndpoint(new Uri("queue:account-action-done"));
                await endpoint.Send(msg);

                (await harness.Consumed.Any<AccountActionDone>()).Should().BeTrue();
                (await harness.Sent.Any<TransactionCompleted>()).Should().BeTrue();

                db = factory.GetDbContext();
                var updatedTx = db.Transactions.Find(tx.TransactionId);
                updatedTx.TransactionStatusEnum.Should().Be(TransactionStatusEnum.Rejected);
            }
            finally
            {
                await harness.Stop();
            }
        }
    }
}
