namespace TransactionService.Services
{
    public interface ISignalRNotifier
    {
        Task NotifyTransactionUpdate(string userId, object data);
    }
}
