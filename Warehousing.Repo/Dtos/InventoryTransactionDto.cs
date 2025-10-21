namespace Warehousing.Repo.Dtos
{
    public class InventoryTransactionDto
    {
        public int Id { get; set; }
        public decimal QuantityChanged { get; set; } //Positive for in, negative for out
        public DateTime TransactionDate { get; set; } //Purchase, Sale, Adjustment
        public string Notes { get; set; } = string.Empty;

        // Calculated fields for better tracking
        public decimal QuantityBefore { get; set; } // Stock level before transaction
        public decimal QuantityAfter { get; set; }  // Stock level after transaction
        public decimal UnitCost { get; set; }       // Cost per unit at transaction time

        // Core relationships
        public int ProductId { get; set; }
        public string ProductNameAr { get; set; } = string.Empty;
        public string? ProductNameEn { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;

        public int TransactionTypeId { get; set; }
        public string TransactionTypeNameAr { get; set; } = string.Empty;
        public string? TransactionTypeNameEn { get; set; } = string.Empty;
        public string TransactionTypeCode { get; set; } = string.Empty;

        // Store tracking (CRITICAL)
        public int StoreId { get; set; }
        public string StoreNameAr { get; set; } = string.Empty;
        public string? StoreNameEn { get; set; } = string.Empty;
        public string? StoreCode { get; set; } = string.Empty;

        // Order tracking
        public int? OrderId { get; set; }
        public string? OrderNumber { get; set; } = string.Empty;
        
        public int? OrderItemId { get; set; }

        // Transfer tracking
        public int? TransferId { get; set; }

        // Audit fields
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; } = string.Empty;
    }
}



