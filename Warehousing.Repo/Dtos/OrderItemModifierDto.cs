namespace Warehousing.Repo.Dtos
{
    public class OrderItemModifierDto
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public int ModifierOptionId { get; set; }
        public decimal PriceAdjustment { get; set; } = 0;
        public decimal? CostAdjustment { get; set; } = 0;
        public int Quantity { get; set; } = 1;
        public string? Notes { get; set; }
        
        // Navigation properties
        public OrderItemDto? OrderItem { get; set; }
        public ProductModifierOptionDto? ModifierOption { get; set; }
    }
}









