using HistoryService.Data.Models;
using HistoryService.Data;
using MassTransit;
using Messaging;

namespace HistoryService.Consumers
{
    public class TransactionCompletedConsumer(HistoryContext dbContext) : IConsumer<TransactionCompleted>
    {
        private readonly HistoryContext _dbContext = dbContext;

        public async Task Consume(ConsumeContext<TransactionCompleted> context)
        {
            var msg = context.Message;

            var historyEvent = new HistoryEvent()
            {
                TransactionId = msg.TransactionId,
                Details = msg.Details,
                AccountNumber = msg.AccountNumber,
                Amount = decimal.Parse(msg.Amount),
                EventType = (int)HistoryEventTypeEnum.TransactionCompleted,
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
