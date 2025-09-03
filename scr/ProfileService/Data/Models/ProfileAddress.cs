using ProfileService.Data.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class ProfileAddress
{
    [Key]
    public long Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    // FK property, EF local real use
    [Required]
    public int AddressTypeId { get; set; }

    // Enum-shortcut, to use enum in code
    [NotMapped]
    public AddressTypeEnum AddressTypeEnum
    {
        get => (AddressTypeEnum)AddressTypeId;
        set => AddressTypeId = (int)value;
    }

    [ForeignKey(nameof(AddressTypeId))]
    public virtual AddressType AddressType { get; set; } = null!;

    [Required, MaxLength(500)]
    public string AddressLine { get; set; } = null!;

    [Required, MaxLength(100)]
    public string City { get; set; } = null!;

    [Required, MaxLength(20)]
    public string ZipCode { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Country { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual Profile Profile { get; set; } = null!;
}
