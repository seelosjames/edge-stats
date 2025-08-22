
using EdgeStats.Interfaces;
using EdgeStats.Models;
using EdgeStats.Services;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions; // For Actions
using OpenQA.Selenium.Interactions; // For Actions
using OpenQA.Selenium.Interactions;
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

		private readonly EdgeStatsDbContext _dbContext;
		private readonly Sportsbook _sportsbook;
		private readonly ScraperService _scraperService;

		public FliffScraper(EdgeStatsDbContext dbContext, String sportsbook, ScraperService scraperService)
		{
			_dbContext = dbContext;
			_sportsbook = _dbContext.Sportsbooks.First(s => s.SportsbookName == sportsbook);
			_scraperService = scraperService;
		}

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

                        DateTime gameTime = FliffParseDateTime(date + " " + time);

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

		public void GetFliffOdds(ChromeDriver driver, WebDriverWait wait, string url, string sportsbook, string league, string gameId)
		{
			try
			{
				driver.Navigate().GoToUrl(url);

				var tabs = wait.Until(drv => drv.FindElements(By.ClassName("tab-filter")));

				for (int i = 1; i < tabs.Count; i++)
				{
					string tabTitle = tabs[i].Text;

					// Skip irrelevant tabs
					if (tabTitle == "SUMMARY" || tabTitle == "BOOSTED" || tabTitle == "GAME PROPS" || tabTitle == "TEAM PROPS")
						continue;

					tabs[i].Click();
					Thread.Sleep(1000);
					((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 0);");

					try
					{
						var closedDivs = wait.Until(drv => drv.FindElements(By.ClassName("toggled-opened")));
						foreach (var div in closedDivs)
						{
							var actions = new Actions(driver);
							actions.MoveToElement(div).Perform();
							div.Click();
						}
					}
					catch (WebDriverTimeoutException)
					{
						Console.WriteLine("No closed divs found for this tab.");
					}

					var mainDiv = driver.FindElement(By.ClassName("more-markets"));
					var propDivs = mainDiv.FindElements(By.XPath("div"));

					foreach (var propDiv in propDivs)
					{
						string className = propDiv.GetAttribute("class");

						if (className == "more-markets-note")
							continue;

						if (className == "more-markets-title")
						{
							HandlePropTitle(propDiv, tabTitle, gameId, sportsbook, league);
						}
						else
						{
							HandlePropBody(propDiv, tabTitle, gameId, sportsbook, league);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
			}
		}

		private void HandlePropTitle(IWebElement propDiv, string tabTitle, string gameId, string sportsbook, string league)
		{
			string propName, propType;

			if (tabTitle == "GAME LINES")
			{
				propName = NormalizePropName(propDiv.FindElement(By.XPath("span")).Text);
				propType = "Game";
				// generate & save prop
			}
			else if (tabTitle == "PLAYER PROPS")
			{
				string text = propDiv.FindElement(By.XPath("span")).Text;
				if (league == "NHL")
				{
					var parts = text.Split(new string[] { "PLAYER " }, StringSplitOptions.None);
					string prop = parts.Length > 1 ? NormalizeProp(parts[1]) : NormalizeProp(parts[0]);
					propType = "Player Prop";
					propName = prop;
				}
				else // NBA
				{
					string prop = NormalizeProp(text.Split(' ', 2)[1]);
					propType = "Player Prop";
					propName = prop;
				}
			}
			else if (tabTitle == "PERIODS" || tabTitle == "HALVES" || tabTitle == "QUARTERS")
			{
				string text = propDiv.FindElement(By.XPath("span")).Text;
				string[] parts = league == "NHL"
					? text.Split(new string[] { "PERIOD " }, StringSplitOptions.None)
					: FliffParseString(text);

				propName = NormalizePropName(parts.Last());
				propType = NormalizePropType(parts.First());
			}
		}

		private void HandlePropBody(IWebElement propDiv, string tabTitle, string gameId, string sportsbook, string league)
		{
			string propName = null;
			string propType = null;

			if (tabTitle == "PLAYER PROPS")
			{
				string playerName = propDiv.FindElement(By.XPath("p")).Text + " ";
				string prop = "??"; // set from HandlePropTitle
				propName = playerName + prop;
				propType = "Player Prop";
			}

			var oddsDivs = propDiv.FindElements(By.XPath("div"));
			foreach (var oddsDiv in oddsDivs)
			{
				bool locked = oddsDiv.FindElements(By.ClassName("icon-lock")).Any();
				if (locked) continue;

				var lineInfo = oddsDiv.FindElements(By.TagName("span"));
				string description = ConvertNumberToFloat(lineInfo[0].Text).ToString();
				double odd = AmericanToPercentage(double.Parse(lineInfo[1].Text.Replace("+", "").Trim()));

				// generate line uuid, insert, etc.
			}
		}

		private string NormalizePropName(string text) => text.Trim();
		private string NormalizeProp(string text) => text.Trim();
		private string NormalizePropType(string text) => text.Trim();
		private string[] FliffParseString(string text) => text.Split(' ');
		private double ConvertNumberToFloat(string text) => double.TryParse(text, out var d) ? d : 0;
		private double AmericanToPercentage(double americanOdd)
		{
			if (americanOdd > 0)
				return 100.0 / (americanOdd + 100.0) * 100.0;
			else
				return (-americanOdd) / ((-americanOdd) + 100.0) * 100.0;
		}
		private DateTime FliffParseDateTime(string inputStr)
		{
			DateTime now = DateTime.Now;

			if (inputStr.Contains("at"))
			{
				if (inputStr.StartsWith("Today at "))
				{
					string timeStr = inputStr.Replace("Today at ", "");
					DateTime parsed = DateTime.ParseExact(timeStr, "h:mm tt", CultureInfo.InvariantCulture);
					return new DateTime(now.Year, now.Month, now.Day, parsed.Hour, parsed.Minute, 0);
				}
				else if (inputStr.StartsWith("Tomorrow at "))
				{
					string timeStr = inputStr.Replace("Tomorrow at ", "");
					DateTime parsed = DateTime.ParseExact(timeStr, "h:mm tt", CultureInfo.InvariantCulture);
					DateTime todayTime = new DateTime(now.Year, now.Month, now.Day, parsed.Hour, parsed.Minute, 0);
					return todayTime.AddDays(1);
				}
				else
				{
					// Example: "Jan 5, 2025 at 7:30 PM"
					return DateTime.ParseExact(inputStr, "MMM d, yyyy 'at' h:mm tt", CultureInfo.InvariantCulture);
				}
			}

			throw new ArgumentException($"Unsupported date string format: {inputStr}");
		}
	}
}
