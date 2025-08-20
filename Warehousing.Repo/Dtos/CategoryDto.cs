namespace Warehousing.Repo.Dtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string? Code { get; set; } = string.Empty;
        public string? NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public virtual ICollection<SubCategoryDto>? SubCategories { get; set; } = new List<SubCategoryDto>();
    }
}