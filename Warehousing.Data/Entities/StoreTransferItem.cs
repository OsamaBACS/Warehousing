namespace Warehousing.Data.Entities
{
    public class StoreTransferItem : BaseClass
    {
        public int Id { get; set; }
        
        public int TransferId { get; set; }
        public StoreTransfer Transfer { get; set; } = null!;
        
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }  // Track cost for accounting
        public string Notes { get; set; } = string.Empty;
    }
}

