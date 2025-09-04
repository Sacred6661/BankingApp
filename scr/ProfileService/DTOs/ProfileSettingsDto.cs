
namespace ProfileService.DTOs
{
    public class ProfileSettingsDto
    {
        public Guid UserId { get; set; }
        public string Language { get; set; }
        public string Timezone { get; set; }
        public bool NotificationsEnabled { get; set; }
    }
}
