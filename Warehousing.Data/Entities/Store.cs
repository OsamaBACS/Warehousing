namespace Warehousing.Data.Entities
{
    public class Store
    {
        public int Id { get; set; }
        public string? NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        
        // Enhanced fields
        public string? Code { get; set; } = string.Empty;        // Unique identifier, like "WH-01", "SHOP-02"
        public string? Address { get; set; } = string.Empty;
        public string? Phone { get; set; } = string.Empty;
        public bool IsMainWarehouse { get; set; } = false;       // Flag to identify primary warehouse

        // Navigation properties
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
        public ICollection<StoreTransfer> TransfersFrom { get; set; } = new List<StoreTransfer>();
        public ICollection<StoreTransfer> TransfersTo { get; set; } = new List<StoreTransfer>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}