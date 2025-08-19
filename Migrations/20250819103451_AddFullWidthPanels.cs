using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CuttingOptimiserDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddFullWidthPanels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Panels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Width = table.Column<double>(type: "REAL", nullable: false),
                    Height = table.Column<double>(type: "REAL", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Panels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockSheets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Width = table.Column<double>(type: "REAL", nullable: false),
                    Height = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSheets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CutSegments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartX = table.Column<double>(type: "REAL", nullable: false),
                    StartY = table.Column<double>(type: "REAL", nullable: false),
                    EndX = table.Column<double>(type: "REAL", nullable: false),
                    EndY = table.Column<double>(type: "REAL", nullable: false),
                    IsFullLengthX = table.Column<bool>(type: "INTEGER", nullable: false),
                    StockSheetId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CutSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CutSegments_StockSheets_StockSheetId",
                        column: x => x.StockSheetId,
                        principalTable: "StockSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Placements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    X = table.Column<double>(type: "REAL", nullable: false),
                    Y = table.Column<double>(type: "REAL", nullable: false),
                    PanelId = table.Column<int>(type: "INTEGER", nullable: false),
                    StockSheetId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Placements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Placements_Panels_PanelId",
                        column: x => x.PanelId,
                        principalTable: "Panels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Placements_StockSheets_StockSheetId",
                        column: x => x.StockSheetId,
                        principalTable: "StockSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Panels",
                columns: new[] { "Id", "Height", "Quantity", "Width" },
                values: new object[,]
                {
                    { 1, 484.0, 8, 700.0 },
                    { 2, 249.0, 4, 501.0 },
                    { 3, 675.0, 2, 1132.0 },
                    { 4, 433.0, 2, 485.0 },
                    { 5, 433.0, 1, 485.0 },
                    { 6, 466.0, 5, 522.0 },
                    { 7, 1756.0, 2, 362.0 },
                    { 8, 926.0, 2, 1726.0 },
                    { 10, 400.0, 1, 3170.0 },
                    { 11, 350.0, 1, 2400.0 }
                });

            migrationBuilder.InsertData(
                table: "StockSheets",
                columns: new[] { "Id", "Height", "Width" },
                values: new object[,]
                {
                    { 1, 1219.0, 1862.0 },
                    { 2, 2440.0, 3210.0 },
                    { 3, 2250.0, 3210.0 },
                    { 4, 1830.0, 2440.0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CutSegments_StockSheetId",
                table: "CutSegments",
                column: "StockSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_Placements_PanelId",
                table: "Placements",
                column: "PanelId");

            migrationBuilder.CreateIndex(
                name: "IX_Placements_StockSheetId",
                table: "Placements",
                column: "StockSheetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CutSegments");

            migrationBuilder.DropTable(
                name: "Placements");

            migrationBuilder.DropTable(
                name: "Panels");

            migrationBuilder.DropTable(
                name: "StockSheets");
        }
    }
}
