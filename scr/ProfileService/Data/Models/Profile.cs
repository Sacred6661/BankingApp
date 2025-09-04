using System.ComponentModel.DataAnnotations;

namespace ProfileService.Data.Models
{
    public class Profile
    {
        [Key]
        public Guid UserId { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [MaxLength(100)]
        public string LastName { get; set; } = null!;

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        public DateTime? Birthday { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Навігація
        public virtual ICollection<ProfileContact> Contacts { get; set; } = new List<ProfileContact>();
        public virtual ICollection<ProfileAddress> Addresses { get; set; } = new List<ProfileAddress>();
        public virtual ProfileSettings Settings { get; set; }
    }
}
