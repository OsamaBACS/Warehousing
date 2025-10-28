using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehousing.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleSubCategoriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleSubCategory_Roles_RoleId",
                table: "RoleSubCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleSubCategory_SubCategories_SubCategoryId",
                table: "RoleSubCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleSubCategory",
                table: "RoleSubCategory");

            migrationBuilder.RenameTable(
                name: "RoleSubCategory",
                newName: "RoleSubCategories");

            migrationBuilder.RenameIndex(
                name: "IX_RoleSubCategory_SubCategoryId",
                table: "RoleSubCategories",
                newName: "IX_RoleSubCategories_SubCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_RoleSubCategory_RoleId",
                table: "RoleSubCategories",
                newName: "IX_RoleSubCategories_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleSubCategories",
                table: "RoleSubCategories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleSubCategories_Roles_RoleId",
                table: "RoleSubCategories",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleSubCategories_SubCategories_SubCategoryId",
                table: "RoleSubCategories",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleSubCategories_Roles_RoleId",
                table: "RoleSubCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleSubCategories_SubCategories_SubCategoryId",
                table: "RoleSubCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleSubCategories",
                table: "RoleSubCategories");

            migrationBuilder.RenameTable(
                name: "RoleSubCategories",
                newName: "RoleSubCategory");

            migrationBuilder.RenameIndex(
                name: "IX_RoleSubCategories_SubCategoryId",
                table: "RoleSubCategory",
                newName: "IX_RoleSubCategory_SubCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_RoleSubCategories_RoleId",
                table: "RoleSubCategory",
                newName: "IX_RoleSubCategory_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleSubCategory",
                table: "RoleSubCategory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleSubCategory_Roles_RoleId",
                table: "RoleSubCategory",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleSubCategory_SubCategories_SubCategoryId",
                table: "RoleSubCategory",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
