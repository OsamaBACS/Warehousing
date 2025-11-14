namespace Warehousing.Data.Entities
{
    public class Order : BaseClass
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }

        //FK
        public OrderType? OrderType { get; set; }
        public int? OrderTypeId { get; set; }
        public Customer? Customer { get; set; }
        public int? CustomerId { get; set; }
        public Supplier? Supplier { get; set; }
        public int? SupplierId { get; set; }
        public Status? Status { get; set; }
        public int? StatusId { get; set; }

        // Navigation properties
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
    }
}