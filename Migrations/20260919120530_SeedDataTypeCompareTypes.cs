using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace resumeSystem.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataTypeCompareTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "data_type_compare_types",
                columns: new[] { "pk", "compare_type", "data_type" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 1 },
                    { 3, 3, 1 },
                    { 4, 1, 2 },
                    { 5, 2, 2 },
                    { 6, 3, 2 },
                    { 7, 1, 3 },
                    { 8, 2, 3 },
                    { 9, 4, 3 },
                    { 10, 5, 3 },
                    { 11, 6, 3 },
                    { 12, 7, 3 },
                    { 13, 1, 4 },
                    { 14, 2, 4 },
                    { 15, 4, 4 },
                    { 16, 5, 4 },
                    { 17, 6, 4 },
                    { 18, 7, 4 },
                    { 19, 1, 5 },
                    { 20, 2, 5 },
                    { 21, 4, 5 },
                    { 22, 5, 5 },
                    { 23, 6, 5 },
                    { 24, 7, 5 },
                    { 25, 1, 6 },
                    { 26, 2, 6 },
                    { 27, 1, 7 },
                    { 28, 2, 7 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "data_type_compare_types",
                keyColumn: "pk",
                keyValue: 28);
        }
    }
}
