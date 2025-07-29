using EdgeStats.models;
using System;
using System.Collections.Generic;

public static class DummySeeder
{
    private static Random rand = new();

    public static (List<League>, List<Team>, List<Sportsbook>, List<Game>, List<Prop>, List<Line>) GenerateAllData()
    {
        var leagues = new List<League>
        {
            new() { LeagueId = 1, LeagueName = "NFL", SportType = "Football" },
            new() { LeagueId = 2, LeagueName = "NBA", SportType = "Basketball" },
            new() { LeagueId = 3, LeagueName = "MLB", SportType = "Baseball" }
        };

        var teams = new List<Team>
        {
            new() { TeamId = 1, TeamName = "Miami Dolphins", LeagueId = 1 },
            new() { TeamId = 2, TeamName = "Kansas City Chiefs", LeagueId = 1 },
            new() { TeamId = 3, TeamName = "Indiana Pacers", LeagueId = 2 },
            new() { TeamId = 4, TeamName = "Boston Celtics", LeagueId = 2 },
            new() { TeamId = 5, TeamName = "Boston Red Sox", LeagueId = 3 },
            new() { TeamId = 6, TeamName = "New York Yankees", LeagueId = 3 }
        };

        var sportsbooks = new List<Sportsbook>
        {
            new() { SportsbookId = 1, SportsbookName = "Fliff" },
            new() { SportsbookId = 2, SportsbookName = "Pinnacle" },
            new() { SportsbookId = 3, SportsbookName = "DraftKings" }
        };

        var games = new List<Game>();
        var props = new List<Prop>();
        var lines = new List<Line>();
        int propIdCounter = 1, lineIdCounter = 1, gameIdCounter = 1;

        // Create 10 games per league
        foreach (var league in leagues)
        {
            var leagueTeams = teams.FindAll(t => t.LeagueId == league.LeagueId);
            for (int i = 0; i < 10; i++)
            {
                var team1 = leagueTeams[rand.Next(leagueTeams.Count)];
                var team2 = leagueTeams.Find(t => t.TeamId != team1.TeamId);
                var game = new Game
                {
                    GameId = gameIdCounter++,
                    GameUuid = Guid.NewGuid(),
                    LeagueId = league.LeagueId,
                    Team1Id = team1.TeamId,
                    Team2Id = team2.TeamId,
                    GameDateTime = DateTime.UtcNow.AddDays(rand.Next(1, 30)),
                    Status = Game.GameStatus.Scheduled
                };
                games.Add(game);

                // Generate 2 game props and 2 player props
                var gameProps = new[] { "Spread", "Total", "Moneyline" };
                foreach (var gp in gameProps)
                {
                    var prop = new Prop
                    {
                        PropId = propIdCounter++,
                        PropUuid = Guid.NewGuid(),
                        GameId = game.GameId,
                        PropName = gp,
                        PropType = "Game"
                    };
                    props.Add(prop);
                    lines.AddRange(CreateLinesForProp(prop, game, sportsbooks, ref lineIdCounter));
                }

                var playerProps = new[]
                {
                    new { Name = $"{team1.TeamName.Split()[1]} Star Points", Type = "Player Prop", Stat = "Over", Value = rand.Next(20, 35) + 0.5m },
                    new { Name = $"{team2.TeamName.Split()[1]} Star Assists", Type = "Player Prop", Stat = "Under", Value = rand.Next(5, 10) + 0.5m }
                };

                foreach (var pp in playerProps)
                {
                    var prop = new Prop
                    {
                        PropId = propIdCounter++,
                        PropUuid = Guid.NewGuid(),
                        GameId = game.GameId,
                        PropName = pp.Name,
                        PropType = pp.Type
                    };
                    props.Add(prop);
                    lines.AddRange(CreateLinesForProp(prop, game, sportsbooks, ref lineIdCounter, pp.Stat, pp.Value));
                }
            }
        }

        return (leagues, teams, sportsbooks, games, props, lines);
    }

    private static List<Line> CreateLinesForProp(Prop prop, Game game, List<Sportsbook> sportsbooks, ref int lineId, string? stat = null, decimal? baseValue = null)
    {
        var lines = new List<Line>();

        foreach (var book in sportsbooks)
        {
            string description;
            if (prop.PropType == "Game")
            {
                description = prop.PropName switch
                {
                    "Spread" => $"{GetShortTeamName(game.Team1Id)} +{RandomDecimal(1.5m, 7.5m)}",
                    "Total" => $"{(rand.Next(0, 2) == 0 ? "Over" : "Under")} {RandomDecimal(38.5m, 54.5m)}",
                    "Moneyline" => $"{GetShortTeamName(game.Team1Id)} ML",
                    _ => "Unknown"
                };
            }
            else // Player Prop
            {
                var direction = stat ?? (rand.Next(0, 2) == 0 ? "Over" : "Under");
                var value = baseValue ?? RandomDecimal(70, 300);
                description = $"{direction} {value}";
            }

            lines.Add(new Line
            {
                LineId = lineId++,
                LineUuid = Guid.NewGuid(),
                PropId = prop.PropId,
                SportsbookId = book.SportsbookId,
                Description = description,
                Odd = RandomOdds()
            });
        }

        return lines;
    }

    private static string GetShortTeamName(int teamId)
    {
        return teamId switch
        {
            1 => "Dolphins",
            2 => "Chiefs",
            3 => "Pacers",
            4 => "Celtics",
            5 => "Red Sox",
            6 => "Yankees",
            _ => "Team"
        };
    }

    private static decimal RandomOdds()
    {
        var odds = new[] { -120m, -110m, +100m, +115m, +130m, -105m, +145m };
        return odds[rand.Next(odds.Length)];
    }

    private static decimal RandomDecimal(decimal min, decimal max)
    {
        return Math.Round(min + (decimal)rand.NextDouble() * (max - min), 1);
    }
}
