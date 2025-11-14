namespace Warehousing.Data.Models
{
    /// <summary>
    /// Print settings structure stored as JSON in User.PrintHeader and User.PrintFooter
    /// This allows flexible customization of print templates
    /// </summary>
    public class PrintHeaderSettings
    {
        /// <summary>
        /// Custom text/html to display at the top of print documents (before company header)
        /// </summary>
        public string? CustomText { get; set; }

        /// <summary>
        /// Show/hide specific elements
        /// </summary>
        public PrintHeaderVisibility Visibility { get; set; } = new PrintHeaderVisibility();
    }

    public class PrintHeaderVisibility
    {
        public bool ShowCompanyName { get; set; } = true;
        public bool ShowCompanyLogo { get; set; } = true;
        public bool ShowCompanyAddress { get; set; } = true;
        public bool ShowCompanyPhone { get; set; } = true;
        public bool ShowCompanyFax { get; set; } = true;
        public bool ShowCompanyEmail { get; set; } = true;
        public bool ShowRegistrationNumber { get; set; } = true;
        public bool ShowCapital { get; set; } = true;
        public bool ShowTaxNumber { get; set; } = true;
        public bool ShowSlogan { get; set; } = true;
        public bool ShowDocumentTitle { get; set; } = true;
    }

    public class PrintFooterSettings
    {
        /// <summary>
        /// Custom text/html to display at the bottom of print documents (after terms and signatures)
        /// </summary>
        public string? CustomText { get; set; }

        /// <summary>
        /// Show/hide specific footer elements
        /// </summary>
        public PrintFooterVisibility Visibility { get; set; } = new PrintFooterVisibility();

        /// <summary>
        /// Custom terms and conditions (overrides company terms if provided)
        /// </summary>
        public string? CustomTerms { get; set; }

        /// <summary>
        /// Custom notes/instructions
        /// </summary>
        public string? CustomNotes { get; set; }
    }

    public class PrintFooterVisibility
    {
        public bool ShowTerms { get; set; } = true;
        public bool ShowNotes { get; set; } = true;
        public bool ShowCustomerSignature { get; set; } = true;
        public bool ShowAuthorizedSignature { get; set; } = true;
        public bool ShowCompanyFooterNote { get; set; } = true;
        public bool ShowDocumentGenerationDate { get; set; } = true;
    }
}




