using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace resumeSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddResumes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "resumes",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    position_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resumes", x => x.pk);
                    table.ForeignKey(
                        name: "FK_resumes_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resumes_positions_position_id",
                        column: x => x.position_id,
                        principalTable: "positions",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "resume_attributes",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    resume_id = table.Column<int>(type: "integer", nullable: false),
                    user_attribute_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resume_attributes", x => x.pk);
                    table.ForeignKey(
                        name: "FK_resume_attributes_resumes_resume_id",
                        column: x => x.resume_id,
                        principalTable: "resumes",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resume_attributes_user_attributes_user_attribute_id",
                        column: x => x.user_attribute_id,
                        principalTable: "user_attributes",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vacancy_resumes",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    resume_id = table.Column<int>(type: "integer", nullable: false),
                    vacancy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    apply_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vacancy_resumes", x => x.pk);
                    table.ForeignKey(
                        name: "FK_vacancy_resumes_resumes_resume_id",
                        column: x => x.resume_id,
                        principalTable: "resumes",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vacancy_resumes_vacancies_vacancy_id",
                        column: x => x.vacancy_id,
                        principalTable: "vacancies",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_resume_attributes_resume_id",
                table: "resume_attributes",
                column: "resume_id");

            migrationBuilder.CreateIndex(
                name: "IX_resume_attributes_user_attribute_id",
                table: "resume_attributes",
                column: "user_attribute_id");

            migrationBuilder.CreateIndex(
                name: "IX_resumes_position_id",
                table: "resumes",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "IX_resumes_user_id",
                table: "resumes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_vacancy_resumes_resume_id",
                table: "vacancy_resumes",
                column: "resume_id");

            migrationBuilder.CreateIndex(
                name: "IX_vacancy_resumes_vacancy_id",
                table: "vacancy_resumes",
                column: "vacancy_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "resume_attributes");

            migrationBuilder.DropTable(
                name: "vacancy_resumes");

            migrationBuilder.DropTable(
                name: "resumes");
        }
    }
}
