namespace Warehousing.Data.Models
{
    /// <summary>
    /// Printer configuration stored as JSON in Company.PrinterConfiguration
    /// Supports multiple printer types: A4, POS (thermal), etc.
    /// </summary>
    public class PrinterConfiguration
    {
        /// <summary>
        /// Printer type: "A4", "POS", "Thermal", "Label", etc.
        /// </summary>
        public string PrinterType { get; set; } = "A4";

        /// <summary>
        /// Paper format for A4 printers
        /// </summary>
        public string PaperFormat { get; set; } = "A4"; // A4, Letter, Legal, etc.

        /// <summary>
        /// Paper width in mm (for POS/Thermal printers)
        /// </summary>
        public int PaperWidth { get; set; } = 80; // 58mm, 80mm, etc.

        /// <summary>
        /// Paper height in mm (for POS/Thermal printers, 0 = continuous)
        /// </summary>
        public int PaperHeight { get; set; } = 0; // 0 = continuous roll

        /// <summary>
        /// Margin settings
        /// </summary>
        public PrinterMargins Margins { get; set; } = new PrinterMargins();

        /// <summary>
        /// Font settings
        /// </summary>
        public PrinterFontSettings FontSettings { get; set; } = new PrinterFontSettings();

        /// <summary>
        /// POS/Thermal printer specific settings
        /// </summary>
        public PosPrinterSettings? PosSettings { get; set; }

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
    }

    public class PrinterMargins
    {
        public string Top { get; set; } = "20mm";
        public string Right { get; set; } = "20mm";
        public string Bottom { get; set; } = "20mm";
        public string Left { get; set; } = "20mm";
    }

    public class PrinterFontSettings
    {
        /// <summary>
        /// Font family
        /// </summary>
        public string FontFamily { get; set; } = "Arial";

        /// <summary>
        /// Base font size in points
        /// </summary>
        public int BaseFontSize { get; set; } = 12;

        /// <summary>
        /// Header font size
        /// </summary>
        public int HeaderFontSize { get; set; } = 16;

        /// <summary>
        /// Footer font size
        /// </summary>
        public int FooterFontSize { get; set; } = 10;

        /// <summary>
        /// Table font size
        /// </summary>
        public int TableFontSize { get; set; } = 11;
    }

    public class PosPrinterSettings
    {
        /// <summary>
        /// Character encoding for POS printers
        /// </summary>
        public string Encoding { get; set; } = "UTF-8";

        /// <summary>
        /// Number of copies
        /// </summary>
        public int Copies { get; set; } = 1;

        /// <summary>
        /// Whether to cut paper after printing
        /// </summary>
        public bool AutoCut { get; set; } = true;

        /// <summary>
        /// Whether to open cash drawer
        /// </summary>
        public bool OpenCashDrawer { get; set; } = false;

        /// <summary>
        /// Print density (0-15)
        /// </summary>
        public int PrintDensity { get; set; } = 8;

        /// <summary>
        /// Print speed
        /// </summary>
        public int PrintSpeed { get; set; } = 3;

        /// <summary>
        /// Whether to use ESC/POS commands
        /// </summary>
        public bool UseEscPos { get; set; } = true;

        /// <summary>
        /// Printer connection type: "USB", "Network", "Serial", "Bluetooth"
        /// </summary>
        public string ConnectionType { get; set; } = "USB";

        /// <summary>
        /// Printer connection string/address
        /// </summary>
        public string? ConnectionString { get; set; }
    }
}

