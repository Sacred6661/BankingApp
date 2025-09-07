using System.ComponentModel.DataAnnotations;

namespace ProfileService.Data.Models
{
    public class Timezone
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string UtcOffset { get; set; } = null;

        public int? OffsetMinutes { get; set; } = null;

        // 🔹 Навігаційна властивість (1-to-many)
        public ICollection<ProfileSettings> ProfileSettings { get; set; } = new List<ProfileSettings>();
    }
}
