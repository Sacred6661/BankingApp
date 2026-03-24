using Microsoft.AspNetCore.SignalR;
using Nest;

namespace TransactionService.Hubs
{
    public class TransactionHub : Hub
    {
        public async Task SubscribeToTransaction(string transactionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, transactionId);
        }
    }
}
