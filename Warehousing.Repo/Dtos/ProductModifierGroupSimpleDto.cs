namespace Warehousing.Repo.Dtos
{
    public class ProductModifierGroupSimpleDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int ModifierId { get; set; }
        public string ModifierName { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = false;
        public int MaxSelections { get; set; } = 1;
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}















