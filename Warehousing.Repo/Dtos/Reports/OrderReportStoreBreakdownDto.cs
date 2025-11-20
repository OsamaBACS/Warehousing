namespace Warehousing.Repo.Dtos.Reports
{
    public class OrderReportStoreBreakdownDto
    {
        public int StoreId { get; set; }
        public string StoreNameAr { get; set; } = string.Empty;
        public string? StoreNameEn { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal TotalAmount { get; set; }
    }
}

