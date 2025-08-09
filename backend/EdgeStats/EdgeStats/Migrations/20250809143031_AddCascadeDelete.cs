using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EdgeStats.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SportType",
                table: "Leagues",
                newName: "Sport");

            migrationBuilder.AddColumn<string>(
                name: "SportsbookUrl",
                table: "Sportsbooks",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LeagueCode",
                table: "Leagues",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "GameUrls",
                columns: table => new
                {
                    GameUrlId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    SportsbookId = table.Column<int>(type: "integer", nullable: false),
                    GameUrlValue = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameUrls", x => x.GameUrlId);
                    table.ForeignKey(
                        name: "FK_GameUrls_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameUrls_Sportsbooks_SportsbookId",
                        column: x => x.SportsbookId,
                        principalTable: "Sportsbooks",
                        principalColumn: "SportsbookId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SportsbookUrls",
                columns: table => new
                {
                    UrlId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SportsbookId = table.Column<int>(type: "integer", nullable: false),
                    LeagueId = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SportsbookUrls", x => x.UrlId);
                    table.ForeignKey(
                        name: "FK_SportsbookUrls_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SportsbookUrls_Sportsbooks_SportsbookId",
                        column: x => x.SportsbookId,
                        principalTable: "Sportsbooks",
                        principalColumn: "SportsbookId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameUrls_GameId",
                table: "GameUrls",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameUrls_SportsbookId",
                table: "GameUrls",
                column: "SportsbookId");

            migrationBuilder.CreateIndex(
                name: "IX_SportsbookUrls_LeagueId",
                table: "SportsbookUrls",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_SportsbookUrls_SportsbookId",
                table: "SportsbookUrls",
                column: "SportsbookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameUrls");

            migrationBuilder.DropTable(
                name: "SportsbookUrls");

            migrationBuilder.DropColumn(
                name: "SportsbookUrl",
                table: "Sportsbooks");

            migrationBuilder.DropColumn(
                name: "LeagueCode",
                table: "Leagues");

            migrationBuilder.RenameColumn(
                name: "Sport",
                table: "Leagues",
                newName: "SportType");
        }
    }
}
