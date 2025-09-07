namespace ProfileService.DTOs
{
    public class TimezoneDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string UtcOffset { get; set; }
        public int? OffsetMinutes { get; set; }
    }
}
