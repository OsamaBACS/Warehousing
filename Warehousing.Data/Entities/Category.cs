namespace Warehousing.Data.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string? NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public virtual ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    }
}