namespace Warehousing.Data.Entities
{
    /// <summary>
    /// Printer configuration entity - stores printer settings that can be assigned to roles
    /// </summary>
    public class PrinterConfiguration : BaseClass
    {
        public int Id { get; set; }

        /// <summary>
        /// Configuration name (e.g., "A4 Standard", "POS Thermal 80mm")
        /// </summary>
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }

        /// <summary>
        /// Description of this printer configuration
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Printer type: "A4", "POS", "Thermal", "Label"
        /// </summary>
        public string PrinterType { get; set; } = "A4";

        /// <summary>
        /// Paper format for A4 printers (A4, Letter, Legal, etc.)
        /// </summary>
        public string PaperFormat { get; set; } = "A4";

        /// <summary>
        /// Paper width in mm (for POS/Thermal printers)
        /// </summary>
        public int PaperWidth { get; set; } = 210; // Default A4 width

        /// <summary>
        /// Paper height in mm (0 = continuous roll for thermal)
        /// </summary>
        public int PaperHeight { get; set; } = 297; // Default A4 height

        /// <summary>
        /// Margin settings stored as JSON
        /// </summary>
        public string? Margins { get; set; } // JSON: {"top":"20mm","right":"20mm","bottom":"20mm","left":"20mm"}

        /// <summary>
        /// Font settings stored as JSON
        /// </summary>
        public string? FontSettings { get; set; } // JSON: {"fontFamily":"Arial","baseFontSize":12,...}

        /// <summary>
        /// POS/Thermal printer specific settings stored as JSON
        /// </summary>
        public string? PosSettings { get; set; } // JSON: {"encoding":"UTF-8","autoCut":true,...}

        /// <summary>
        /// Whether to print in color (for A4 printers)
        /// </summary>
        public bool PrintInColor { get; set; } = true;

        /// <summary>
        /// Whether to print background graphics
        /// </summary>
        public bool PrintBackground { get; set; } = true;

        /// <summary>
        /// Orientation: "Portrait" or "Landscape"
        /// </summary>
        public string Orientation { get; set; } = "Portrait";

        /// <summary>
        /// Scale factor (0.1 to 2.0)
        /// </summary>
        public double Scale { get; set; } = 1.0;

        /// <summary>
        /// Whether this configuration is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether this is a default/system configuration (cannot be deleted)
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Roles that use this printer configuration
        /// </summary>
        public List<Role> Roles { get; set; } = new();
    }
}

