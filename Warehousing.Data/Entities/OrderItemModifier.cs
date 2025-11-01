using System.ComponentModel.DataAnnotations;

namespace Warehousing.Data.Entities
{
    public class OrderItemModifier : BaseClass
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; } = null!;
        
        [Required]
        public int ModifierOptionId { get; set; }
        public ProductModifierOption ModifierOption { get; set; } = null!;
        
        // Pricing
        public decimal PriceAdjustment { get; set; } = 0;
        public decimal? CostAdjustment { get; set; } = 0;
        
        // Quantity (for modifiers that can be added multiple times)
        public int Quantity { get; set; } = 1;
        
        // Notes
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}









