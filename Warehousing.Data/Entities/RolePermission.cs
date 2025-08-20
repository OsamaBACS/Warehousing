namespace Warehousing.Data.Entities
{
    public class RolePermission : BaseClass
    {
        public int Id { get; set; }
        public Role? Role { get; set; }
        public int RoleId { get; set; }

        public Permission? Permission { get; set; }
        public int PermissionId { get; set; }
    }
}