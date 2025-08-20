namespace Warehousing.Data.Entities
{
    public class SubCategory
    {
        public int Id { get; set; }
        public string? NameEn { get; set; } = string.Empty;
        public string? NameAr { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}