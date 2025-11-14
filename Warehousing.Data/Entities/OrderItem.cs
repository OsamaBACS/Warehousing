namespace Warehousing.Data.Entities
{
    public class OrderItem : BaseClass
    {
        public int Id { get; set; }
        public decimal Quantity { get; set; }
        
        // Fixed pricing - always capture cost at time of transaction
        public decimal UnitCost { get; set; }       // Cost per unit (for COGS calculation)
        public decimal UnitPrice { get; set; }      // Selling price per unit
        public decimal Discount { get; set; } = 0;  // Discount amount or percentage
        
        public string? Notes { get; set; } = string.Empty;

        //FK
        public Product Product { get; set; } = null!;
        public int ProductId { get; set; }
        
        public Store Store { get; set; } = null!;
        public int StoreId { get; set; }
        
        public Order Order { get; set; } = null!;
        public int OrderId { get; set; }
        
        // Variant support
        public ProductVariant? Variant { get; set; }
        public int? VariantId { get; set; }

        // Computed properties
        public decimal TotalCost => Quantity * UnitCost;
        public decimal TotalPrice => (Quantity * UnitPrice) - Discount;

        // Navigation properties
        public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
        public ICollection<OrderItemModifier> Modifiers { get; set; } = new List<OrderItemModifier>();
    }
}