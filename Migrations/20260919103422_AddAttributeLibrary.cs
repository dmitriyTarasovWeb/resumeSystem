using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace resumeSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAttributeLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    is_display = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.pk);
                });

            migrationBuilder.CreateTable(
                name: "data_types",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_type = table.Column<string>(type: "text", nullable: false),
                    is_display = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_types", x => x.pk);
                });

            migrationBuilder.CreateTable(
                name: "attributes",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    data_type = table.Column<int>(type: "integer", nullable: false),
                    is_display = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attributes", x => x.pk);
                    table.ForeignKey(
                        name: "FK_attributes_categories_category",
                        column: x => x.category,
                        principalTable: "categories",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_attributes_data_types_data_type",
                        column: x => x.data_type,
                        principalTable: "data_types",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attributes_category",
                table: "attributes",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_attributes_data_type",
                table: "attributes",
                column: "data_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attributes");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "data_types");
        }
    }
}
