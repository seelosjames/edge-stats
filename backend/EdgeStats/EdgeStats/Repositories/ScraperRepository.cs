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

        public async Task<League> GetLeagueByCodeAsync(string leagueCode)
        {
            return await _dbContext.Leagues
                .FirstAsync(l => l.LeagueCode == leagueCode);
        }

        public async Task<SportsbookUrl> GetSportsbookUrlAsync(int leagueId, int sportsbookId)
        {
            return await _dbContext.SportsbookUrls
                .FirstAsync(su => su.LeagueId == leagueId && su.SportsbookId == sportsbookId);
        }

        public async Task SaveScrapedGamesAsync(List<ScrapedGameDto> scrapedGames, League league)
        {
            var newTeams = new List<Team>();
            var newGames = new List<Game>();

            foreach (var scrapedGame in scrapedGames)
            {
                var teamNames = new List<string> { scrapedGame.Team1, scrapedGame.Team2 };
                var scrapedTeams = InsertTeams(teamNames, league, newTeams);

                var createdGames = InsertGames(scrapedGame, league, scrapedTeams[0], scrapedTeams[1]);
                if (createdGames != null && createdGames.Any())
                {
                    newGames.AddRange(createdGames);
                }
            }

            if (newTeams.Count > 0)
            {
                _dbContext.Teams.AddRange(newTeams);
            }

            if (newGames.Count > 0)
            {
                _dbContext.Games.AddRange(newGames);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Game>> GetGamesByUuidsAsync(List<Guid> gameUuids)
        {
            return await _dbContext.Games
                .Include(g => g.GameUrls)
                .Include(g => g.Props).ThenInclude(p => p.Lines)
                .Where(g => gameUuids.Contains(g.GameUuid))
                .ToListAsync();
        }

        public async Task UpsertPropsAndLinesAsync(
            IEnumerable<ScrapedPropDto> scrapedProps,
            IEnumerable<ScrapedLineDto> scrapedLines,
            int sportsbookId)
        {
            var propUuids = scrapedProps.Select(sp => sp.PropUuid).ToList();
            var existingProps = await _dbContext.Props
                .Where(p => propUuids.Contains(p.PropUuid))
                .ToDictionaryAsync(p => p.PropUuid);

            // Upsert Props
            foreach (var sp in scrapedProps)
            {
                if (!existingProps.TryGetValue(sp.PropUuid, out var prop))
                {
                    prop = new Prop
                    {
                        PropUuid = sp.PropUuid,
                        GameId = sp.GameId,
                        PropName = sp.PropName,
                        PropType = sp.PropType
                    };
                    _dbContext.Props.Add(prop);
                    existingProps[sp.PropUuid] = prop;
                }
                else
                {
                    prop.PropName = sp.PropName;
                    prop.PropType = sp.PropType;
                    _dbContext.Props.Update(prop);
                }
            }

            // Lines
            var lineUuids = scrapedLines.Select(sl => sl.LineUuid).ToList();
            var existingLines = await _dbContext.Lines
                .Where(l => lineUuids.Contains(l.LineUuid))
                .ToDictionaryAsync(l => l.LineUuid);

            foreach (var sl in scrapedLines)
            {
                if (!existingLines.TryGetValue(sl.LineUuid, out var line))
                {
                    if (!existingProps.ContainsKey(sl.PropUuid))
                    {
                        Console.WriteLine(
                            $"[WARNING] Missing PropUuid {sl.PropUuid} for line {sl.LineUuid} ({sl.Description})"
                        );
                        continue;
                    }

                    line = new Line
                    {
                        LineUuid = sl.LineUuid,
                        Description = sl.Description,
                        Odd = sl.Odd,
                        SportsbookId = sportsbookId,
                        PropId = existingProps[sl.PropUuid].PropId
                    };
                    _dbContext.Lines.Add(line);
                }
                else
                {
                    line.Description = sl.Description;
                    line.Odd = sl.Odd;
                    _dbContext.Lines.Update(line);
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
