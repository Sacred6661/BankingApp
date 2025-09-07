using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProfileService.Data.Models
{
    public class ProfileSettings
    {
        [Key, ForeignKey(nameof(Profile))] // PK одночасно є FK
        public Guid UserId { get; set; }

        public int LanguageId { get; set; }

        public int TimeZoneId { get; set; }

        public bool NotificationsEnabled { get; set; } = true;

        public virtual Profile Profile { get; set; } = null!;
        public virtual Language Language { get; set; } = null!;
        public virtual Timezone Timezone { get; set; } = null!;
    }
}
