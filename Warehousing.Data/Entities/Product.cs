namespace Warehousing.Data.Entities
{
    public class Product : BaseClass
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public decimal? OpeningBalance { get; set; }
        public decimal? ReorderLevel { get; set; } = 10;
        public string? ImagePath { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public decimal QuantityInStock { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public DateTime LastStockUpdateDate { get; set; } = DateTime.UtcNow;

        // FKs
        public SubCategory? SubCategory { get; set; }
        public int? SubCategoryId { get; set; }
        public int? UnitId { get; set; }
        public Unit? Unit { get; set; }
        public int? StoreId { get; set; }
        public Store? Store { get; set; }
    }
}