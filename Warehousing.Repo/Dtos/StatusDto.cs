namespace Warehousing.Repo.Dtos
{
    public class StatusDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}