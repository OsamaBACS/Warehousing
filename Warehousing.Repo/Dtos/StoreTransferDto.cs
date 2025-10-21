namespace Warehousing.Repo.Dtos
{
    public class StoreTransferDto
    {
        public int Id { get; set; }
        public DateTime TransferDate { get; set; }
        public string Notes { get; set; } = string.Empty;

        // From/To stores
        public int FromStoreId { get; set; }
        public string FromStoreNameAr { get; set; } = string.Empty;
        public string? FromStoreNameEn { get; set; } = string.Empty;
        public string? FromStoreCode { get; set; } = string.Empty;

        public int ToStoreId { get; set; }
        public string ToStoreNameAr { get; set; } = string.Empty;
        public string? ToStoreNameEn { get; set; } = string.Empty;
        public string? ToStoreCode { get; set; } = string.Empty;

        // Status
        public int StatusId { get; set; }
        public string StatusNameAr { get; set; } = string.Empty;
        public string? StatusNameEn { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;

        // Audit fields
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; } = string.Empty;

        // Items
        public List<StoreTransferItemDto> Items { get; set; } = new List<StoreTransferItemDto>();
    }
}



