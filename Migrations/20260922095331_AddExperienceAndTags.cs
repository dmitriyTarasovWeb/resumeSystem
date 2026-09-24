using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace resumeSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddExperienceAndTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "positions",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    is_display = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_positions", x => x.pk);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    is_display = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.pk);
                });

            migrationBuilder.CreateTable(
                name: "experience",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_position_id = table.Column<int>(type: "integer", nullable: true),
                    fk_user_id = table.Column<string>(type: "text", nullable: false),
                    company_name = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_experience", x => x.pk);
                    table.ForeignKey(
                        name: "FK_experience_AspNetUsers_fk_user_id",
                        column: x => x.fk_user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_experience_positions_fk_position_id",
                        column: x => x.fk_position_id,
                        principalTable: "positions",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "experience_tags",
                columns: table => new
                {
                    pk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_experience_id = table.Column<int>(type: "integer", nullable: false),
                    fk_tag_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_experience_tags", x => x.pk);
                    table.ForeignKey(
                        name: "FK_experience_tags_experience_fk_experience_id",
                        column: x => x.fk_experience_id,
                        principalTable: "experience",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_experience_tags_tags_fk_tag_id",
                        column: x => x.fk_tag_id,
                        principalTable: "tags",
                        principalColumn: "pk",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_experience_fk_position_id",
                table: "experience",
                column: "fk_position_id");

            migrationBuilder.CreateIndex(
                name: "IX_experience_fk_user_id",
                table: "experience",
                column: "fk_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_experience_tags_fk_experience_id",
                table: "experience_tags",
                column: "fk_experience_id");

            migrationBuilder.CreateIndex(
                name: "IX_experience_tags_fk_tag_id",
                table: "experience_tags",
                column: "fk_tag_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "experience_tags");

            migrationBuilder.DropTable(
                name: "experience");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "positions");
        }
    }
}
