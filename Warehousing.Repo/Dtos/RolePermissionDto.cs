namespace Warehousing.Repo.Dtos
{
    public class RolePermissionDto
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;     
        public string Code { get; set; } = string.Empty;
    }
}