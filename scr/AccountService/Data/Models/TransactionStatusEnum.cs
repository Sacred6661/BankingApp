using Microsoft.AspNetCore.Http.HttpResults;

namespace AccountService.Data.Models
{
    public enum TransactionStatusEnum
    {
        Pending = 1,
        Accepted = 2,
        Rejected = 3
    }
}
