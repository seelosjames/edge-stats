using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EdgeStats.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "league",
                columns: table => new
                {
                    league_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    league_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sport_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_league", x => x.league_id);
                });

            migrationBuilder.CreateTable(
                name: "sportsbook",
                columns: table => new
                {
                    sportsbook_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sportsbook_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sportsbook", x => x.sportsbook_id);
                });

            migrationBuilder.CreateTable(
                name: "team",
                columns: table => new
                {
                    team_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    team_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    league_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team", x => x.team_id);
                    table.ForeignKey(
                        name: "FK_team_league_league_id",
                        column: x => x.league_id,
                        principalTable: "league",
                        principalColumn: "league_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game",
                columns: table => new
                {
                    game_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    game_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    league_id = table.Column<int>(type: "integer", nullable: false),
                    team_1 = table.Column<int>(type: "integer", nullable: false),
                    team_2 = table.Column<int>(type: "integer", nullable: false),
                    game_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game", x => x.game_id);
                    table.ForeignKey(
                        name: "FK_game_league_league_id",
                        column: x => x.league_id,
                        principalTable: "league",
                        principalColumn: "league_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_game_team_team_1",
                        column: x => x.team_1,
                        principalTable: "team",
                        principalColumn: "team_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_game_team_team_2",
                        column: x => x.team_2,
                        principalTable: "team",
                        principalColumn: "team_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prop",
                columns: table => new
                {
                    prop_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prop_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<int>(type: "integer", nullable: false),
                    prop_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    prop_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prop", x => x.prop_id);
                    table.ForeignKey(
                        name: "FK_prop_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "game_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "line",
                columns: table => new
                {
                    line_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    line_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    prop_id = table.Column<int>(type: "integer", nullable: false),
                    sportsbook_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    odd = table.Column<decimal>(type: "numeric(8,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_line", x => x.line_id);
                    table.ForeignKey(
                        name: "FK_line_prop_prop_id",
                        column: x => x.prop_id,
                        principalTable: "prop",
                        principalColumn: "prop_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_line_sportsbook_sportsbook_id",
                        column: x => x.sportsbook_id,
                        principalTable: "sportsbook",
                        principalColumn: "sportsbook_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WatchlistItems",
                columns: table => new
                {
                    WatchListItemsId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LineId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchlistItems", x => x.WatchListItemsId);
                    table.ForeignKey(
                        name: "FK_WatchlistItems_line_LineId",
                        column: x => x.LineId,
                        principalTable: "line",
                        principalColumn: "line_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_game_league_id",
                table: "game",
                column: "league_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_team_1",
                table: "game",
                column: "team_1");

            migrationBuilder.CreateIndex(
                name: "IX_game_team_2",
                table: "game",
                column: "team_2");

            migrationBuilder.CreateIndex(
                name: "IX_line_prop_id",
                table: "line",
                column: "prop_id");

            migrationBuilder.CreateIndex(
                name: "IX_line_sportsbook_id",
                table: "line",
                column: "sportsbook_id");

            migrationBuilder.CreateIndex(
                name: "IX_prop_game_id",
                table: "prop",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_league_id",
                table: "team",
                column: "league_id");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistItems_LineId",
                table: "WatchlistItems",
                column: "LineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WatchlistItems");

            migrationBuilder.DropTable(
                name: "line");

            migrationBuilder.DropTable(
                name: "prop");

            migrationBuilder.DropTable(
                name: "sportsbook");

            migrationBuilder.DropTable(
                name: "game");

            migrationBuilder.DropTable(
                name: "team");

            migrationBuilder.DropTable(
                name: "league");
        }
    }
}
