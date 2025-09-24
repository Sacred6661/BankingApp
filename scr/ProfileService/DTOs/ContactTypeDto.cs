namespace ProfileService.DTOs
{
    public class ContactTypeDto
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public string TypeDescription { get; set; }
        public bool IsActive { get; set; }
    }
}
