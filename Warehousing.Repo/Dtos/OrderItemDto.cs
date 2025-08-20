namespace Warehousing.Repo.Dtos
{
    public class OrderItemDto
    {
        public int? Id { get; set; }
        public decimal Quantity { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal SellingPrice { get; set; }

        //FK
        public ProductDto? Product { get; set; }
        public int? ProductId { get; set; }
        public StoreDto? Store { get; set; }
        public int? StoreId { get; set; }
        public OrderDto? Order { get; set; }
        public int? OrderId { get; set; }
    }
}