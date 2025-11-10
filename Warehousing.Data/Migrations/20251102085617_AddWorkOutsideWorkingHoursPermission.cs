using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehousing.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOutsideWorkingHoursPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert WORK_OUTSIDE_WORKING_HOURS permission if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Code] = 'WORK_OUTSIDE_WORKING_HOURS')
                BEGIN
                    INSERT INTO [Permissions] ([Code], [NameEn], [NameAr], [CreatedBy], [UpdatedBy])
                    VALUES ('WORK_OUTSIDE_WORKING_HOURS', 'Work Outside Working Hours', N'العمل خارج ساعات العمل', '', '')
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the permission and its role assignments
            migrationBuilder.Sql(@"
                DELETE FROM [RolePermissions] WHERE [PermissionId] IN (
                    SELECT Id FROM [Permissions] WHERE [Code] = 'WORK_OUTSIDE_WORKING_HOURS'
                );
                DELETE FROM [Permissions] WHERE [Code] = 'WORK_OUTSIDE_WORKING_HOURS'
            ");
        }
    }
}
