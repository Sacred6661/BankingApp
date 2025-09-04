using ProfileService.Data.Models;

namespace ProfileService.DTOs
{
    public class ProfileAddressDto
    {
        public long Id { get; set; }

        public int AddressTypeId { get; set; }
        public string AddessTypeName { get; set; }
 
        public string AddressLine { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
    }
}
