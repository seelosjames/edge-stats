
using EdgeStats.Dtos;
using EdgeStats.Factories;
using EdgeStats.Interfaces;
using EdgeStats.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Scraper.Utils;
using System.Globalization;

namespace EdgeStats.Scrapers
{
	public class FliffScraper : ISportsbookScraper
	{
		private readonly IScraperRepository _scraperRepository;
		private readonly Sportsbook _sportsbook;

		public FliffScraper(Sportsbook sportsbook, IScraperRepository scraperRepository)
		{
			_sportsbook = sportsbook ?? throw new ArgumentNullException(nameof(sportsbook));
			_scraperRepository = scraperRepository ?? throw new ArgumentNullException(nameof(scraperRepository));
		}

		public async Task Scrape(List<string> leagues)
		{
			using var driver = WebDriverFactory.CreateDriver(headless: false, configureOptions: opts =>
			{
				opts.EnableMobileEmulation("iPhone 12 Pro");
			});
			var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

			try
			{
				foreach (var leagueCode in leagues)
				{
					var league = await _scraperRepository.GetLeagueByCodeAsync(leagueCode);
					var sportsbookUrl = await _scraperRepository.GetSportsbookUrlAsync(league.LeagueId, _sportsbook.SportsbookId);

					var scrapedGames = GetGames(driver, wait, sportsbookUrl.Url, _sportsbook, league);
					await _scraperRepository.SaveScrapedGamesAsync(scrapedGames, league);

					var gameUuids = scrapedGames.Select(g => g.GameUuid).ToList();
					var gamesFromDb = await _scraperRepository.GetGamesByUuidsAsync(gameUuids);

					foreach (var game in gamesFromDb)
					{
						var (scrapedProps, scrapedLines) = GetProps(driver, wait, game, league);

						await _scraperRepository.UpsertPropsAndLinesAsync(
							scrapedProps,
							scrapedLines,
							_sportsbook.SportsbookId
						);
					}
				}
			}
			finally
			{
				driver.Quit();
			}
		}

		private List<ScrapedGameDto> GetGames(ChromeDriver driver, WebDriverWait wait, string url, Sportsbook sportsbook, League league)
		{
			var scrapedGames = new List<ScrapedGameDto>();

			try
			{
				driver.Navigate().GoToUrl(url);

				var games = wait.Until(d => d.FindElements(By.ClassName("card-shared-container")));

				var notLive = games
					.Select((game, index) => new { game, index })
					.Where(g => g.game.FindElements(By.ClassName("live-bar-state")).Count == 0)
					.ToList();

				foreach (var entry in notLive)
				{
					var game = entry.game;
					var index = entry.index;

					string date = game.FindElement(By.XPath("div[2]/div[1]/div[1]/div/span[1]")).Text;
					string time = game.FindElement(By.XPath("div[2]/div[1]/div[1]/div/span[2]")).Text;

					string dateTimeStr = $"{date} {time}";

					DateTime dateTimeObj = DateTime.ParseExact(
						dateTimeStr,
						"ddd, MMM d yyyy h:mm tt",
						CultureInfo.InvariantCulture,
						DateTimeStyles.AssumeLocal
					);
					dateTimeObj = DateTime.SpecifyKind(dateTimeObj.ToUniversalTime(), DateTimeKind.Utc);

					string team1 = game.FindElement(By.XPath("div[2]/div[2]/div[1]/span")).Text;
					string team2 = game.FindElement(By.XPath("div[2]/div[3]/div[1]/span")).Text;

					// Open the game details page
					driver.Navigate().GoToUrl(url);
					var gamesAgain = wait.Until(d => d.FindElements(By.ClassName("card-shared-container")));

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

					var gameUuid = IdHelper.GenerateGameUuid(team1, team2, dateTimeObj, league.LeagueId);

					var scrapedGame = new ScrapedGameDto
					{
						GameUuid = gameUuid,
						LeagueId = league.LeagueId,
						Team1 = team1,
						Team2 = team2,
						GameTime = dateTimeObj,
						GameUrl = new ScrapedGameUrlDto
						{
							GameUrl = gameUrl,
							SportsbookId = sportsbook.SportsbookId,
						},
					};

					scrapedGames.Add(scrapedGame);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error while fetching matchups: {e.Message}");
			}

			return scrapedGames;
		}

		private (List<ScrapedPropDto>, List<ScrapedLineDto>) GetProps(
	ChromeDriver driver,
	WebDriverWait wait,
	Game game,
	League league
)
		{
			var scrapedProps = new List<ScrapedPropDto>();
			var scrapedLines = new List<ScrapedLineDto>();

			try
			{
				var url = game.GameUrls
					.FirstOrDefault(gurl => gurl.SportsbookId == _sportsbook.SportsbookId)?.GameUrlValue;

				if (url == null)
				{
					Console.WriteLine(
						$"No URL found for sportsbook {_sportsbook.SportsbookName} on game {game.GameId}"
					);
					return (scrapedProps, scrapedLines);
				}

				driver.Navigate().GoToUrl(url);

				var tabs = wait.Until(drv => drv.FindElements(By.ClassName("tab-filter")));

				for (int i = 1; i < tabs.Count; i++)
				{
					string tabTitle = tabs[i].Text;

					// Skip irrelevant tabs
					if (tabTitle == "SUMMARY" || tabTitle == "BOOSTED" || tabTitle == "GAME PROPS" || tabTitle == "TEAM PROPS")
						continue;

					tabs[i].Click();
					((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 0);");

					// Expand closed divs
					try
					{
						var closedDivs = wait.Until(drv => drv.FindElements(By.ClassName("toggled-opened")));
						foreach (var div in closedDivs)
						{
							new Actions(driver).MoveToElement(div).Perform();
							div.Click();
						}
					}
					catch (WebDriverTimeoutException)
					{
						Console.WriteLine("No closed divs found for this tab.");
					}

					var mainDiv = driver.FindElement(By.ClassName("more-markets"));
					var propDivs = mainDiv.FindElements(By.XPath("div"));

					string currentPropName = null;
					string currentPropType = null;
					Guid? currentPropUuid = null;

					foreach (var propDiv in propDivs)
					{
						string className = propDiv.GetAttribute("class");

						if (className == "more-markets-note")
							continue;

						if (className == "more-markets-title")
						{
							// Update "current prop" context
							(currentPropName, currentPropType) = HandlePropTitle(propDiv, tabTitle, game, league);
							if (!string.IsNullOrEmpty(currentPropName))
							{
								currentPropUuid = IdHelper.GeneratePropUuid(currentPropName, currentPropType, game.GameId);

								scrapedProps.Add(new ScrapedPropDto
								{
									PropUuid = currentPropUuid.Value,
									GameId = game.GameId,
									PropName = currentPropName,
									PropType = currentPropType
								});

								Console.WriteLine($"Found {currentPropType} prop: {currentPropName} at {_sportsbook.SportsbookName}");
							}
						}
						else
						{
							// Handle Prop body → create Lines
							if (tabTitle == "PLAYER PROPS" && currentPropUuid.HasValue)
							{
								// Player props sometimes have <p> element with player name
								string playerName = "";
								try
								{
									playerName = propDiv.FindElement(By.XPath("p")).Text + " ";
								}
								catch { /* not all divs have <p> */ }

								if (!string.IsNullOrEmpty(playerName) && currentPropName != null)
								{
									string combinedPropName = playerName + currentPropName;
									var playerPropUuid = IdHelper.GeneratePropUuid(combinedPropName, currentPropType, game.GameId);

									scrapedProps.Add(new ScrapedPropDto
									{
										PropUuid = playerPropUuid,
										GameId = game.GameId,
										PropName = combinedPropName,
										PropType = currentPropType
									});

									currentPropUuid = playerPropUuid;
									Console.WriteLine($"Found Player Prop: {combinedPropName} at {_sportsbook.SportsbookName}");
								}
							}

							// Now handle odds/lines
							var lineElements = propDiv.FindElements(By.XPath(".//div[contains(@class, 'market-outcome')]"));

							foreach (var lineElem in lineElements)
							{
								// Skip locked lines
								if (lineElem.FindElements(By.ClassName("icon-lock")).Any())
									continue;

								var spans = lineElem.FindElements(By.TagName("span"));
								if (spans.Count < 2) continue;

								string description = ConvertNumberToFloat(spans[0].Text).ToString();

								if (!double.TryParse(spans[1].Text.Replace("+", "").Trim(), out double odd))
									continue;

								if (!currentPropUuid.HasValue) continue;

								var lineUuid = IdHelper.GenerateLineUuid(currentPropUuid.Value, description, _sportsbook.SportsbookName);

								scrapedLines.Add(new ScrapedLineDto
								{
									LineUuid = lineUuid,
									PropUuid = currentPropUuid.Value,
									Description = description,
									Odd = odd   // now double
								});

								Console.WriteLine($"Found line: {description} at {odd}");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred in GetProps: {ex.Message}");
			}

			return (scrapedProps, scrapedLines);
		}


		private (string, string) HandlePropTitle(IWebElement propDiv, string tabTitle, Game game, League league)
		{
			string propName = string.Empty;
			string propType = string.Empty;

			if (tabTitle == "GAME LINES")
			{
				propName = NormalizePropName(propDiv.FindElement(By.XPath("span")).Text);
				propType = "Game";
			}
			else if (tabTitle == "PLAYER PROPS")
			{
				string text = propDiv.FindElement(By.XPath("span")).Text;
				if (league.LeagueCode.ToLower() == "nhl")
				{
					var parts = text.Split(new string[] { "PLAYER " }, StringSplitOptions.None);
					string prop = parts.Length > 1 ? NormalizeProp(parts[1]) : NormalizeProp(parts[0]);
					propType = "Player Prop";
					propName = prop;
				}
				else
				{
					string prop = NormalizeProp(text.Split(' ', 2)[1]);
					propType = "Player Prop";
					propName = prop;
				}
			}
			else if (tabTitle == "PERIODS" || tabTitle == "HALVES" || tabTitle == "QUARTERS")
			{
				string text = propDiv.FindElement(By.XPath("span")).Text;
				string[] parts = league.LeagueCode.ToLower() == "nhl"
					? text.Split(new string[] { "PERIOD " }, StringSplitOptions.None)
					: FliffParseString(text);

				propName = NormalizePropName(parts.Last());
				propType = NormalizePropType(parts.First());
			}

			return (propName, propType);
		}

		private void HandlePropBody(IWebElement propDiv, string tabTitle, string gameId, string sportsbook, string league)
		{
			string propName;
			string propType;

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
