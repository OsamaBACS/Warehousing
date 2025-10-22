using System.ComponentModel.DataAnnotations;

namespace Warehousing.Data.Entities
{
    public class ProductModifier : BaseClass
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? Code { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        // Pricing
        public decimal PriceAdjustment { get; set; } = 0; // Can be positive or negative
        public decimal? CostAdjustment { get; set; } = 0;
        
        // Behavior
        public bool IsRequired { get; set; } = false;
        public bool IsMultiple { get; set; } = false; // Can select multiple options
        public int MaxSelections { get; set; } = 1;
        
        // Status
        public bool IsActive { get; set; } = true;
        
        // Display order
        public int DisplayOrder { get; set; } = 0;
        
        // Navigation properties
        public ICollection<ProductModifierOption> Options { get; set; } = new List<ProductModifierOption>();
        public ICollection<ProductModifierGroup> Groups { get; set; } = new List<ProductModifierGroup>();
    }
}

