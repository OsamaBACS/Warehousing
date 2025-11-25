namespace Warehousing.Repo.Dtos
{
    public class ProductModifierDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public decimal PriceAdjustment { get; set; } = 0;
        public decimal? CostAdjustment { get; set; } = 0;
        public bool IsRequired { get; set; } = false;
        public bool IsMultiple { get; set; } = false;
        public int MaxSelections { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
        
        // Navigation properties
        public ICollection<ProductModifierOptionDto> Options { get; set; } = new List<ProductModifierOptionDto>();
        public ICollection<ProductModifierGroupDto> Groups { get; set; } = new List<ProductModifierGroupDto>();
    }
}















