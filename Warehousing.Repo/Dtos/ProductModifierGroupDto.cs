namespace Warehousing.Repo.Dtos
{
    public class ProductModifierGroupDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int ModifierId { get; set; }
        public bool IsRequired { get; set; } = false;
        public int MaxSelections { get; set; } = 1;
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public ProductDto? Product { get; set; }
        public ProductModifierDto? Modifier { get; set; }
    }
}







