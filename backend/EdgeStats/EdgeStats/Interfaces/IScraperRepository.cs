using EdgeStats.Dtos;
using EdgeStats.Models;

namespace EdgeStats.Interfaces
{
    public interface IScraperRepository
    {
        // Teams
        List<Team> InsertTeams(List<string> names, League league, List<Team> newTeams);

        // Games
        List<Game> InsertGames(ScrapedGameDto scrapedGame, League league, Team team1, Team team2);
        Task RemoveOldGamesAsync();

        Task<League> GetLeagueByCodeAsync(string leagueCode);
        Task<SportsbookUrl> GetSportsbookUrlAsync(int leagueId, int sportsbookId);

        Task SaveScrapedGamesAsync(List<ScrapedGameDto> scrapedGames, League league);

        Task<List<Game>> GetGamesByUuidsAsync(List<Guid> gameUuids);

        Task UpsertPropsAndLinesAsync(IEnumerable<ScrapedPropDto> scrapedProps, IEnumerable<ScrapedLineDto> scrapedLines, int sportsbookId);
    }
}
