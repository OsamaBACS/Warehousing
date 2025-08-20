namespace Warehousing.Repo.Dtos
{
    public class AssignPermissionsDto
    {
        public int RoleId { get; set; }
        public List<string> PermissionCodes { get; set; } = new();
    }
}