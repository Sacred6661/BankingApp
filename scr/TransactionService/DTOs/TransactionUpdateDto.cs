using TransactionService.Data.Models;

namespace TransactionService.DTOs
{
    public class TransactionUpdateDto
    {
        public Guid TransactionId { get; set; }
        public TransactionTypeEnum TransactionType { get; set; }

        public string FromAccountNumber { get; set; }
        public decimal FromAccountBalance { get; set; }

        public string ToAccountNumber { get; set; }
        public decimal ToAccountBalance { get; set; }

        public decimal Amount { get; set; }
        public TransactionStatusEnum TransactionStatus { get; set; }
        public string Details { get; set; }
    }
}
