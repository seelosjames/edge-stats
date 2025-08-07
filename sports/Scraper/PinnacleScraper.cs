using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Scraper.Utils;
using System.Globalization;
using System.Text.RegularExpressions;
using static Scraper.PinnacleScraper;

namespace Scraper
{
	internal class PinnacleScraper
	{
		public class Game
		{
			public string League { get; set; }
			public string Sportsbook { get; set; }
			public string Team1 { get; set; }
			public string Team2 { get; set; }
			public DateTime GameTime { get; set; }
			public string GameUrl { get; set; }
			public string GameUuid { get; set; }
		}

		public class Prop
		{
			public string PropName { get; set; }
			public string PropType { get; set; }
			public string PropUuid { get; set; }
			public int GameId { get; set; }
		}

		public class Line
		{
			public string LineUuid { get; set; }
			public string PropId { get; set; }
			public string PropUuid { get; set; }
			public string Description { get; set; }
			public Double Odd { get; set; }
			public string Sportsbook {  set; get; }

		}

		public List<Game> GetPinnacleGames(string url, string sportsbook, string league)
		{
			var gamesFound = new List<Game>();

			var chromeOptions = new ChromeOptions();
			chromeOptions.AddArgument("--no-sandbox");
			chromeOptions.AddArgument("--disable-dev-shm-usage");
			chromeOptions.AddArgument("--disable-gpu");
			//chromeOptions.AddArgument("--headless=new");
			//chromeOptions.AddArgument("--window-size=1920,1080");

			using var driver = new ChromeDriver(chromeOptions);
			var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

			try
			{
				driver.Navigate().GoToUrl(url);

				var gameElements = wait.Until(d =>
				{
					var elements = d.FindElements(By.XPath("//*[@id='root']/div[1]/div[2]/main/div/div[4]/div[2]/div/div"));
					return elements.Count > 0 ? elements : null;
				});

				var gameUrls = new List<string>();

				Console.WriteLine();
				foreach (var game in gameElements)
				{
					if (game.GetAttribute("class") == "row-u9F3b9WCM3 row-k9ktBvvTsJ")
					{
						var link = game.FindElement(By.TagName("a"));
						gameUrls.Add(link.GetAttribute("href"));
					}
				}

				foreach (var gameUrl in gameUrls)
				{
					driver.Navigate().GoToUrl(gameUrl);

					var team1 = Normalizer.NormalizeFromMapping((wait.Until(d =>
						d.FindElement(By.XPath("//*[@id='root']/div[1]/div[2]/main/div[1]/div[2]/div[3]/div[2]/div/label"))
					).Text), Mapping.TeamMapping);

					var team2 = Normalizer.NormalizeFromMapping((wait.Until(d =>
						d.FindElement(By.XPath("//*[@id='root']/div[1]/div[2]/main/div[1]/div[2]/div[4]/div[2]/div/label"))
					).Text), Mapping.TeamMapping);

					String dateTimeStr = wait.Until(d =>
						d.FindElement(By.XPath("//*[@id='root']/div[1]/div[2]/main/div[1]/div[2]/div[2]/div/span"))
					).Text;

					DateTime dateTimeObj = DateTime.ParseExact(
						dateTimeStr,
						"dddd, MMMM d, yyyy 'at' HH:mm",
						CultureInfo.InvariantCulture
					);

					var gameUuid = IdGenerator.GenerateGameUuid(team1, team2, dateTimeObj);

					gamesFound.Add(new Game
					{
						League = league,
						Sportsbook = sportsbook,
						Team1 = team1,
						Team2 = team2,
						GameTime = dateTimeObj,
						GameUrl = gameUrl,
						GameUuid = gameUuid
					});
                }
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error while processing link {url}: {ex.Message}");
			}
			finally
			{
				driver.Quit();
			}

			return gamesFound;
		}

		public void GetPinnacleOdds(List<Game> gamesFound)
		{
			var chromeOptions = new ChromeOptions();
			chromeOptions.AddArgument("--no-sandbox");
			chromeOptions.AddArgument("--disable-dev-shm-usage");
			chromeOptions.AddArgument("--disable-gpu");
			//chromeOptions.AddArgument("--headless=new");
			//chromeOptions.AddArgument("--window-size=1920,1080");

			using var driver = new ChromeDriver(chromeOptions);
			var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

			foreach (var game in gamesFound)
			{
				Console.WriteLine("HELLO");
				GetPinnacleProps(driver, wait, game);
			}
		}

		private void GetPinnacleProps(ChromeDriver driver, WebDriverWait wait, Game game)
		{
			try
			{
				driver.Navigate().GoToUrl(game.GameUrl);

				var showAllButton = wait.Until(driver =>
				{
					var element = driver.FindElement(By.XPath("//*[@id='root']/div[1]/div[2]/main/div[3]/div[1]/div[1]/div[1]/div/button"));
					return element.Displayed ? element : null;
				});

				if (showAllButton.Text.Trim() != "Hide All")
				{
					showAllButton.Click();
				}

				var propCategoryDivs = wait.Until(driver =>
				{
					var elements = driver.FindElements(By.XPath("//*[@id='root']/div[1]/div[2]/main/div[3]/div"));
					return elements.Count > 0 ? elements : null;
				});

				foreach (var propCategory in propCategoryDivs)
				{
					var propDivs = propCategory.FindElements(By.XPath("div"));

					foreach (var propDiv in propDivs)
					{
						string rawTitleText;
						try
						{
							rawTitleText = propDiv.FindElement(By.XPath("div[1]/span[1]")).Text;
						}
						catch (NoSuchElementException)
						{
							continue;
						}

						var title = StringParser.PinnacleParsePropString(rawTitleText); // Your C# equivalent of `pinnacle_parse_prop_string`

						if (title.ContainsKey("message") && title["message"] == "Prop not yet supported")
							continue;

						string propName = Normalizer.NormalizeFromMapping(title["prop_name"], Mapping.PropNameMapping);
						string propType = title["prop_type"];
						string propUuid = IdGenerator.GeneratePropUuid(propName, propType, 0); // Get Game Id somehow

						Prop prop = new Prop
						{
							PropName = propName,
							PropType = propType,
							PropUuid = propUuid,
							GameId = 0,
						};


						try
						{
							var button = propDiv.FindElement(By.XPath("div[2]/button"));
							if (button.Text == "Show more")
							{
                                button.Click();
                            }
                        }
                        catch (NoSuchElementException)
						{
							// Continue if there's no button to click
						}

						string? team1 = null;
						string? team2 = null;

						try
						{
							team1 = propDiv.FindElement(By.XPath("div[2]/ul/li[1]")).Text;
							team2 = propDiv.FindElement(By.XPath("div[2]/ul/li[2]")).Text;
						}
						catch (NoSuchElementException)
						{
							// Leave team1 and team2 as null if not found
						}

						var lineDivs = propDiv.FindElements(By.ClassName("button-wrapper-Z7pE7Fol_T"));

						Console.WriteLine($"Found {prop.PropType} prop: {prop.PropName} at {game.Sportsbook}");

						GetPinnacleLines(driver, wait, lineDivs, prop, game, team1, team2);
					}
				}
			}
            catch (Exception ex)
            {
                Console.WriteLine($"Error scraping game: {ex.Message}");
            }
        }

		private void GetPinnacleLines(ChromeDriver driver, WebDriverWait wait, IList<IWebElement> lineDivs, Prop prop, Game game, string? team1, string? team2)
		{
			for (int i = 0; i < lineDivs.Count; i++)
			{
				var lineDiv = lineDivs[i];
				var lineInfo = lineDiv.FindElements(By.XPath("button/span"));

				if (lineInfo.Count == 0)
					return;

				string description;
				if (prop.PropType == "Player Prop")
				{
					var words = lineInfo[0].Text.Split(' ');
					description = string.Join(" ", words.Take(2));
				}
				else
				{
					description = lineInfo[0].Text;
				}

				if (!string.IsNullOrEmpty(team1) || !string.IsNullOrEmpty(team2))
				{
					if (i % 2 == 0 && !string.IsNullOrEmpty(team1))
					{
						description = $"{team1} {description}";
					}
					else if (!string.IsNullOrEmpty(team2))
					{
						description = $"{team2} {description}";
					}
				}

				double odd = OddsConverter.DecimalToPercentage(double.Parse(lineInfo[1].Text));
				string lineUuid = IdGenerator.GenerateLineUuid(prop.PropUuid, description, game.Sportsbook);

				var line = new Line
				{
					LineUuid = lineUuid,
					Odd = odd,
					Description = StringParser.FormatTrailingNumber(description),
					Sportsbook = game.Sportsbook
				};

				Console.WriteLine($"{line.Description} at {line.Odd}");

			}

			Console.WriteLine();
		}



	}
}
