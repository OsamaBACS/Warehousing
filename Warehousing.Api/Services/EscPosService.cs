using System.Text;
using Warehousing.Data.Models;

namespace Warehousing.Api.Services
{
    /// <summary>
    /// Service for generating ESC/POS commands for thermal/POS printers
    /// </summary>
    public class EscPosService
    {
        /// <summary>
        /// Generate ESC/POS commands from HTML content for POS/Thermal printers
        /// </summary>
        public byte[] GenerateEscPosCommands(string htmlContent, PrinterConfiguration config)
        {
            var commands = new List<byte>();

            // Initialize printer
            commands.AddRange(InitializePrinter());

            // Set encoding
            if (config.PosSettings != null)
            {
                commands.AddRange(SetEncoding(config.PosSettings.Encoding));
            }

            // Set print density and speed
            if (config.PosSettings != null)
            {
                commands.AddRange(SetPrintDensity(config.PosSettings.PrintDensity));
                commands.AddRange(SetPrintSpeed(config.PosSettings.PrintSpeed));
            }

            // Parse HTML and convert to ESC/POS commands
            var textContent = ExtractTextFromHtml(htmlContent);
            commands.AddRange(ConvertTextToEscPos(textContent, config));

            // Add line feeds
            commands.AddRange(LineFeed(3));

            // Cut paper if auto-cut is enabled
            if (config.PosSettings?.AutoCut == true)
            {
                commands.AddRange(CutPaper());
            }

            // Open cash drawer if enabled
            if (config.PosSettings?.OpenCashDrawer == true)
            {
                commands.AddRange(OpenCashDrawer());
            }

            return commands.ToArray();
        }

        /// <summary>
        /// Initialize printer (ESC @)
        /// </summary>
        private byte[] InitializePrinter()
        {
            return new byte[] { 0x1B, 0x40 }; // ESC @
        }

        /// <summary>
        /// Set character encoding
        /// </summary>
        private byte[] SetEncoding(string encoding)
        {
            // ESC t n (Select character code table)
            // n = 0: PC437, 1: Katakana, 2: PC850, 3: PC860, 4: PC863, 5: PC865, 16: PC1252, 17: PC866, 18: PC852, 19: PC858, 20: Thai42, 21: Thai11, 22: Thai13, 23: Thai14, 24: Thai16, 25: Thai17, 26: Thai18
            // For Arabic/UTF-8, we'll use PC1256 (Windows Arabic)
            byte codePage = encoding switch
            {
                "Windows-1256" => 20, // Arabic
                "UTF-8" => 16, // PC1252 (closest for UTF-8)
                "ISO-8859-6" => 20, // Arabic
                _ => 16
            };

            return new byte[] { 0x1B, 0x74, codePage }; // ESC t n
        }

        /// <summary>
        /// Set print density (GS d n)
        /// </summary>
        private byte[] SetPrintDensity(int density)
        {
            // Density: 0-15 (0 = lightest, 15 = darkest)
            byte densityByte = (byte)Math.Clamp(density, 0, 15);
            return new byte[] { 0x1D, 0x64, densityByte }; // GS d n
        }

        /// <summary>
        /// Set print speed (GS s n)
        /// </summary>
        private byte[] SetPrintSpeed(int speed)
        {
            // Speed: 1-5 (1 = slowest, 5 = fastest)
            byte speedByte = (byte)Math.Clamp(speed, 1, 5);
            return new byte[] { 0x1D, 0x73, speedByte }; // GS s n
        }

        /// <summary>
        /// Extract text content from HTML (simple implementation)
        /// </summary>
        private string ExtractTextFromHtml(string html)
        {
            // Simple HTML stripping - in production, use a proper HTML parser
            var text = html;
            
            // Remove script and style tags
            text = System.Text.RegularExpressions.Regex.Replace(text, @"<script[^>]*>[\s\S]*?</script>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"<style[^>]*>[\s\S]*?</style>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            // Remove HTML tags
            text = System.Text.RegularExpressions.Regex.Replace(text, @"<[^>]+>", "");
            
            // Decode HTML entities
            text = System.Net.WebUtility.HtmlDecode(text);
            
            // Clean up whitespace
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
            text = text.Trim();
            
            return text;
        }

        /// <summary>
        /// Convert text to ESC/POS commands
        /// </summary>
        private byte[] ConvertTextToEscPos(string text, PrinterConfiguration config)
        {
            var commands = new List<byte>();
            var encoding = Encoding.GetEncoding(config.PosSettings?.Encoding ?? "UTF-8");

            // Set font
            commands.AddRange(SetFont(config.FontSettings.FontFamily));

            // Set text size
            commands.AddRange(SetTextSize(config.FontSettings.BaseFontSize));

            // Center align for header (if needed)
            // commands.AddRange(SetAlignment(1)); // Center

            // Add text
            var textBytes = encoding.GetBytes(text);
            commands.AddRange(textBytes);

            // Reset alignment
            commands.AddRange(SetAlignment(0)); // Left

            return commands.ToArray();
        }

        /// <summary>
        /// Set font (ESC ! n)
        /// </summary>
        private byte[] SetFont(string fontFamily)
        {
            // Font A = 0, Font B = 1, Font C = 2
            byte font = fontFamily.ToLower() switch
            {
                "courier" => 1, // Font B
                "times new roman" => 2, // Font C
                _ => 0 // Font A (default)
            };

            return new byte[] { 0x1B, 0x21, font }; // ESC ! n
        }

        /// <summary>
        /// Set text size (GS ! n)
        /// </summary>
        private byte[] SetTextSize(int fontSize)
        {
            // Size calculation: 0 = normal, 1 = double width, 2 = double height, 3 = double width and height
            byte size = fontSize switch
            {
                >= 16 => 3, // Double width and height
                >= 12 => 1, // Double width
                _ => 0 // Normal
            };

            return new byte[] { 0x1D, 0x21, size }; // GS ! n
        }

        /// <summary>
        /// Set alignment (ESC a n)
        /// </summary>
        private byte[] SetAlignment(int alignment)
        {
            // 0 = Left, 1 = Center, 2 = Right
            byte align = (byte)Math.Clamp(alignment, 0, 2);
            return new byte[] { 0x1B, 0x61, align }; // ESC a n
        }

        /// <summary>
        /// Line feed (LF)
        /// </summary>
        private byte[] LineFeed(int count = 1)
        {
            var feeds = new List<byte>();
            for (int i = 0; i < count; i++)
            {
                feeds.Add(0x0A); // LF
            }
            return feeds.ToArray();
        }

        /// <summary>
        /// Cut paper (GS V n)
        /// </summary>
        private byte[] CutPaper()
        {
            // Full cut: GS V 0 or GS V 48
            return new byte[] { 0x1D, 0x56, 0x00 }; // GS V 0
        }

        /// <summary>
        /// Open cash drawer (ESC p m t1 t2)
        /// </summary>
        private byte[] OpenCashDrawer()
        {
            // ESC p 0 25 250 (pulse pin 0 for 250ms)
            return new byte[] { 0x1B, 0x70, 0x00, 0x19, 0xFA };
        }

        /// <summary>
        /// Set bold (ESC E n)
        /// </summary>
        public byte[] SetBold(bool bold)
        {
            return new byte[] { 0x1B, 0x45, (byte)(bold ? 1 : 0) }; // ESC E n
        }

        /// <summary>
        /// Set underline (ESC - n)
        /// </summary>
        public byte[] SetUnderline(bool underline)
        {
            return new byte[] { 0x1B, 0x2D, (byte)(underline ? 1 : 0) }; // ESC - n
        }
    }
}

