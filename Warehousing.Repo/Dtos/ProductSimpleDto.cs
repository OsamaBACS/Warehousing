namespace Warehousing.Repo.Dtos
{
    public class ProductSimpleDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public bool IsActive { get; set; } = true;
        public int SubCategoryId { get; set; }
        public int UnitId { get; set; }
        public string? ImagePath { get; set; } = string.Empty;
        
        // Simple navigation properties (no deep loading)
        public string? SubCategoryName { get; set; }
        public string? UnitName { get; set; }
        public string? UnitCode { get; set; }
        
        // Total quantity across all stores
        public decimal TotalQuantity { get; set; } = 0;
        
        // Audit fields
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; } = string.Empty;
    }
}
