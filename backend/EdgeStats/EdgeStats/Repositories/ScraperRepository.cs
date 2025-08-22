using EdgeStats.Dtos;
using EdgeStats.Interfaces;
using EdgeStats.Models;
using Microsoft.EntityFrameworkCore;

namespace EdgeStats.Repositories
{
    public class ScraperRepository : IScraperRepository
    {
        private readonly EdgeStatsDbContext _dbContext;

        public ScraperRepository(EdgeStatsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<Team> InsertTeams(List<string> teams, League league, List<Team> newTeams)
        {
            var teamList = new List<Team>();

            var existingTeams = _dbContext.Teams
                .Where(t => t.LeagueId == league.LeagueId)
                .ToDictionary(t => t.TeamName);

            foreach (var teamName in teams)
            {

                if (!existingTeams.TryGetValue(teamName, out var existingTeam))
                {
                    var newTeam = new Team { TeamName = teamName, LeagueId = league.LeagueId };
                    existingTeams[teamName] = newTeam;
                    newTeams.Add(newTeam);
                    teamList.Add(newTeam);
                }
                else
                {
                    teamList.Add(existingTeam);
                }
            }

            return teamList;
        }

        public List<Game> InsertGames(ScrapedGameDto scrapedGame, League league, Team team1, Team team2)
        {
            var newGames = new List<Game>();

            var existingGames = _dbContext.Games
                .Include(g => g.GameUrls)
                .Where(g => g.LeagueId == league.LeagueId)
                .ToDictionary(g => g.GameUuid);

            if (!existingGames.TryGetValue(scrapedGame.GameUuid, out var game))
            {
                game = new Game
                {
                    GameUuid = scrapedGame.GameUuid,
                    LeagueId = league.LeagueId,
                    Team1 = team1,
                    Team2 = team2,
                    GameDateTime = scrapedGame.GameTime,
                    GameUrls = new List<GameUrl>
            {
                new GameUrl
                {
                    SportsbookId = scrapedGame.GameUrl.SportsbookId,
                    GameUrlValue = scrapedGame.GameUrl.GameUrl
                }
            }
                };
                existingGames[scrapedGame.GameUuid] = game;
                newGames.Add(game);
            }
            else
            {
                game.GameDateTime = scrapedGame.GameTime;
                if (!game.GameUrls.Any(gu => gu.GameUrlValue == scrapedGame.GameUrl.GameUrl))
                {
                    game.GameUrls.Add(new GameUrl
                    {
                        SportsbookId = scrapedGame.GameUrl.SportsbookId,
                        GameUrlValue = scrapedGame.GameUrl.GameUrl
                    });
                }
            }
            return newGames;
        }

        public async Task RemoveOldGamesAsync()
        {
            var cutoff = DateTime.UtcNow;
            var oldGames = await _dbContext.Games
                .Where(g => g.GameDateTime < cutoff)
                .ToListAsync();

            if (oldGames.Count > 0)
            {
                _dbContext.Games.RemoveRange(oldGames);
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"Deleted {oldGames.Count} old games.");
            }
        }
    }
}
