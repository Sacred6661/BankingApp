namespace AccountService.DTOs
{
    public class CreateAccountRequest
    {
        public Guid? UserId { get; set; }
        public decimal InitialBalance { get; set; } = 0;
    }
}
