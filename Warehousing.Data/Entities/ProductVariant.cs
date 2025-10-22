using System.ComponentModel.DataAnnotations;

namespace Warehousing.Data.Entities
{
    public class ProductVariant : BaseClass
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? Code { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        // Pricing
        public decimal? PriceAdjustment { get; set; } = 0; // Can be positive or negative
        public decimal? CostAdjustment { get; set; } = 0;
        
        // Inventory
        public decimal? StockQuantity { get; set; }
        public decimal? ReorderLevel { get; set; }
        
        // Status
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false; // Only one default per product
        
        // Display order
        public int DisplayOrder { get; set; } = 0;
        
        // Navigation properties
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}

