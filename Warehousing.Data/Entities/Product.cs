namespace Warehousing.Data.Entities
{
    public class Product : BaseClass
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string? ImagePath { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }

        // FKs
        public SubCategory? SubCategory { get; set; }
        public int? SubCategoryId { get; set; }
        public int? UnitId { get; set; }
        public Unit? Unit { get; set; }

        // Navigation properties
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        
        // Recipe support (future)
        public ICollection<ProductRecipe> RecipeAsParent { get; set; } = new List<ProductRecipe>();
        public ICollection<ProductRecipe> RecipeAsComponent { get; set; } = new List<ProductRecipe>();
        public ICollection<StoreTransferItem> TransferItems { get; set; } = new List<StoreTransferItem>();
    }
}