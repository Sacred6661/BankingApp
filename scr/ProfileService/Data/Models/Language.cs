using System.ComponentModel.DataAnnotations;

namespace ProfileService.Data.Models
{
    public class Language
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        // navigation prop (1-to-many)
        public ICollection<ProfileSettings> ProfileSettings { get; set; } = new List<ProfileSettings>();
    }

}
