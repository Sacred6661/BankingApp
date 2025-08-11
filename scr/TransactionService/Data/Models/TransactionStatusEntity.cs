using System.ComponentModel.DataAnnotations;

namespace TransactionService.Data.Models
{
    public class TransactionStatusEntity
    {
        [Key]
        public int TransactionStatusId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}
