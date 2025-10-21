using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Warehousing.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderTypeSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "OrderTypes",
                columns: new[] { "Id", "Description", "IsActive", "NameAr", "NameEn" },
                values: new object[,]
                {
                    { 1, "Order for purchasing products from suppliers", true, "أمر شراء", "Purchase Order" },
                    { 2, "Order for selling products to customers", true, "أمر بيع", "Sale Order" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrderTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "OrderTypes",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
