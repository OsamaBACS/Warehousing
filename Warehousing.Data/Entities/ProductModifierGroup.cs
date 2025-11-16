using System.ComponentModel.DataAnnotations;

namespace Warehousing.Data.Entities
{
    public class ProductModifierGroup : BaseClass
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        [Required]
        public int ModifierId { get; set; }
        public ProductModifier Modifier { get; set; } = null!;
        
        // Behavior
        public bool IsRequired { get; set; } = false;
        public int MaxSelections { get; set; } = 1;
        
        // Display order
        public int DisplayOrder { get; set; } = 0;
        
        // Status
        public bool IsActive { get; set; } = true;
    }
}













