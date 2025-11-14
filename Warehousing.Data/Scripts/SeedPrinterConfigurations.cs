using Microsoft.EntityFrameworkCore;
using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using System.Text.Json;

namespace Warehousing.Data.Scripts
{
    public static class SeedPrinterConfigurations
    {
        public static async Task SeedAsync(WarehousingContext context)
        {
            var existingConfigs = await context.PrinterConfigurations.AnyAsync();
            if (existingConfigs)
            {
                Console.WriteLine("Printer configurations already exist. Skipping seeding.");
                return;
            }

            Console.WriteLine("Seeding printer configurations...");

            // A4 Default Configuration
            var a4Config = new PrinterConfiguration
            {
                Id = 1,
                NameAr = "طابعة A4 الافتراضية",
                NameEn = "Default A4 Printer",
                Description = "إعدادات افتراضية لطابعة A4",
                PrinterType = "A4",
                PaperFormat = "A4",
                PaperWidth = 210,
                PaperHeight = 297,
                Margins = JsonSerializer.Serialize(new
                {
                    top = "20mm",
                    right = "20mm",
                    bottom = "20mm",
                    left = "20mm"
                }),
                FontSettings = JsonSerializer.Serialize(new
                {
                    fontFamily = "Arial",
                    baseFontSize = 12,
                    headerFontSize = 16,
                    footerFontSize = 10,
                    tableFontSize = 11
                }),
                PrintInColor = true,
                PrintBackground = true,
                Orientation = "Portrait",
                Scale = 1.0,
                IsActive = true,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            // POS/Thermal Default Configuration
            var posConfig = new PrinterConfiguration
            {
                Id = 2,
                NameAr = "طابعة نقاط البيع الحرارية",
                NameEn = "POS Thermal Printer",
                Description = "إعدادات افتراضية لطابعة نقاط البيع الحرارية 80مم",
                PrinterType = "POS",
                PaperFormat = "Thermal",
                PaperWidth = 80,
                PaperHeight = 0, // Continuous roll
                Margins = JsonSerializer.Serialize(new
                {
                    top = "5mm",
                    right = "5mm",
                    bottom = "5mm",
                    left = "5mm"
                }),
                FontSettings = JsonSerializer.Serialize(new
                {
                    fontFamily = "Courier",
                    baseFontSize = 10,
                    headerFontSize = 12,
                    footerFontSize = 8,
                    tableFontSize = 9
                }),
                PosSettings = JsonSerializer.Serialize(new
                {
                    encoding = "UTF-8",
                    copies = 1,
                    autoCut = true,
                    openCashDrawer = false,
                    printDensity = 8,
                    printSpeed = 3,
                    useEscPos = true,
                    connectionType = "USB",
                    connectionString = (string?)null
                }),
                PrintInColor = false,
                PrintBackground = false,
                Orientation = "Portrait",
                Scale = 1.0,
                IsActive = true,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            context.PrinterConfigurations.AddRange(a4Config, posConfig);
            await context.SaveChangesAsync();
            Console.WriteLine("Printer configurations seeded successfully.");

            // Assign A4 config to Admin and Warehouse Manager roles
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Code == "ADMIN");
            var warehouseManagerRole = await context.Roles.FirstOrDefaultAsync(r => r.Code == "WAREHOUSE_MANAGER");
            
            if (adminRole != null)
            {
                adminRole.PrinterConfigurationId = a4Config.Id;
                Console.WriteLine($"Assigned A4 config to {adminRole.NameAr} role.");
            }
            if (warehouseManagerRole != null)
            {
                warehouseManagerRole.PrinterConfigurationId = a4Config.Id;
                Console.WriteLine($"Assigned A4 config to {warehouseManagerRole.NameAr} role.");
            }

            // Assign POS config to Sales Manager role
            var salesManagerRole = await context.Roles.FirstOrDefaultAsync(r => r.Code == "SALES_MANAGER");
            if (salesManagerRole != null)
            {
                salesManagerRole.PrinterConfigurationId = posConfig.Id;
                Console.WriteLine($"Assigned POS config to {salesManagerRole.NameAr} role.");
            }

            await context.SaveChangesAsync();
            Console.WriteLine("Role assignments completed.");
        }
    }
}

