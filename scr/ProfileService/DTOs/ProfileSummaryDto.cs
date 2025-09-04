namespace ProfileService.DTOs
{
    public class ProfileSummaryDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
    }
}
