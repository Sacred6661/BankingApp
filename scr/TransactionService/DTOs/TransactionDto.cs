using TransactionService.Data.Models;

namespace TransactionService.DTOs
{
    public class TransactionDto
    {
        public Guid TransactionId { get; set; }

        public TransactionTypeEnum TransactionType { get; set; }

        public string FromAccount { get; set; } = string.Empty;

        public string? ToAccount { get; set; }

        public decimal Amount { get; set; }

        public DateTime Timestamp { get; set; }

        public string? PerformedBy { get; set; }

        public TransactionStatusEnum TransactionStatus { get; set; }
    }
}
