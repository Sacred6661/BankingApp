using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Transactions;

namespace TransactionService.Data.Models
{
    public class Transaction
    {
        [Key]
        public Guid TransactionId { get; set; }

        public int TransactionTypeId { get; set; }

        [ForeignKey(nameof(TransactionTypeId))]
        public TransactionType TransactionType { get; set; }

        [NotMapped]
        public TransactionTypeEnum TransactionTypeEnum
        {
            get => (TransactionTypeEnum)TransactionTypeId;
            set => TransactionTypeId = (int)value;
        }

        [Required]
        [MaxLength(50)]
        public string FromAccount { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ToAccount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? PerformedBy { get; set; }

        public int TransactionStatusId { get; set; }

        [ForeignKey(nameof(TransactionStatusId))]
        public TransactionType TransactionStatus { get; set; }

        [NotMapped]
        public TransactionStatusEnum TransactionStatusEnum
        {
            get => (TransactionStatusEnum)TransactionStatusId;
            set => TransactionStatusId = (int)value;
        }

    }
}
