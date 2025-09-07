using ProfileService.Data.Models;

namespace ProfileService.DTOs
{
    public class ProfileContactDto
    {
        public long Id { get; set; }

        public int ContactTypeId { get; set; }
        public string ContactTypeName { get; set; }

        public string Value { get; set; } = null!;

        public bool IsActive { get; set; }
    }
}
