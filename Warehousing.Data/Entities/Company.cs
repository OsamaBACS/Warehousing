namespace Warehousing.Data.Entities
{
    public class Company : BaseClass
    {
        public int Id { get; set; }

        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;

        public string AddressEn { get; set; } = string.Empty;
        public string AddressAr { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;

        public string TaxNumber { get; set; } = string.Empty;

        public string CurrencyCode { get; set; } = "JOD"; // Default currency
        public string FooterNoteEn { get; set; } = string.Empty;
        public string FooterNoteAr { get; set; } = string.Empty;

        public string TermsEn { get; set; } = string.Empty;
        public string TermsAr { get; set; } = string.Empty;

        public string LogoUrl { get; set; } = string.Empty;

        public int? PrintTemplateId { get; set; } // For supporting custom layouts

        public bool IsActive { get; set; } = true;
    }
}