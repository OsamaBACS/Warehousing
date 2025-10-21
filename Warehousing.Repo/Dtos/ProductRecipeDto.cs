namespace Warehousing.Repo.Dtos
{
    public class ProductRecipeDto
    {
        public int Id { get; set; }
        
        public int ParentProductId { get; set; }
        public string ParentProductNameAr { get; set; } = string.Empty;
        public string? ParentProductNameEn { get; set; } = string.Empty;
        public string ParentProductCode { get; set; } = string.Empty;
        
        public int ComponentProductId { get; set; }
        public string ComponentProductNameAr { get; set; } = string.Empty;
        public string? ComponentProductNameEn { get; set; } = string.Empty;
        public string ComponentProductCode { get; set; } = string.Empty;
        
        public decimal Quantity { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; } = string.Empty;

        // Audit fields
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; } = string.Empty;
    }
}

