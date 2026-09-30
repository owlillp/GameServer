using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClanService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "clan");

            migrationBuilder.CreateTable(
                name: "clans",
                schema: "clan",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    tag = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    description = table.Column<string>(type: "character varying(280)", maxLength: 280, nullable: true),
                    leader_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clan_members",
                schema: "clan",
                columns: table => new
                {
                    clan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clan_members", x => new { x.clan_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_clan_members_clans_clan_id",
                        column: x => x.clan_id,
                        principalSchema: "clan",
                        principalTable: "clans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clan_members_user_id",
                schema: "clan",
                table: "clan_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_clans_normalized_name",
                schema: "clan",
                table: "clans",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clans_tag",
                schema: "clan",
                table: "clans",
                column: "tag",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clan_members",
                schema: "clan");

            migrationBuilder.DropTable(
                name: "clans",
                schema: "clan");
        }
    }
}
