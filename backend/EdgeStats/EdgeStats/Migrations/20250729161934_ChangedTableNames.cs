using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdgeStats.Migrations
{
    /// <inheritdoc />
    public partial class ChangedTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_game_league_league_id",
                table: "game");

            migrationBuilder.DropForeignKey(
                name: "FK_game_team_team_1",
                table: "game");

            migrationBuilder.DropForeignKey(
                name: "FK_game_team_team_2",
                table: "game");

            migrationBuilder.DropForeignKey(
                name: "FK_line_prop_prop_id",
                table: "line");

            migrationBuilder.DropForeignKey(
                name: "FK_line_sportsbook_sportsbook_id",
                table: "line");

            migrationBuilder.DropForeignKey(
                name: "FK_prop_game_game_id",
                table: "prop");

            migrationBuilder.DropForeignKey(
                name: "FK_team_league_league_id",
                table: "team");

            migrationBuilder.DropForeignKey(
                name: "FK_WatchlistItems_line_LineId",
                table: "WatchlistItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_team",
                table: "team");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sportsbook",
                table: "sportsbook");

            migrationBuilder.DropPrimaryKey(
                name: "PK_prop",
                table: "prop");

            migrationBuilder.DropPrimaryKey(
                name: "PK_line",
                table: "line");

            migrationBuilder.DropPrimaryKey(
                name: "PK_league",
                table: "league");

            migrationBuilder.DropPrimaryKey(
                name: "PK_game",
                table: "game");

            migrationBuilder.RenameTable(
                name: "team",
                newName: "Teams");

            migrationBuilder.RenameTable(
                name: "sportsbook",
                newName: "Sportsbooks");

            migrationBuilder.RenameTable(
                name: "prop",
                newName: "Props");

            migrationBuilder.RenameTable(
                name: "line",
                newName: "Lines");

            migrationBuilder.RenameTable(
                name: "league",
                newName: "Leagues");

            migrationBuilder.RenameTable(
                name: "game",
                newName: "Games");

            migrationBuilder.RenameColumn(
                name: "team_name",
                table: "Teams",
                newName: "TeamName");

            migrationBuilder.RenameColumn(
                name: "league_id",
                table: "Teams",
                newName: "LeagueId");

            migrationBuilder.RenameColumn(
                name: "team_id",
                table: "Teams",
                newName: "TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_team_league_id",
                table: "Teams",
                newName: "IX_Teams_LeagueId");

            migrationBuilder.RenameColumn(
                name: "sportsbook_name",
                table: "Sportsbooks",
                newName: "SportsbookName");

            migrationBuilder.RenameColumn(
                name: "sportsbook_id",
                table: "Sportsbooks",
                newName: "SportsbookId");

            migrationBuilder.RenameColumn(
                name: "prop_uuid",
                table: "Props",
                newName: "PropUuid");

            migrationBuilder.RenameColumn(
                name: "prop_type",
                table: "Props",
                newName: "PropType");

            migrationBuilder.RenameColumn(
                name: "prop_name",
                table: "Props",
                newName: "PropName");

            migrationBuilder.RenameColumn(
                name: "game_id",
                table: "Props",
                newName: "GameId");

            migrationBuilder.RenameColumn(
                name: "prop_id",
                table: "Props",
                newName: "PropId");

            migrationBuilder.RenameIndex(
                name: "IX_prop_game_id",
                table: "Props",
                newName: "IX_Props_GameId");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Lines",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "sportsbook_id",
                table: "Lines",
                newName: "SportsbookId");

            migrationBuilder.RenameColumn(
                name: "prop_id",
                table: "Lines",
                newName: "PropId");

            migrationBuilder.RenameColumn(
                name: "line_uuid",
                table: "Lines",
                newName: "LineUuid");

            migrationBuilder.RenameColumn(
                name: "line_id",
                table: "Lines",
                newName: "LineId");

            migrationBuilder.RenameIndex(
                name: "IX_line_sportsbook_id",
                table: "Lines",
                newName: "IX_Lines_SportsbookId");

            migrationBuilder.RenameIndex(
                name: "IX_line_prop_id",
                table: "Lines",
                newName: "IX_Lines_PropId");

            migrationBuilder.RenameColumn(
                name: "sport_type",
                table: "Leagues",
                newName: "SportType");

            migrationBuilder.RenameColumn(
                name: "league_name",
                table: "Leagues",
                newName: "LeagueName");

            migrationBuilder.RenameColumn(
                name: "league_id",
                table: "Leagues",
                newName: "LeagueId");

            migrationBuilder.RenameColumn(
                name: "team_2",
                table: "Games",
                newName: "Team2Id");

            migrationBuilder.RenameColumn(
                name: "team_1",
                table: "Games",
                newName: "Team1Id");

            migrationBuilder.RenameColumn(
                name: "league_id",
                table: "Games",
                newName: "LeagueId");

            migrationBuilder.RenameColumn(
                name: "game_uuid",
                table: "Games",
                newName: "GameUuid");

            migrationBuilder.RenameColumn(
                name: "game_datetime",
                table: "Games",
                newName: "GameDateTime");

            migrationBuilder.RenameColumn(
                name: "game_id",
                table: "Games",
                newName: "GameId");

            migrationBuilder.RenameIndex(
                name: "IX_game_team_2",
                table: "Games",
                newName: "IX_Games_Team2Id");

            migrationBuilder.RenameIndex(
                name: "IX_game_team_1",
                table: "Games",
                newName: "IX_Games_Team1Id");

            migrationBuilder.RenameIndex(
                name: "IX_game_league_id",
                table: "Games",
                newName: "IX_Games_LeagueId");

            migrationBuilder.AlterColumn<string>(
                name: "PropType",
                table: "Props",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Teams",
                table: "Teams",
                column: "TeamId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sportsbooks",
                table: "Sportsbooks",
                column: "SportsbookId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Props",
                table: "Props",
                column: "PropId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lines",
                table: "Lines",
                column: "LineId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Leagues",
                table: "Leagues",
                column: "LeagueId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Games",
                table: "Games",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Leagues_LeagueId",
                table: "Games",
                column: "LeagueId",
                principalTable: "Leagues",
                principalColumn: "LeagueId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Teams_Team1Id",
                table: "Games",
                column: "Team1Id",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Teams_Team2Id",
                table: "Games",
                column: "Team2Id",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lines_Props_PropId",
                table: "Lines",
                column: "PropId",
                principalTable: "Props",
                principalColumn: "PropId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lines_Sportsbooks_SportsbookId",
                table: "Lines",
                column: "SportsbookId",
                principalTable: "Sportsbooks",
                principalColumn: "SportsbookId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Props_Games_GameId",
                table: "Props",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "GameId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Leagues_LeagueId",
                table: "Teams",
                column: "LeagueId",
                principalTable: "Leagues",
                principalColumn: "LeagueId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WatchlistItems_Lines_LineId",
                table: "WatchlistItems",
                column: "LineId",
                principalTable: "Lines",
                principalColumn: "LineId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Leagues_LeagueId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Teams_Team1Id",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Teams_Team2Id",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Lines_Props_PropId",
                table: "Lines");

            migrationBuilder.DropForeignKey(
                name: "FK_Lines_Sportsbooks_SportsbookId",
                table: "Lines");

            migrationBuilder.DropForeignKey(
                name: "FK_Props_Games_GameId",
                table: "Props");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Leagues_LeagueId",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_WatchlistItems_Lines_LineId",
                table: "WatchlistItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Teams",
                table: "Teams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sportsbooks",
                table: "Sportsbooks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Props",
                table: "Props");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lines",
                table: "Lines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Leagues",
                table: "Leagues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Games",
                table: "Games");

            migrationBuilder.RenameTable(
                name: "Teams",
                newName: "team");

            migrationBuilder.RenameTable(
                name: "Sportsbooks",
                newName: "sportsbook");

            migrationBuilder.RenameTable(
                name: "Props",
                newName: "prop");

            migrationBuilder.RenameTable(
                name: "Lines",
                newName: "line");

            migrationBuilder.RenameTable(
                name: "Leagues",
                newName: "league");

            migrationBuilder.RenameTable(
                name: "Games",
                newName: "game");

            migrationBuilder.RenameColumn(
                name: "TeamName",
                table: "team",
                newName: "team_name");

            migrationBuilder.RenameColumn(
                name: "LeagueId",
                table: "team",
                newName: "league_id");

            migrationBuilder.RenameColumn(
                name: "TeamId",
                table: "team",
                newName: "team_id");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_LeagueId",
                table: "team",
                newName: "IX_team_league_id");

            migrationBuilder.RenameColumn(
                name: "SportsbookName",
                table: "sportsbook",
                newName: "sportsbook_name");

            migrationBuilder.RenameColumn(
                name: "SportsbookId",
                table: "sportsbook",
                newName: "sportsbook_id");

            migrationBuilder.RenameColumn(
                name: "PropUuid",
                table: "prop",
                newName: "prop_uuid");

            migrationBuilder.RenameColumn(
                name: "PropType",
                table: "prop",
                newName: "prop_type");

            migrationBuilder.RenameColumn(
                name: "PropName",
                table: "prop",
                newName: "prop_name");

            migrationBuilder.RenameColumn(
                name: "GameId",
                table: "prop",
                newName: "game_id");

            migrationBuilder.RenameColumn(
                name: "PropId",
                table: "prop",
                newName: "prop_id");

            migrationBuilder.RenameIndex(
                name: "IX_Props_GameId",
                table: "prop",
                newName: "IX_prop_game_id");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "line",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "SportsbookId",
                table: "line",
                newName: "sportsbook_id");

            migrationBuilder.RenameColumn(
                name: "PropId",
                table: "line",
                newName: "prop_id");

            migrationBuilder.RenameColumn(
                name: "LineUuid",
                table: "line",
                newName: "line_uuid");

            migrationBuilder.RenameColumn(
                name: "LineId",
                table: "line",
                newName: "line_id");

            migrationBuilder.RenameIndex(
                name: "IX_Lines_SportsbookId",
                table: "line",
                newName: "IX_line_sportsbook_id");

            migrationBuilder.RenameIndex(
                name: "IX_Lines_PropId",
                table: "line",
                newName: "IX_line_prop_id");

            migrationBuilder.RenameColumn(
                name: "SportType",
                table: "league",
                newName: "sport_type");

            migrationBuilder.RenameColumn(
                name: "LeagueName",
                table: "league",
                newName: "league_name");

            migrationBuilder.RenameColumn(
                name: "LeagueId",
                table: "league",
                newName: "league_id");

            migrationBuilder.RenameColumn(
                name: "Team2Id",
                table: "game",
                newName: "team_2");

            migrationBuilder.RenameColumn(
                name: "Team1Id",
                table: "game",
                newName: "team_1");

            migrationBuilder.RenameColumn(
                name: "LeagueId",
                table: "game",
                newName: "league_id");

            migrationBuilder.RenameColumn(
                name: "GameUuid",
                table: "game",
                newName: "game_uuid");

            migrationBuilder.RenameColumn(
                name: "GameDateTime",
                table: "game",
                newName: "game_datetime");

            migrationBuilder.RenameColumn(
                name: "GameId",
                table: "game",
                newName: "game_id");

            migrationBuilder.RenameIndex(
                name: "IX_Games_Team2Id",
                table: "game",
                newName: "IX_game_team_2");

            migrationBuilder.RenameIndex(
                name: "IX_Games_Team1Id",
                table: "game",
                newName: "IX_game_team_1");

            migrationBuilder.RenameIndex(
                name: "IX_Games_LeagueId",
                table: "game",
                newName: "IX_game_league_id");

            migrationBuilder.AlterColumn<string>(
                name: "prop_type",
                table: "prop",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_team",
                table: "team",
                column: "team_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sportsbook",
                table: "sportsbook",
                column: "sportsbook_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_prop",
                table: "prop",
                column: "prop_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_line",
                table: "line",
                column: "line_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_league",
                table: "league",
                column: "league_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_game",
                table: "game",
                column: "game_id");

            migrationBuilder.AddForeignKey(
                name: "FK_game_league_league_id",
                table: "game",
                column: "league_id",
                principalTable: "league",
                principalColumn: "league_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_game_team_team_1",
                table: "game",
                column: "team_1",
                principalTable: "team",
                principalColumn: "team_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_game_team_team_2",
                table: "game",
                column: "team_2",
                principalTable: "team",
                principalColumn: "team_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_line_prop_prop_id",
                table: "line",
                column: "prop_id",
                principalTable: "prop",
                principalColumn: "prop_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_line_sportsbook_sportsbook_id",
                table: "line",
                column: "sportsbook_id",
                principalTable: "sportsbook",
                principalColumn: "sportsbook_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_prop_game_game_id",
                table: "prop",
                column: "game_id",
                principalTable: "game",
                principalColumn: "game_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_team_league_league_id",
                table: "team",
                column: "league_id",
                principalTable: "league",
                principalColumn: "league_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WatchlistItems_line_LineId",
                table: "WatchlistItems",
                column: "LineId",
                principalTable: "line",
                principalColumn: "line_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
