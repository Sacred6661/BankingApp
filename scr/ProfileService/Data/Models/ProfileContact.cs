using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProfileService.Data.Models
{
    public class ProfileContact
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        // real FK for DB
        [Required]
        public int ContactTypeId { get; set; }

        // enum shortcut for code
        [NotMapped]
        public ContactTypeEnum ContactTypeEnum
        {
            get => (ContactTypeEnum)ContactTypeId;
            set => ContactTypeId = (int)value;
        }

        [Required, MaxLength(200)]
        public string Value { get; set; } = null!;

        [ForeignKey(nameof(ContactTypeId))]
        public virtual ContactType ContactType { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual Profile Profile { get; set; } = null!;
    }
}
