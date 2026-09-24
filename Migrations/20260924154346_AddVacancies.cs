using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace resumeSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddVacancies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vacancies",
                columns: table => new
                {
                    pk = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_user_id = table.Column<string>(type: "text", nullable: false),
                    fk_position_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vacancies", x => x.pk);
                    table.ForeignKey(
                        name: "FK_vacancies_AspNetUsers_fk_user_id",
                        column: x => x.fk_user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vacancies_positions_fk_position_id",
                        column: x => x.fk_position_id,
                        principalTable: "positions",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vacancy_attributes",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_vacancy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_attribute_id = table.Column<int>(type: "integer", nullable: false),
                    attribute_value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vacancy_attributes", x => x.pk);
                    table.ForeignKey(
                        name: "FK_vacancy_attributes_attributes_fk_attribute_id",
                        column: x => x.fk_attribute_id,
                        principalTable: "attributes",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vacancy_attributes_vacancies_fk_vacancy_id",
                        column: x => x.fk_vacancy_id,
                        principalTable: "vacancies",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vacancy_tags",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_vacancy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_tag_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vacancy_tags", x => x.pk);
                    table.ForeignKey(
                        name: "FK_vacancy_tags_tags_fk_tag_id",
                        column: x => x.fk_tag_id,
                        principalTable: "tags",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vacancy_tags_vacancies_fk_vacancy_id",
                        column: x => x.fk_vacancy_id,
                        principalTable: "vacancies",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vacancies_fk_position_id",
                table: "vacancies",
                column: "fk_position_id");

            migrationBuilder.CreateIndex(
                name: "IX_vacancies_fk_user_id",
                table: "vacancies",
                column: "fk_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_vacancy_attributes_fk_attribute_id",
                table: "vacancy_attributes",
                column: "fk_attribute_id");

            migrationBuilder.CreateIndex(
                name: "IX_vacancy_attributes_fk_vacancy_id",
                table: "vacancy_attributes",
                column: "fk_vacancy_id");

            migrationBuilder.CreateIndex(
                name: "IX_vacancy_tags_fk_tag_id",
                table: "vacancy_tags",
                column: "fk_tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_vacancy_tags_fk_vacancy_id",
                table: "vacancy_tags",
                column: "fk_vacancy_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vacancy_attributes");

            migrationBuilder.DropTable(
                name: "vacancy_tags");

            migrationBuilder.DropTable(
                name: "vacancies");
        }
    }
}
