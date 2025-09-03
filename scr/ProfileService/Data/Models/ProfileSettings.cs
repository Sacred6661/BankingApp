using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProfileService.Data.Models
{
    public class ProfileSettings
    {
        [Key, ForeignKey(nameof(Profile))] // PK одночасно є FK
        public Guid UserId { get; set; }

        [Required, MaxLength(10)]
        public string Language { get; set; } = "en";

        [Required, MaxLength(50)]
        public string Timezone { get; set; } = "UTC";

        public bool NotificationsEnabled { get; set; } = true;

        public virtual Profile Profile { get; set; } = null!;
    }
}
