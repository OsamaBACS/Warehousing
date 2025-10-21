namespace Warehousing.Data.Entities
{
    public class Inventory : BaseClass
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int StoreId { get; set; }
        public Store? Store { get; set; }

        public decimal Quantity { get; set; }   // total stock in that store
    }
}