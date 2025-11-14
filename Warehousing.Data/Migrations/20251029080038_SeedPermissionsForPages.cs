using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehousing.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedPermissionsForPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed idempotent page-specific permissions so DB gets updated on migration
            migrationBuilder.Sql(@"
                -- Order approvals (in case earlier migration was empty)
                IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Code] = 'APPROVE_PURCHASE_ORDER')
                BEGIN
                    INSERT INTO [Permissions] ([Code], [NameEn], [NameAr], [CreatedBy], [UpdatedBy])
                    VALUES ('APPROVE_PURCHASE_ORDER', 'Approve Purchase Order', N'اعتماد أمر شراء', '', '')
                END
                IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Code] = 'CANCEL_PURCHASE_ORDER')
                BEGIN
                    INSERT INTO [Permissions] ([Code], [NameEn], [NameAr], [CreatedBy], [UpdatedBy])
                    VALUES ('CANCEL_PURCHASE_ORDER', 'Cancel Purchase Order', N'إلغاء أمر شراء', '', '')
                END
                IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Code] = 'APPROVE_SALE_ORDER')
                BEGIN
                    INSERT INTO [Permissions] ([Code], [NameEn], [NameAr], [CreatedBy], [UpdatedBy])
                    VALUES ('APPROVE_SALE_ORDER', 'Approve Sale Order', N'اعتماد أمر بيع', '', '')
                END
                IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Code] = 'CANCEL_SALE_ORDER')
                BEGIN
                    INSERT INTO [Permissions] ([Code], [NameEn], [NameAr], [CreatedBy], [UpdatedBy])
                    VALUES ('CANCEL_SALE_ORDER', 'Cancel Sale Order', N'إلغاء أمر بيع', '', '')
                END

                -- Activity Logs
                IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Code] = 'VIEW_ACTIVITY_LOGS')
                BEGIN
                    INSERT INTO [Permissions] ([Code], [NameEn], [NameAr], [CreatedBy], [UpdatedBy])
                    VALUES ('VIEW_ACTIVITY_LOGS', 'View Activity Logs', N'عرض سجل الأنشطة', '', '')
                END
                IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Code] = 'EXPORT_ACTIVITY_LOGS')
                BEGIN
                    INSERT INTO [Permissions] ([Code], [NameEn], [NameAr], [CreatedBy], [UpdatedBy])
                    VALUES ('EXPORT_ACTIVITY_LOGS', 'Export Activity Logs', N'تصدير سجل الأنشطة', '', '')
                END

                -- Working Hours
                IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Code] = 'VIEW_WORKING_HOURS')
                BEGIN
                    INSERT INTO [Permissions] ([Code], [NameEn], [NameAr], [CreatedBy], [UpdatedBy])
                    VALUES ('VIEW_WORKING_HOURS', 'View Working Hours', N'عرض ساعات العمل', '', '')
                END
                IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Code] = 'EDIT_WORKING_HOURS')
                BEGIN
                    INSERT INTO [Permissions] ([Code], [NameEn], [NameAr], [CreatedBy], [UpdatedBy])
                    VALUES ('EDIT_WORKING_HOURS', 'Edit Working Hours', N'تعديل ساعات العمل', '', '')
                END
                IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Code] = 'MANAGE_WORKING_HOURS_EXCEPTIONS')
                BEGIN
                    INSERT INTO [Permissions] ([Code], [NameEn], [NameAr], [CreatedBy], [UpdatedBy])
                    VALUES ('MANAGE_WORKING_HOURS_EXCEPTIONS', 'Manage Working Hours Exceptions', N'إدارة استثناءات ساعات العمل', '', '')
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM [RolePermissions] WHERE [PermissionId] IN (
                    SELECT Id FROM [Permissions] WHERE [Code] IN (
                        'APPROVE_PURCHASE_ORDER','CANCEL_PURCHASE_ORDER','APPROVE_SALE_ORDER','CANCEL_SALE_ORDER',
                        'VIEW_ACTIVITY_LOGS','EXPORT_ACTIVITY_LOGS','VIEW_WORKING_HOURS','EDIT_WORKING_HOURS','MANAGE_WORKING_HOURS_EXCEPTIONS'
                    )
                );
                DELETE FROM [Permissions] WHERE [Code] IN (
                    'APPROVE_PURCHASE_ORDER','CANCEL_PURCHASE_ORDER','APPROVE_SALE_ORDER','CANCEL_SALE_ORDER',
                    'VIEW_ACTIVITY_LOGS','EXPORT_ACTIVITY_LOGS','VIEW_WORKING_HOURS','EDIT_WORKING_HOURS','MANAGE_WORKING_HOURS_EXCEPTIONS'
                );
            ");
        }
    }
}
