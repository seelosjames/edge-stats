
using EdgeStats.Models;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Interactions; // For Actions
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Support.UI;  // For WebDriverWait
using System;
using System.Collections.Generic;
using System.Collections.Generic;   // For Dictionary
using System.Globalization;         // For DateTime.ParseExact
using System.Linq;                  // For LINQ queries if needed
using System.Threading;

namespace EdgeStats.Scrapers
{
    public class FliffScraper : ISportsbookScraper
    {
		public async Task Scrape(List<string> leagues)
        {
            var options = new ChromeOptions();
            options.EnableMobileEmulation("iPhone 12 Pro");
            IWebDriver driver = new ChromeDriver(options);
			var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

			driver.Quit();
		}

        public void GetFliffGames(ChromeDriver driver, WebDriverWait wait, string url, Sportsbook sportsbook, League league)
        {
            try
            {
                driver.Navigate().GoToUrl(url);

                var games = wait.Until(d =>
                    d.FindElements(By.ClassName("card-shared-container"))
                );

                var notLive = new List<Dictionary<string, object>>();

                for (int i = 0; i < games.Count; i++)
                {
                    var game = games[i];
                    bool live = game.FindElements(By.ClassName("live-bar-state")).Count > 0;

                    if (!live)
                    {
                        var newGame = new Dictionary<string, object>();
                        newGame["index"] = i;

                        string date = game.FindElement(By.XPath("div[2]/div[1]/div[1]/div/span[1]")).Text;
                        string time = game.FindElement(By.XPath("div[2]/div[1]/div[1]/div/span[2]")).Text;

                        DateTime gameTime = Helpers.FliffParseDatetime(date + " " + time);

                        string team1 = game.FindElement(By.XPath("div[2]/div[2]/div[1]/span")).Text;
                        string team2 = game.FindElement(By.XPath("div[2]/div[3]/div[1]/span")).Text;

                        Console.WriteLine($"Found {league} game: {team1} vs. {team2} on {gameTime}");

                        newGame["gameTime"] = gameTime;
                        newGame["team1"] = team1;
                        newGame["team2"] = team2;

                        notLive.Add(newGame);
                    }
                }

                // Click into each non-live game
                foreach (var game in notLive)
                {
                    int index = (int)game["index"];
                    driver.Navigate().GoToUrl(url);

                    var gamesAgain = wait.Until(d =>
                        d.FindElements(By.ClassName("card-shared-container"))
                    );

                    var cardHeader = gamesAgain[index].FindElement(By.ClassName("card-row-header"));
                    var cardFooter = gamesAgain[index].FindElement(By.ClassName("card-regular-footer"));

                    new Actions(driver).MoveToElement(cardFooter).Perform();

                    try
                    {
                        cardHeader.Click();
                    }
                    catch
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", cardHeader);
                    }

                    wait.Until(d => d.FindElements(By.XPath("//*[@id='root']/div[1]")));
                    string gameUrl = driver.Url;

                    Console.WriteLine($"Game URL: {gameUrl} ({game["team1"]} vs {game["team2"]})");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while fetching matchups: {e.Message}");
            }
            finally
            {
                driver.Quit();
            }
        }
    }
}
