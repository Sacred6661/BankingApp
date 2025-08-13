namespace HistoryService.Data.Models
{
    public enum HistoryEventTypeEnum
    {
        TransactionCreated = 1,
        MoneyWithdraw = 2,
        MoneyDeposited = 3,
        TransactionCompleted = 4,
        Error = 5
    }
}
