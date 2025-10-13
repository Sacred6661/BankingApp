using System.ComponentModel.DataAnnotations;

namespace ProfileService.Data.Models
{
    public class ContactType
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string TypeName { get; set; } = null!; // "Email", "Phone"

        [MaxLength(200)]
        public string TypeDescription { get; set; }

        public bool IsActive { get; set; } = true;

        public string Code { get; set; } = null!;
        public string RegexPattern { get; set; }

        public virtual ICollection<ProfileContact> ProfileContacts { get; set; } = new List<ProfileContact>();
    }
}
