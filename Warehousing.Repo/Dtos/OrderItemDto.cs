namespace Warehousing.Repo.Dtos
{
    public class OrderItemDto
    {
        public int? Id { get; set; }
        public decimal Quantity { get; set; }
        
        // Fixed pricing - always capture cost at time of transaction
        public decimal UnitCost { get; set; }       // Cost per unit (for COGS calculation)
        public decimal UnitPrice { get; set; }      // Selling price per unit
        public decimal Discount { get; set; } = 0;  // Discount amount or percentage
        
        public string? Notes { get; set; } = string.Empty;

        //FK
        public ProductDto? Product { get; set; }
        public int ProductId { get; set; }
        
        public StoreDto? Store { get; set; }
        public int StoreId { get; set; }
        
        public OrderDto? Order { get; set; }
        public int OrderId { get; set; }

        // Computed properties
        public decimal TotalCost => Quantity * UnitCost;
        public decimal TotalPrice => (Quantity * UnitPrice) - Discount;

        // Audit fields
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; } = string.Empty;
    }
}