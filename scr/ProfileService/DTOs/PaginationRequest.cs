namespace ProfileService.DTOs
{
    public class PaginationRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "asc"; // або "desc"
    }
}
