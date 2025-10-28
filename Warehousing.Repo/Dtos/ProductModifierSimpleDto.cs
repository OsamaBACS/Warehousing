namespace Warehousing.Repo.Dtos
{
    public class ProductModifierSimpleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public decimal PriceAdjustment { get; set; } = 0;
        public decimal? CostAdjustment { get; set; }
        public bool IsRequired { get; set; } = false;
        public bool IsMultiple { get; set; } = false;
        public int MaxSelections { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }
}







