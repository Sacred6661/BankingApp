using HistoryService.Data;
using HistoryService.Data.Models;
using MassTransit;
using Messaging;

namespace HistoryService.Consumers
{
    public class TransactionCreatedConsumer(HistoryContext dbContext) : IConsumer<TransactionCreated>
    {
        private readonly HistoryContext _dbContext = dbContext;

        public async Task Consume(ConsumeContext<TransactionCreated> context)
        {
            var msg = context.Message;

            var historyEvent = new HistoryEvent()
            {
                TransactionId = msg.TransactionId,
                Details = msg.Details,
                AccountNumber = msg.AccountNumber,
                Amount = decimal.Parse(msg.Amount),
                EventType = (int) HistoryEventTypeEnum.TransactionCreated,
                PerformedBy = msg.PerformedBy,
                PerformedByService = msg.PerformedByService,
                RelatedAccountNumber = msg.RelatedAccountNumber,
                TransactionStatus = msg.TransactionStatus,
                TransactionType = msg.TransactionType
            };

            await _dbContext.HistoryEvents.InsertOneAsync(historyEvent);
        }
    }
}
