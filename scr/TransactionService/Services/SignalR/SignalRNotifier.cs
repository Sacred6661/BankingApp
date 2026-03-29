using Microsoft.AspNetCore.SignalR;
using TransactionService.Hubs;

namespace TransactionService.Services
{
    public class SignalRNotifier(IHubContext<TransactionHub> hub) : ISignalRNotifier
    {
        public Task NotifyTransactionUpdate(string userId, object data)
        {
            return hub.Clients.User(userId).SendAsync("TransactionUpdated", data);
        }
    }
}
