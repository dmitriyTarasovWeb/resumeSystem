using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace resumeSystem.Migrations
{
    /// <inheritdoc />
    public partial class RestoreBeforeLoose : Migration
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
                name: "compare_types",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    compare_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compare_types", x => x.pk);
                });

            migrationBuilder.CreateTable(
                name: "data_types",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_type = table.Column<string>(type: "text", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "attribute_options",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    attribute = table.Column<int>(type: "integer", nullable: false),
                    options = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attribute_options", x => x.pk);
                    table.ForeignKey(
                        name: "FK_attribute_options_attributes_attribute",
                        column: x => x.attribute,
                        principalTable: "attributes",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_attributes",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user = table.Column<string>(type: "text", nullable: false),
                    attribute = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_attributes", x => x.pk);
                    table.ForeignKey(
                        name: "FK_user_attributes_AspNetUsers_user",
                        column: x => x.user,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_attributes_attributes_attribute",
                        column: x => x.attribute,
                        principalTable: "attributes",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "pk", "is_display", "title" },
                values: new object[] { 1, true, "Общие" });

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

            migrationBuilder.InsertData(
                table: "data_types",
                columns: new[] { "pk", "data_type" },
                values: new object[,]
                {
                    { 1, "String" },
                    { 2, "Text" },
                    { 3, "Number" },
                    { 4, "Date" },
                    { 5, "Period" },
                    { 6, "Boolean" },
                    { 7, "One of many" }
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_attribute_options_attribute",
                table: "attribute_options",
                column: "attribute");

            migrationBuilder.CreateIndex(
                name: "IX_attributes_category",
                table: "attributes",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_attributes_data_type",
                table: "attributes",
                column: "data_type");

            migrationBuilder.CreateIndex(
                name: "IX_data_type_compare_types_compare_type",
                table: "data_type_compare_types",
                column: "compare_type");

            migrationBuilder.CreateIndex(
                name: "IX_data_type_compare_types_data_type",
                table: "data_type_compare_types",
                column: "data_type");

            migrationBuilder.CreateIndex(
                name: "IX_user_attributes_attribute",
                table: "user_attributes",
                column: "attribute");

            migrationBuilder.CreateIndex(
                name: "IX_user_attributes_user",
                table: "user_attributes",
                column: "user");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attribute_options");

            migrationBuilder.DropTable(
                name: "data_type_compare_types");

            migrationBuilder.DropTable(
                name: "user_attributes");

            migrationBuilder.DropTable(
                name: "compare_types");

            migrationBuilder.DropTable(
                name: "attributes");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "data_types");
        }
    }
}
