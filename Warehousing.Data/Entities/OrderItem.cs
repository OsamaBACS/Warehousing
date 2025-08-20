namespace Warehousing.Data.Entities
{
    public class OrderItem : BaseClass
    {
        public int Id { get; set; }
        public decimal Quantity { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal SellingPrice { get; set; }

        //FK
        public Product? Product { get; set; }
        public int? ProductId { get; set; }
        public Store? Store { get; set; }
        public int? StoreId { get; set; }
        public Order? Order { get; set; }
        public int? OrderId { get; set; }
    }
}