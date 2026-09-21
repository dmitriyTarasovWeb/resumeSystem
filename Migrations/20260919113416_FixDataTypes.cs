using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace resumeSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixDataTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_display",
                table: "data_types");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_display",
                table: "data_types",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 1,
                column: "is_display",
                value: true);

            migrationBuilder.UpdateData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 2,
                column: "is_display",
                value: true);

            migrationBuilder.UpdateData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 3,
                column: "is_display",
                value: true);

            migrationBuilder.UpdateData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 4,
                column: "is_display",
                value: true);

            migrationBuilder.UpdateData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 5,
                column: "is_display",
                value: true);

            migrationBuilder.UpdateData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 6,
                column: "is_display",
                value: true);

            migrationBuilder.UpdateData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 7,
                column: "is_display",
                value: true);

            migrationBuilder.UpdateData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 8,
                column: "is_display",
                value: true);
        }
    }
}
