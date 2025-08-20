namespace Warehousing.Repo.Dtos
{
    public class OrderDto
    {
        public int? Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }

        //FK
        public OrderTypeDto? OrderType { get; set; }
        public int? OrderTypeId { get; set; }
        public CustomerDto? Customer { get; set; }
        public int? CustomerId { get; set; }
        public SupplierDto? Supplier { get; set; }
        public int? SupplierId { get; set; }
        public StatusDto? Status { get; set; }
        public int? StatusId { get; set; }

        public ICollection<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }
}