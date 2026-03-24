using Microsoft.AspNetCore.SignalR;
using TransactionService.Hubs;

namespace TransactionService.Services
{
    public class SignalRNotifier : ISignalRNotifier
    {
        private readonly IHubContext<TransactionHub> _hub;

        public SignalRNotifier(IHubContext<TransactionHub> hub)
        {
            _hub = hub;
        }

        public Task NotifyTransactionUpdate(Guid transactionId, object data)
        {
            return _hub.Clients
                .Group(transactionId.ToString())
                .SendAsync("TransactionUpdated", data);
        }
    }
}
