using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace resumeSystem.Migrations
{
    /// <inheritdoc />
    public partial class SeedAttributeDataTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "data_types",
                columns: new[] { "pk", "data_type", "is_display" },
                values: new object[,]
                {
                    { 1, "String", true },
                    { 2, "Text", true },
                    { 3, "Number", true },
                    { 4, "Date", true },
                    { 5, "Period", true },
                    { 6, "Boolean", true },
                    { 7, "One of many", true },
                    { 8, "Multiple choice", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 8);
        }
    }
}
