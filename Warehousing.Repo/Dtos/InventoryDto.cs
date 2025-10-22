namespace Warehousing.Repo.Dtos
{
    public class InventoryDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int StoreId { get; set; }
        public decimal Quantity { get; set; }   // total stock in that store
        
        // Variant support
        public ProductVariantDto? Variant { get; set; }
        public int? VariantId { get; set; }
    }
}