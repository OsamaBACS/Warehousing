using Microsoft.AspNetCore.Http;

namespace Warehousing.Repo.Dtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string? Code { get; set; } = string.Empty;
        public string? NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string? ImagePath { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Audit fields
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; } = string.Empty;
        
        public virtual ICollection<SubCategoryDto>? SubCategories { get; set; } = new List<SubCategoryDto>();
    }
}