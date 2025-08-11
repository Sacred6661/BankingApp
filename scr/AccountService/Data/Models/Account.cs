namespace AccountService.Data.Models
{
    public class Account
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; } // from AuthService
        public decimal Balance { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
