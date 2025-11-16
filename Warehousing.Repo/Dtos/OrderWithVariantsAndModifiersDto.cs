using Warehousing.Data.Entities;

namespace Warehousing.Repo.Dtos
{
    public class OrderWithVariantsAndModifiersDto : BaseClass
    {
        public int? Id { get; set; }
        public int OrderTypeId { get; set; }
        public int? CustomerId { get; set; }
        public int? SupplierId { get; set; }
        public int? StatusId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? Notes { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? FinalAmount { get; set; }

        // Navigation properties
        public OrderTypeDto? OrderType { get; set; }
        public CustomerDto? Customer { get; set; }
        public SupplierDto? Supplier { get; set; }
        public StatusDto? Status { get; set; }
        public ICollection<OrderItemWithVariantsAndModifiersDto> Items { get; set; } = new List<OrderItemWithVariantsAndModifiersDto>();
    }

    public class OrderItemWithVariantsAndModifiersDto : BaseClass
    {
        public int? Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int StoreId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public ProductDto? Product { get; set; }
        public ProductVariantDto? Variant { get; set; }
        public StoreDto? Store { get; set; }
        public ICollection<OrderItemModifierDto> Modifiers { get; set; } = new List<OrderItemModifierDto>();
    }
}













