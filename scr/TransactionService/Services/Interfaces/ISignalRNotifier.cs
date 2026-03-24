namespace TransactionService.Services
{
    public interface ISignalRNotifier
    {
        Task NotifyTransactionUpdate(Guid transactionId, object data);
    }
}
