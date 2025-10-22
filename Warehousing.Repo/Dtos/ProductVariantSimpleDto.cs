namespace Warehousing.Repo.Dtos
{
    public class ProductVariantSimpleDto
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
    }
}
