namespace Warehousing.Data.Entities
{
    public class StoreTransfer : BaseClass
    {
        public int Id { get; set; }
        public DateTime TransferDate { get; set; }
        public string Notes { get; set; } = string.Empty;

        // From/To stores
        public int FromStoreId { get; set; }
        public Store FromStore { get; set; } = null!;
        
        public int ToStoreId { get; set; }
        public Store ToStore { get; set; } = null!;

        // Status tracking
        public int StatusId { get; set; }  // Draft, In-Transit, Completed, Cancelled
        public Status Status { get; set; } = null!;

        // Navigation properties
        public ICollection<StoreTransferItem> Items { get; set; } = new List<StoreTransferItem>();
        public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
    }
}

