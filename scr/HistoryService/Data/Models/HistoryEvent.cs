using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace HistoryService.Data.Models
{
    public class HistoryEvent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string EventId { get; set; }

        public string TransactionId { get; set; }
        public string AccountNumber { get; set; }

        [BsonRepresentation(BsonType.Int32)]
        public int EventType { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int? TransactionStatus { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int? TransactionType { get; set; }

        public decimal Amount { get; set; }
        public string RelatedAccountNumber { get; set; } 
        public string PerformedBy { get; set; }
        public string PerformedByService { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
