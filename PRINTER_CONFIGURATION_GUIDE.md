# Printer Configuration System Guide

## Overview

The Warehousing system now supports dynamic printer configuration, allowing clients to configure different printer types (A4, POS, Thermal, Label) with custom settings per company.

## Features

### Supported Printer Types

1. **A4 Printers** - Standard paper printers
   - Configurable paper formats (A4, Letter, Legal, A3, A5)
   - Margins, fonts, orientation, color settings
   - Full PDF generation support

2. **POS/Thermal Printers** - Point of Sale thermal printers
   - ESC/POS command generation
   - Configurable paper width (58mm, 80mm, etc.)
   - Auto-cut, cash drawer support
   - Connection types: USB, Network, Serial, Bluetooth

3. **Label Printers** - Label printing support
   - Custom paper dimensions
   - Label-specific formatting

## Configuration

### Setting Up Printer Configuration

1. **Navigate to Company Settings**
   - Go to Admin Panel → Companies
   - Edit or create a company

2. **Access Printer Settings**
   - Scroll to "إعدادات الطابعة" (Printer Settings) section
   - Click "إظهار الإعدادات" (Show Settings) to expand

3. **Configure Printer Type**
   - Select printer type: A4, POS, Thermal, or Label
   - Settings will automatically adjust based on selection

4. **Configure Settings**

   **For A4 Printers:**
   - Paper Format: A4, Letter, Legal, A3, A5
   - Orientation: Portrait or Landscape
   - Margins: Top, Right, Bottom, Left (e.g., "20mm")
   - Font Settings: Family, sizes for header, body, footer, tables
   - Print Options: Color, Background graphics
   - Scale: 0.1 to 2.0

   **For POS/Thermal Printers:**
   - Paper Width: in millimeters (e.g., 80mm)
   - Paper Height: 0 for continuous roll
   - Encoding: UTF-8, Windows-1256, ISO-8859-6
   - Connection Type: USB, Network, Serial, Bluetooth
   - Connection String: COM port or IP address
   - Print Density: 0-15
   - Print Speed: 1-5
   - Auto Cut: Enable/disable
   - Open Cash Drawer: Enable/disable
   - Use ESC/POS: Enable ESC/POS command generation

5. **Save Configuration**
   - Click "حفظ الشركة" (Save Company)
   - Configuration is stored as JSON in the database

## Usage

### Printing Documents

The system automatically detects the configured printer type and generates the appropriate output:

- **A4 Printers**: Generates PDF files that open in print dialog
- **POS/Thermal Printers**: Generates ESC/POS commands that can be:
  - Sent directly to printer via WebUSB (if supported)
  - Downloaded as .escpos file for manual printing

### API Endpoints

#### Generate Print Output
```
POST /api/print/generate-pdf
Body: {
  "htmlContent": "<html>...</html>",
  "title": "Document Title",
  "type": "document" | "order" | "report"
}
```

**Response:**
- Content-Type: `application/pdf` for A4 printers
- Content-Type: `application/octet-stream` for POS/Thermal printers

#### Generate Order PDF
```
POST /api/print/generate-order-pdf
Body: {
  "orderId": 123,
  "orderDate": "2024-01-01",
  "customerName": "Customer Name",
  "totalAmount": 100.00,
  "items": [...]
}
```

#### Health Check
```
GET /api/print/health
```

## Database Migration

Run the migration to add the `PrinterConfiguration` field to the `Companies` table:

```bash
dotnet ef database update
```

Or apply the migration file:
- `20251110134101_AddPrinterConfigurationToCompany.cs`

## Technical Details

### Backend Structure

- **Models**: `Warehousing.Data/Models/PrinterConfiguration.cs`
- **Entity**: `Company.PrinterConfiguration` (JSON field)
- **Service**: `Warehousing.Api/Services/EscPosService.cs`
- **Controller**: `Warehousing.Api/Controllers/PrintController.cs`

### Frontend Structure

- **Models**: `Warehousing.UI/src/app/admin/models/PrinterConfiguration.ts`
- **Service**: `Warehousing.UI/src/app/shared/services/pdf-print.service.ts`
- **Component**: `Warehousing.UI/src/app/admin/components/Companies/company-form.component/`

### ESC/POS Commands

The system generates standard ESC/POS commands including:
- Printer initialization (ESC @)
- Character encoding (ESC t)
- Print density (GS d)
- Print speed (GS s)
- Text formatting (ESC E, ESC -)
- Paper cutting (GS V)
- Cash drawer (ESC p)

## Default Configurations

### A4 Default
- Paper Format: A4
- Margins: 20mm all sides
- Font: Arial, 12pt base
- Orientation: Portrait
- Color: Enabled
- Background: Enabled

### POS Default
- Paper Width: 80mm
- Paper Height: 0 (continuous)
- Margins: 5mm all sides
- Font: Courier, 10pt base
- Encoding: UTF-8
- Auto Cut: Enabled
- ESC/POS: Enabled

## Troubleshooting

### PDF Generation Issues
- Check Puppeteer installation
- Verify printer configuration is valid JSON
- Check server logs for errors

### ESC/POS Issues
- Verify printer supports ESC/POS commands
- Check connection string (COM port or IP)
- Test with downloaded .escpos file first
- Ensure WebUSB permissions if using browser printing

### Configuration Not Applied
- Verify company has printer configuration saved
- Check database for `PrinterConfiguration` field
- Reload company settings in frontend

## Future Enhancements

Potential improvements:
- Direct network printer support
- Print preview for POS receipts
- Multiple printer profiles per company
- Print queue management
- Print history and logs

## Support

For issues or questions:
1. Check server logs for error messages
2. Verify printer configuration JSON is valid
3. Test with default configurations first
4. Ensure database migration is applied

