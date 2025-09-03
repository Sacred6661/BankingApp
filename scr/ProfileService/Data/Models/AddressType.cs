using System.ComponentModel.DataAnnotations;

namespace ProfileService.Data.Models
{
    public class AddressType
    {
        [Key]
        public int Id {  get; set; }
        public string TypeName {  get; set; }
        public string TypeDescription {  get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<ProfileAddress> ProfileAddresses { get; set; } = new List<ProfileAddress>();
    }
}
