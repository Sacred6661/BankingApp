using HistoryService.Data.Models;
using HistoryService.Data;
using MassTransit;
using Messaging;

namespace HistoryService.Consumers
{
    public class AccountActionDoneConsumer(HistoryContext dbContext) : IConsumer<AccountActionDone>
    {
        private readonly HistoryContext _dbContext = dbContext;

        public async Task Consume(ConsumeContext<AccountActionDone> context)
        {
            var msg = context.Message;

            var historyEvent = new HistoryEvent()
            {
                TransactionId = msg.TransactionId,
                Details = msg.Details,
                AccountNumber = msg.AccountNumber,
                Amount = decimal.Parse(msg.Amount),
                TransactionType = msg.TransactionType,
                PerformedBy = msg.PerformedBy,
                PerformedByService = msg.PerformedByService,
                RelatedAccountNumber = msg.RelatedAccountNumber,
                TransactionStatus = msg.TransactionStatus
            };

            if (msg.TransactionType == (int)TransactionTypeEnum.Deposit)
                historyEvent.EventType = (int)HistoryEventTypeEnum.MoneyDeposited;
            if (msg.TransactionType == (int)TransactionTypeEnum.Withdraw)
                historyEvent.EventType = (int)HistoryEventTypeEnum.MoneyWithdraw;

            if(msg.IsError)
                historyEvent.EventType = (int)HistoryEventTypeEnum.Error;

            await _dbContext.HistoryEvents.InsertOneAsync(historyEvent);
        }
    }
}
