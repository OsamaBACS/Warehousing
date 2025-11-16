using System.ComponentModel.DataAnnotations;

namespace Warehousing.Data.Entities
{
    public class ProductModifierOption : BaseClass
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ModifierId { get; set; }
        public ProductModifier Modifier { get; set; } = null!;
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? Code { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        // Pricing
        public decimal PriceAdjustment { get; set; } = 0;
        public decimal? CostAdjustment { get; set; } = 0;
        
        // Status
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        
        // Display order
        public int DisplayOrder { get; set; } = 0;
        
        // Navigation properties
        public ICollection<OrderItemModifier> OrderItemModifiers { get; set; } = new List<OrderItemModifier>();
    }
}













