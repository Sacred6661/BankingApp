namespace ProfileService.DTOs
{
    public class CountryDto
    {
        public int Id { get; set; }

        public string Alpha2Code { get; set; } = string.Empty; 
        public string Alpha3Code { get; set; } = string.Empty; 
        public int NumericCode { get; set; }                  
        public string Name { get; set; } = string.Empty;  
    }
}
