namespace Warehousing.Repo.Dtos
{
    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Fingerprint { get; set; }
        public string? ip { get; set; }
    }
}