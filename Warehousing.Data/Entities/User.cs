namespace Warehousing.Data.Entities
{
    public class User : BaseClass
    {
        public int Id { get; set; }
        public string? NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PrintHeader { get; set; } = string.Empty;
        public string PrintFooter { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public ICollection<UserRole> UserRoles { get; set; }
        public List<UserDevice> Devices { get; set; }
    }
}