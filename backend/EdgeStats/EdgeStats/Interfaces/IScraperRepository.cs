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

    }
}
