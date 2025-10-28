namespace Warehousing.Data.Entities
{
    public class Role : BaseClass
    {
        public int Id { get; set; }
        public string? Code { get; set; } = string.Empty;
        public string? NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public List<RolePermission> RolePermissions { get; set; }

        public List<RoleCategory> RoleCategories { get; set; } = new();
        public List<RoleProduct> RoleProducts { get; set; } = new();
        public List<RoleSubCategory> RoleSubCategories { get; set; } = new();
    }
}