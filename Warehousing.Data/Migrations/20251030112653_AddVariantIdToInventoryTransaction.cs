using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehousing.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantIdToInventoryTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "InventoryTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_VariantId",
                table: "InventoryTransactions",
                column: "VariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_ProductVariants_VariantId",
                table: "InventoryTransactions",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_ProductVariants_VariantId",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_VariantId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "InventoryTransactions");
        }
    }
}
