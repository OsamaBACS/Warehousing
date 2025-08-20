namespace Warehousing.Repo.Dtos
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string? Code { get; set; } = string.Empty;
        public string? NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public List<RolePermissionDto> Permissions { get; set; }
        public List<int>? CategoryIds { get; set; }
        public List<int>? ProductIds { get; set; }
    }

    public class RoleCreateUpdateDto
    {
        public int Id { get; set; }
        public string? Code { get; set; } = string.Empty;
        public string? NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public List<int>? RolePermissionIds { get; set; } = new();
        public List<int>? CategoryIds { get; set; }
        public List<int>? ProductIds { get; set; }
    }
}