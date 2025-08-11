namespace TransactionService.DTOs
{
    public class WithdrawRequest
    {
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
    }
}
