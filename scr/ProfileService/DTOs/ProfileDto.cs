
namespace ProfileService.DTOs
{
    public class ProfileDto
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string FullName { get { return $"{FirstName} {LastName}"; } }
        public string AvatarUrl { get; set; }
        public string Email { get; set; }
        public DateTime? Birthday { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public IEnumerable<ProfileContactDto> Contacts { get; set; }
        public IEnumerable<ProfileAddressDto> Addresses { get; set; }
        public ProfileSettingsDto Settings { get; set; }
    }
}
