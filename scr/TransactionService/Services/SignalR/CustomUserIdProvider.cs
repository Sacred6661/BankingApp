using Microsoft.AspNetCore.SignalR;

namespace TransactionService.Services
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("user_id")?.Value;
        }
    }
}
