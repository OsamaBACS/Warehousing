using Microsoft.AspNetCore.Http;

namespace Warehousing.Repo.Dtos
{
    public class ProductDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; } = string.Empty;
        public string? NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public decimal? OpeningBalance { get; set; }
        public decimal? ReorderLevel { get; set; }
        public decimal QuantityInStock { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public DateTime LastStockUpdateDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; }
        public SubCategoryDto? SubCategory { get; set; }
        public int? SubCategoryId { get; set; }
        public int? UnitId { get; set; }
        public UnitDto? Unit { get; set; }
        public int? StoreId { get; set; }
        public StoreDto? Store { get; set; }

        public IFormFile? Image { get; set; }
        public string? ImagePath { get; set; } = string.Empty;
    }
}