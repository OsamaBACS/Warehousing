namespace Warehousing.Repo.Dtos
{
    public class PrinterConfigurationDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string? Description { get; set; }
        public string PrinterType { get; set; } = "A4";
        public string PaperFormat { get; set; } = "A4";
        public int PaperWidth { get; set; } = 210;
        public int PaperHeight { get; set; } = 297;
        public string? Margins { get; set; }
        public string? FontSettings { get; set; }
        public string? PosSettings { get; set; }
        public bool PrintInColor { get; set; } = true;
        public bool PrintBackground { get; set; } = true;
        public string Orientation { get; set; } = "Portrait";
        public double Scale { get; set; } = 1.0;
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
    }
}

