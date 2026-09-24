using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace resumeSystem.Migrations
{
    /// <inheritdoc />
    public partial class SeedCompareTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "compare_types",
                columns: new[] { "pk", "compare_type" },
                values: new object[,]
                {
                    { 1, "Equals" },
                    { 2, "Not equals" },
                    { 3, "Contains" },
                    { 4, "Greater than" },
                    { 5, "Less than" },
                    { 6, "Greater than or equal" },
                    { 7, "Less than or equal" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "compare_types",
                keyColumn: "pk",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "compare_types",
                keyColumn: "pk",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "compare_types",
                keyColumn: "pk",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "compare_types",
                keyColumn: "pk",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "compare_types",
                keyColumn: "pk",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "compare_types",
                keyColumn: "pk",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "compare_types",
                keyColumn: "pk",
                keyValue: 7);
        }
    }
}
