using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace resumeSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMultipleChoiceDataType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "data_types",
                keyColumn: "pk",
                keyValue: 8);

            migrationBuilder.CreateTable(
                name: "CompareTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompareTypeName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompareTypes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompareTypes");

            migrationBuilder.InsertData(
                table: "data_types",
                columns: new[] { "pk", "data_type" },
                values: new object[] { 8, "Multiple choice" });
        }
    }
}
