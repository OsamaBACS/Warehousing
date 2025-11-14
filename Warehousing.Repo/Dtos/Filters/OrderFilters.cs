namespace Warehousing.Repo.Dtos.Filters
{
    public class OrderFilters
    {
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 0;
        public string? SearchTerm { get; set; }
        public DateTime? OrderDate { get; set; }
        public int? OrderTypeId { get; set; }
        public int? CustomerId { get; set; }
        public int? SupplierId { get; set; }
        public int? StatusId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }
}