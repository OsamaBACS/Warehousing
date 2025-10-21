namespace Warehousing.Repo.Dtos
{
    public class StoreTransferItemDto
    {
        public int Id { get; set; }
        public int TransferId { get; set; }
        
        public int ProductId { get; set; }
        public string ProductNameAr { get; set; } = string.Empty;
        public string? ProductNameEn { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Audit fields
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; } = string.Empty;
    }
}

