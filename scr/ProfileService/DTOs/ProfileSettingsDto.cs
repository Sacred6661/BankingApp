
namespace ProfileService.DTOs
{
    public class ProfileSettingsDto
    {
        public Guid UserId { get; set; }
        public int? LanguageId { get; set; }
        public string LanguageName { get; set; }
        public int? TimezoneId { get; set; }
        public string TimezoneName { get; set; }
        public bool NotificationsEnabled { get; set; }
    }
}
