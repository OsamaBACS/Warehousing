using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehousing.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreatePrinterConfigurationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrinterConfiguration",
                table: "Companies");

            migrationBuilder.AddColumn<int>(
                name: "PrinterConfigurationId",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PrinterConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrinterType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaperFormat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaperWidth = table.Column<int>(type: "int", nullable: false),
                    PaperHeight = table.Column<int>(type: "int", nullable: false),
                    Margins = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FontSettings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PosSettings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrintInColor = table.Column<bool>(type: "bit", nullable: false),
                    PrintBackground = table.Column<bool>(type: "bit", nullable: false),
                    Orientation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Scale = table.Column<double>(type: "float", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrinterConfigurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_PrinterConfigurationId",
                table: "Roles",
                column: "PrinterConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_PrinterConfigurations_PrinterConfigurationId",
                table: "Roles",
                column: "PrinterConfigurationId",
                principalTable: "PrinterConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_PrinterConfigurations_PrinterConfigurationId",
                table: "Roles");

            migrationBuilder.DropTable(
                name: "PrinterConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_Roles_PrinterConfigurationId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "PrinterConfigurationId",
                table: "Roles");

            migrationBuilder.AddColumn<string>(
                name: "PrinterConfiguration",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
