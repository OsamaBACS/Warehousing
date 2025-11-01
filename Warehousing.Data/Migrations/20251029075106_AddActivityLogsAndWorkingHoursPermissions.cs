using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehousing.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityLogsAndWorkingHoursPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert Activity Logs and Working Hours permissions if they don't exist
            migrationBuilder.Sql(@"
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
            // Remove inserted permissions
            migrationBuilder.Sql(@"
                DELETE FROM [RolePermissions] WHERE [PermissionId] IN (
                    SELECT Id FROM [Permissions] WHERE [Code] IN (
                        'VIEW_ACTIVITY_LOGS','EXPORT_ACTIVITY_LOGS','VIEW_WORKING_HOURS','EDIT_WORKING_HOURS','MANAGE_WORKING_HOURS_EXCEPTIONS'
                    )
                );
                DELETE FROM [Permissions] WHERE [Code] IN (
                    'VIEW_ACTIVITY_LOGS','EXPORT_ACTIVITY_LOGS','VIEW_WORKING_HOURS','EDIT_WORKING_HOURS','MANAGE_WORKING_HOURS_EXCEPTIONS'
                );
            ");
        }
    }
}
