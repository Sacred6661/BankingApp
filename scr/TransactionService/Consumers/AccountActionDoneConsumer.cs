using MassTransit;
using Messaging;
using System.Transactions;
using TransactionService.Data;
using TransactionService.Data.Models;
using TransactionService.DTOs;
using TransactionService.Services;

namespace TransactionService.Consumers
{
    public class AccountActionDoneConsumer(TransactionsDbContext dbContext, ISendEndpointProvider sendProvider,
        ISignalRNotifier notifier) : IConsumer<AccountActionDone>
    {
        private readonly TransactionsDbContext _dbContext = dbContext;
        private readonly ISendEndpointProvider _sendProvider = sendProvider;

        public async Task Consume(ConsumeContext<AccountActionDone> context)
        {
            var msg = context.Message;

            var sendEndpointHistory = await _sendProvider.GetSendEndpoint(new Uri("queue:history-transaction-completed"));

            var transactionId = Guid.Parse(msg.TransactionId);
            var transaction = _dbContext.Transactions.Where(t => t.TransactionId == transactionId)?.FirstOrDefault();

            var transactionResult = new TransactionCompleted()
            {
                AccountNumber = msg.AccountNumber,
                Amount = msg.Amount,
                Details = "Transaction Completed and accepted",
                PerformedBy = msg.PerformedBy,
                PerformedByService = "TransactionService",
                RelatedAccountNumber = msg.RelatedAccountNumber,
                TransactionId = msg.TransactionId,
                TransactionStatus = msg.TransactionStatus,
                TransactionType = msg.TransactionType
            };

            var transactionUpdateData = new TransactionUpdateDto
            {
                TransactionId = transactionId,
                FromAccountNumber = msg.AccountNumber,
                FromAccountBalance = decimal.Parse(msg.AccountBalance),
                ToAccountNumber = msg.RelatedAccountNumber,
                ToAccountBalance = decimal.Parse(msg.RelatedAccountBalance),
                Amount = decimal.Parse(msg.Amount),
                TransactionType = (TransactionTypeEnum)msg.TransactionType,
                TransactionStatus = (TransactionStatusEnum)msg.TransactionStatus,
                Details = msg.Details
            };

            await notifier.NotifyTransactionUpdate(msg?.UserId, transactionUpdateData);


            if (transaction != null && msg.TransactionStatus == (int)TransactionStatusEnum.Accepted)
            {
                transaction.TransactionStatusEnum = TransactionStatusEnum.Accepted;
                await _dbContext.SaveChangesAsync();

                await sendEndpointHistory.Send(transactionResult);
                return;
            }

            transactionResult.Details = $"Transaction rejected: {msg.Details}";
            transaction.TransactionStatusEnum = TransactionStatusEnum.Rejected;
            await _dbContext.SaveChangesAsync();

            await sendEndpointHistory.Send(transactionResult);
        }
    }
}
