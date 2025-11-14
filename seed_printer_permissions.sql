-- Manually seed Printer Configuration permissions
-- Run this if the permissions don't appear after restarting the application

-- Check if permissions already exist
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Code = 'VIEW_PRINTER_CONFIGURATIONS')
BEGIN
    INSERT INTO Permissions (Id, Code, NameEn, NameAr, CreatedAt, CreatedBy)
    VALUES (1013, N'VIEW_PRINTER_CONFIGURATIONS', N'View Printer Configurations', N'عرض إعدادات الطابعة', GETUTCDATE(), N'system');
    PRINT 'VIEW_PRINTER_CONFIGURATIONS permission created.';
END
ELSE
BEGIN
    PRINT 'VIEW_PRINTER_CONFIGURATIONS permission already exists.';
END

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Code = 'ADD_PRINTER_CONFIGURATION')
BEGIN
    INSERT INTO Permissions (Id, Code, NameEn, NameAr, CreatedAt, CreatedBy)
    VALUES (1014, N'ADD_PRINTER_CONFIGURATION', N'Add Printer Configuration', N'إضافة إعدادات طابعة', GETUTCDATE(), N'system');
    PRINT 'ADD_PRINTER_CONFIGURATION permission created.';
END
ELSE
BEGIN
    PRINT 'ADD_PRINTER_CONFIGURATION permission already exists.';
END

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Code = 'EDIT_PRINTER_CONFIGURATION')
BEGIN
    INSERT INTO Permissions (Id, Code, NameEn, NameAr, CreatedAt, CreatedBy)
    VALUES (1015, N'EDIT_PRINTER_CONFIGURATION', N'Edit Printer Configuration', N'تعديل إعدادات طابعة', GETUTCDATE(), N'system');
    PRINT 'EDIT_PRINTER_CONFIGURATION permission created.';
END
ELSE
BEGIN
    PRINT 'EDIT_PRINTER_CONFIGURATION permission already exists.';
END

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Code = 'DELETE_PRINTER_CONFIGURATION')
BEGIN
    INSERT INTO Permissions (Id, Code, NameEn, NameAr, CreatedAt, CreatedBy)
    VALUES (1016, N'DELETE_PRINTER_CONFIGURATION', N'Delete Printer Configuration', N'حذف إعدادات طابعة', GETUTCDATE(), N'system');
    PRINT 'DELETE_PRINTER_CONFIGURATION permission created.';
END
ELSE
BEGIN
    PRINT 'DELETE_PRINTER_CONFIGURATION permission already exists.';
END

PRINT 'Printer configuration permissions seeding completed.';

