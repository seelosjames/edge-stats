using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Scraper;
using System;

public class Program
{
    public static void Main()
    {
        var scraper = new PinnacleScraper();
        var games = scraper.GetPinnacleGames(
            url: "https://www.pinnacle.com/en/football/nfl/matchups/#all",
            sportsbook: "Pinnacle",
            league: "NFL"
        );


        // Use the games list however you want
        foreach (var game in games)
        {
            Console.WriteLine($"{game.GameUuid} - {game.Team1} vs {game.Team2} on {game.GameTime}");
        }
        scraper.GetPinnacleOdds(games);
    }
}
