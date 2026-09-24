using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace resumeSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCompareTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompareTypes",
                table: "CompareTypes");

            migrationBuilder.RenameTable(
                name: "CompareTypes",
                newName: "compare_types");

            migrationBuilder.RenameColumn(
                name: "CompareTypeName",
                table: "compare_types",
                newName: "compare_type");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "compare_types",
                newName: "pk");

            migrationBuilder.AddPrimaryKey(
                name: "PK_compare_types",
                table: "compare_types",
                column: "pk");

            migrationBuilder.CreateTable(
                name: "data_type_compare_types",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_type = table.Column<int>(type: "integer", nullable: false),
                    compare_type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_type_compare_types", x => x.pk);
                    table.ForeignKey(
                        name: "FK_data_type_compare_types_compare_types_compare_type",
                        column: x => x.compare_type,
                        principalTable: "compare_types",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_data_type_compare_types_data_types_data_type",
                        column: x => x.data_type,
                        principalTable: "data_types",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_data_type_compare_types_compare_type",
                table: "data_type_compare_types",
                column: "compare_type");

            migrationBuilder.CreateIndex(
                name: "IX_data_type_compare_types_data_type",
                table: "data_type_compare_types",
                column: "data_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_type_compare_types");

            migrationBuilder.DropPrimaryKey(
                name: "PK_compare_types",
                table: "compare_types");

            migrationBuilder.RenameTable(
                name: "compare_types",
                newName: "CompareTypes");

            migrationBuilder.RenameColumn(
                name: "compare_type",
                table: "CompareTypes",
                newName: "CompareTypeName");

            migrationBuilder.RenameColumn(
                name: "pk",
                table: "CompareTypes",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompareTypes",
                table: "CompareTypes",
                column: "Id");
        }
    }
}
