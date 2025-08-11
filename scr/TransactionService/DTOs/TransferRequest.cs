namespace TransactionService.DTOs
{
    public class TransferRequest
    {
        public string FromAccountNumber { get; set; }
        public string ToAccountNumber { get; set; }
        public decimal Amount { get; set; }    
    }
}
