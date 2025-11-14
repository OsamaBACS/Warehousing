namespace Warehousing.Repo.Dtos
{
    public class SubCategorySimpleDto
    {
        public int Id { get; set; }
        public string? NameEn { get; set; } = string.Empty;
        public string? NameAr { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string? ImagePath { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; } // To display category name without loading full category object
        
        // Audit fields
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; } = string.Empty;
    }
}












