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
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public bool IsActive { get; set; }
        public SubCategoryDto? SubCategory { get; set; }
        public int? SubCategoryId { get; set; }
        public int? UnitId { get; set; }
        public UnitDto? Unit { get; set; }

        public ICollection<InventoryDto> Inventories { get; set; } = new List<InventoryDto>();
        
        // StoreId for frontend compatibility (can be null or set to primary store)
        public int? StoreId { get; set; }

        public IFormFile? Image { get; set; }
        public string? ImagePath { get; set; } = string.Empty;
        
        // Variants and Modifiers support
        public ICollection<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
        public ICollection<ProductModifierGroupDto> ModifierGroups { get; set; } = new List<ProductModifierGroupDto>();
    }
}