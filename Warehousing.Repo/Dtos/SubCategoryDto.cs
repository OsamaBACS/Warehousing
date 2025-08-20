namespace Warehousing.Repo.Dtos
{
    public class SubCategoryDto
    {
        public int Id { get; set; }
        public string? NameEn { get; set; } = string.Empty;
        public string? NameAr { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public int CategoryId { get; set; }
        public virtual CategoryDto? Category { get; set; } = null!;
        public virtual ICollection<ProductDto>? Products { get; set; } = new List<ProductDto>();
    }
}