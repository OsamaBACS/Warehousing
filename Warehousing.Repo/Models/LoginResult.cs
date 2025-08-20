namespace Warehousing.Repo.Models
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Status { get; set; }
    }
}