using System.ComponentModel.DataAnnotations;

namespace ProfileService.Data.Models
{
    public class Country
    {
        [Key]
        public int Id { get; set; }

        public string Alpha2Code { get; set; } = string.Empty; // "UA"
        public string Alpha3Code { get; set; } = string.Empty; // "UKR"
        public int NumericCode { get; set; }                  // 804
        public string Name { get; set; } = string.Empty;      // "Ukraine"

        public ICollection<ProfileAddress> Addresses { get; set; } = new List<ProfileAddress>();
    }
}
