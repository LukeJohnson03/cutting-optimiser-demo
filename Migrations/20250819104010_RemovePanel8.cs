using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuttingOptimiserDemo.Migrations
{
    /// <inheritdoc />
    public partial class RemovePanel8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Panels",
                keyColumn: "Id",
                keyValue: 8);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Panels",
                columns: new[] { "Id", "Height", "Quantity", "Width" },
                values: new object[] { 8, 926.0, 2, 1726.0 });
        }
    }
}
