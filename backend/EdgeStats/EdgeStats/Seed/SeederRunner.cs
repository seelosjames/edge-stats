// SeederRunner.cs
using EdgeStats;
using EdgeStats.models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class SeederRunner
{
    public static (List<League>, List<Team>, List<Sportsbook>, List<Game>, List<Prop>, List<Line>) GenerateAllData()
    {
        return DummySeeder.GenerateAllData(); // assumes you have a DummySeeder class
    }

    public static async Task SaveToDatabase(EdgeStatsDbContext context)
    {
        var (leagues, teams, books, games, props, lines) = GenerateAllData();

        context.Leagues.AddRange(leagues);
        context.Teams.AddRange(teams);
        context.Sportsbooks.AddRange(books);
        context.Games.AddRange(games);
        context.Props.AddRange(props);
        context.Lines.AddRange(lines);

        await context.SaveChangesAsync();
    }

    public static void ExportToJson<T>(List<T> data, string fileName)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        Directory.CreateDirectory("dummy_data_output");
        var json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(Path.Combine("dummy_data_output", $"{fileName}.json"), json);
    }

    public static void ExportAllToJson()
    {
        var (leagues, teams, books, games, props, lines) = GenerateAllData();

        ExportToJson(leagues, "leagues");
        ExportToJson(teams, "teams");
        ExportToJson(books, "sportsbooks");
        ExportToJson(games, "games");
        ExportToJson(props, "props");
        ExportToJson(lines, "lines");
    }
}
