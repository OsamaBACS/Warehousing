namespace Warehousing.Data.Entities
{
    public class ProductRecipe : BaseClass
    {
        public int Id { get; set; }
        
        public int ParentProductId { get; set; }  // The finished product
        public Product ParentProduct { get; set; } = null!;
        
        public int ComponentProductId { get; set; }  // The ingredient/component
        public Product ComponentProduct { get; set; } = null!;
        
        public decimal Quantity { get; set; }  // How much of component needed
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; } = string.Empty;
    }
}

