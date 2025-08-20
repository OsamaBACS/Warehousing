namespace Warehousing.Data.Entities
{
    public class InventoryTransaction : BaseClass
    {
        public int Id { get; set; }
        public decimal QuantityChanged { get; set; } //Positive for in, negative for out
        public DateTime TransactionDate { get; set; } //Purchase, Sale, Adjustment
        public string Notes { get; set; } = string.Empty;

        //FK
        public Product? Product { get; set; }
        public int? ProductId { get; set; }
        public TransactionType? TransactionType { get; set; } //Purchase, Sale, Adjustment
        public int? TransactionTypeId { get; set; }
        public Order? Order { get; set; } //PO or SO Id depending on type (nullable)
        public int? OrderId { get; set; }

    }
}