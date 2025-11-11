-- Seed Printer Configurations
-- Run this script if you want to manually seed the printer configurations

-- Check if configurations already exist
IF NOT EXISTS (SELECT 1 FROM PrinterConfigurations WHERE Id = 1)
BEGIN
    -- A4 Default Configuration
    INSERT INTO PrinterConfigurations (
        Id, NameAr, NameEn, Description, PrinterType, PaperFormat, PaperWidth, PaperHeight,
        Margins, FontSettings, PosSettings, PrintInColor, PrintBackground, Orientation, Scale,
        IsActive, IsDefault, CreatedAt, CreatedBy
    ) VALUES (
        1,
        N'طابعة A4 الافتراضية',
        N'Default A4 Printer',
        N'إعدادات افتراضية لطابعة A4',
        N'A4',
        N'A4',
        210,
        297,
        N'{"top":"20mm","right":"20mm","bottom":"20mm","left":"20mm"}',
        N'{"fontFamily":"Arial","baseFontSize":12,"headerFontSize":16,"footerFontSize":10,"tableFontSize":11}',
        NULL,
        1,
        1,
        N'Portrait',
        1.0,
        1,
        1,
        GETUTCDATE(),
        N'system'
    );
    PRINT 'A4 Printer Configuration created.';
END
ELSE
BEGIN
    PRINT 'A4 Printer Configuration already exists.';
END

IF NOT EXISTS (SELECT 1 FROM PrinterConfigurations WHERE Id = 2)
BEGIN
    -- POS/Thermal Default Configuration
    INSERT INTO PrinterConfigurations (
        Id, NameAr, NameEn, Description, PrinterType, PaperFormat, PaperWidth, PaperHeight,
        Margins, FontSettings, PosSettings, PrintInColor, PrintBackground, Orientation, Scale,
        IsActive, IsDefault, CreatedAt, CreatedBy
    ) VALUES (
        2,
        N'طابعة نقاط البيع الحرارية',
        N'POS Thermal Printer',
        N'إعدادات افتراضية لطابعة نقاط البيع الحرارية 80مم',
        N'POS',
        N'Thermal',
        80,
        0,
        N'{"top":"5mm","right":"5mm","bottom":"5mm","left":"5mm"}',
        N'{"fontFamily":"Courier","baseFontSize":10,"headerFontSize":12,"footerFontSize":8,"tableFontSize":9}',
        N'{"encoding":"UTF-8","copies":1,"autoCut":true,"openCashDrawer":false,"printDensity":8,"printSpeed":3,"useEscPos":true,"connectionType":"USB","connectionString":null}',
        0,
        0,
        N'Portrait',
        1.0,
        1,
        1,
        GETUTCDATE(),
        N'system'
    );
    PRINT 'POS Printer Configuration created.';
END
ELSE
BEGIN
    PRINT 'POS Printer Configuration already exists.';
END

-- Assign A4 config to Admin role
UPDATE Roles
SET PrinterConfigurationId = 1
WHERE Code = 'ADMIN' AND PrinterConfigurationId IS NULL;

-- Assign A4 config to Warehouse Manager role
UPDATE Roles
SET PrinterConfigurationId = 1
WHERE Code = 'WAREHOUSE_MANAGER' AND PrinterConfigurationId IS NULL;

-- Assign POS config to Sales Manager role
UPDATE Roles
SET PrinterConfigurationId = 2
WHERE Code = 'SALES_MANAGER' AND PrinterConfigurationId IS NULL;

PRINT 'Printer configuration assignments completed.';

