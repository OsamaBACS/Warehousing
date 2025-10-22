namespace Warehousing.Repo.Dtos
{
    public class ProductVariantDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public decimal? PriceAdjustment { get; set; } = 0;
        public decimal? CostAdjustment { get; set; } = 0;
        public decimal? StockQuantity { get; set; }
        public decimal? ReorderLevel { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        
        // Navigation properties
        public ProductDto? Product { get; set; }
        public ICollection<InventoryDto> Inventories { get; set; } = new List<InventoryDto>();
        public ICollection<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }
}

