using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehousing.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePrinterConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrinterConfiguration",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrinterConfiguration",
                table: "Companies");
        }
    }
}
