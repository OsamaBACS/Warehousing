namespace Warehousing.Repo.Dtos
{
    public class UsersDto
    {
        public int Id { get; set; }
        public string? NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PrintHeader { get; set; } = string.Empty;
        public string PrintFooter { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public List<int>? Roles { get; set; }
        public List<UserDeviceDto>? Devices { get; set; }
    }
}