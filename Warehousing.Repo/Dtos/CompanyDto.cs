using Microsoft.AspNetCore.Http;

namespace Warehousing.Repo.Dtos
{
    public class CompanyDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string AddressEn { get; set; }
        public string AddressAr { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string TaxNumber { get; set; }
        public string Fax { get; set; }
        public string RegistrationNumber { get; set; }
        public decimal? Capital { get; set; }
        public string SloganEn { get; set; }
        public string SloganAr { get; set; }
        public string CurrencyCode { get; set; }
        public string FooterNoteEn { get; set; }
        public string FooterNoteAr { get; set; }
        public string TermsEn { get; set; }
        public string TermsAr { get; set; }
        public int? PrintTemplateId { get; set; }
        public bool IsActive { get; set; }
        public IFormFile? Image { get; set; }
    }
}