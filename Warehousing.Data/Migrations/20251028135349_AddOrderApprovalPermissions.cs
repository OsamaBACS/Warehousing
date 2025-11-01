using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehousing.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderApprovalPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert order approval/cancellation permissions if they don't exist
            migrationBuilder.Sql(@"
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
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove inserted permissions
            migrationBuilder.Sql(@"
                DELETE FROM [RolePermissions] WHERE [PermissionId] IN (
                    SELECT Id FROM [Permissions] WHERE [Code] IN (
                        'APPROVE_PURCHASE_ORDER','CANCEL_PURCHASE_ORDER','APPROVE_SALE_ORDER','CANCEL_SALE_ORDER'
                    )
                );
                DELETE FROM [Permissions] WHERE [Code] IN (
                    'APPROVE_PURCHASE_ORDER','CANCEL_PURCHASE_ORDER','APPROVE_SALE_ORDER','CANCEL_SALE_ORDER'
                );
            ");
        }
    }
}
