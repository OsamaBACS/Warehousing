namespace Warehousing.Repo.Dtos
{
    public class ProductModifierOptionDto
    {
        public int Id { get; set; }
        public int ModifierId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public decimal PriceAdjustment { get; set; } = 0;
        public decimal? CostAdjustment { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        
        // Navigation properties
        public ProductModifierDto? Modifier { get; set; }
        public ICollection<OrderItemModifierDto> OrderItemModifiers { get; set; } = new List<OrderItemModifierDto>();
    }
}











