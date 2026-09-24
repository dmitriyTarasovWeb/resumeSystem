using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace resumeSystem.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "pk", "is_display", "title" },
                values: new object[] { 1, true, "Общие" });

            migrationBuilder.InsertData(
                table: "attributes",
                columns: new[] { "pk", "category", "data_type", "is_display", "title" },
                values: new object[,]
                {
                    { 1, 1, 1, true, "Любимый язык программирования" },
                    { 2, 1, 2, true, "О себе" },
                    { 3, 1, 3, true, "Количество лет опыта" },
                    { 4, 1, 4, true, "Дата начала карьеры" },
                    { 5, 1, 5, true, "Период работы" },
                    { 6, 1, 6, true, "Готов к удалённой работе" },
                    { 7, 1, 7, true, "Уровень английского" }
                });

            migrationBuilder.InsertData(
                table: "attribute_options",
                columns: new[] { "pk", "attribute", "options" },
                values: new object[,]
                {
                    { 1, 7, "A1" },
                    { 2, 7, "A2" },
                    { 3, 7, "B1" },
                    { 4, 7, "B2" },
                    { 5, 7, "C1" },
                    { 6, 7, "C2" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "attribute_options",
                keyColumn: "pk",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "attribute_options",
                keyColumn: "pk",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "attribute_options",
                keyColumn: "pk",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "attribute_options",
                keyColumn: "pk",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "attribute_options",
                keyColumn: "pk",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "attribute_options",
                keyColumn: "pk",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "attributes",
                keyColumn: "pk",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "attributes",
                keyColumn: "pk",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "attributes",
                keyColumn: "pk",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "attributes",
                keyColumn: "pk",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "attributes",
                keyColumn: "pk",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "attributes",
                keyColumn: "pk",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "attributes",
                keyColumn: "pk",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "pk",
                keyValue: 1);
        }
    }
}
