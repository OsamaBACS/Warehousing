namespace Warehousing.Data.Entities
{
    public class InventoryTransaction : BaseClass
    {
        public int Id { get; set; }
        public decimal QuantityChanged { get; set; } //Positive for in, negative for out
        public DateTime TransactionDate { get; set; } //Purchase, Sale, Adjustment
        public string Notes { get; set; } = string.Empty;

        // Calculated fields for better tracking
        public decimal QuantityBefore { get; set; } // Stock level before transaction
        public decimal QuantityAfter { get; set; }  // Stock level after transaction
        public decimal UnitCost { get; set; }       // Cost per unit at transaction time

        //FK - Core relationships
        public Product Product { get; set; } = null!;
        public int ProductId { get; set; }
        
        public TransactionType TransactionType { get; set; } = null!; //Purchase, Sale, Adjustment
        public int TransactionTypeId { get; set; }

        // Store tracking (CRITICAL FIX)
        public Store Store { get; set; } = null!;
        public int StoreId { get; set; }

        // Order tracking
        public Order? Order { get; set; } //PO or SO Id depending on type (nullable)
        public int? OrderId { get; set; }
        
        public OrderItem? OrderItem { get; set; } // Links to specific order line
        public int? OrderItemId { get; set; }

        // Transfer tracking
        public StoreTransfer? Transfer { get; set; } // For transfer transactions
        public int? TransferId { get; set; }
    }
}