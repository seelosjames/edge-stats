using EdgeStats.Dtos;
using EdgeStats.Models;
using EdgeStats.Utils.Mappings;
using EdgeStats.Utils.Parsing;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Scraper.Utils;
using System.Globalization;

namespace EdgeStats.Scrapers
{
	public class PinnacleScraper : ISportsbookScraper
	{
		private readonly EdgeStatsDbContext _dbContext;
        private readonly Sportsbook _sportsbook;


        public PinnacleScraper(EdgeStatsDbContext dbContext, String sportsbook)
		{
			_dbContext = dbContext;
            _sportsbook = _dbContext.Sportsbooks.First(s => s.SportsbookName == sportsbook);
		}

        public async Task Scrape(List<string> leagues)
        {
            foreach (var leagueCode in leagues)
            {
                // Load league and url for sportsbook
                League league = await _dbContext.Leagues.FirstAsync(l => l.LeagueCode == leagueCode);
                var sportsbookUrl = await _dbContext.SportsbookUrls
                    .FirstAsync(su => su.LeagueId == league.LeagueId && su.SportsbookId == _sportsbook.SportsbookId);


                // Setup Selenium driver once per league (optional: reuse if you want)
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("--no-sandbox");
                chromeOptions.AddArgument("--disable-dev-shm-usage");
                chromeOptions.AddArgument("--disable-gpu");
                //chromeOptions.AddArgument("--headless=new");

                using var driver = new ChromeDriver(chromeOptions);
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                
				// Scrape games and upsert teams + games
                var scrapedGames = GetPinnacleGames(driver, wait, sportsbookUrl.Url, _sportsbook, league);

                // 2. Fetch the saved Game entities to use for GetPinnacleProps
                var gameUuids = scrapedGames.Select(g => g.GameUuid).ToList();
                var gamesFromDb = _dbContext.Games
                    .Include(g => g.GameUrls)
                    .Include(g => g.Props).ThenInclude(p => p.Lines)
                    .Where(g => gameUuids.Contains(g.GameUuid))
                    .ToList();

                foreach (var game in gamesFromDb)
                {
                    var (scrapedProps, scrapedLines) = GetPinnacleProps(driver, wait, game);

                    // Bulk fetch existing props for this game by UUID
                    var propUuids = scrapedProps.Select(sp => sp.PropUuid).ToList();
                    var existingProps = _dbContext.Props
                        .Where(p => propUuids.Contains(p.PropUuid))
                        .ToDictionary(p => p.PropUuid);

                    var propsToAdd = new List<Prop>();

                    foreach (var sp in scrapedProps)
                    {
                        if (!existingProps.TryGetValue(sp.PropUuid, out var existingProp))
                        {
                            // New prop - add it
                            propsToAdd.Add(new Prop
                            {
                                PropUuid = sp.PropUuid,
                                GameId = sp.GameId,
                                PropName = sp.PropName,
                                PropType = sp.PropType,
                                Lines = new List<Line>()
                            });
                        }
                        else
                        {
                            // Optional update of existing prop fields if changed
                            bool isModified = false;
                            if (existingProp.PropName != sp.PropName)
                            {
                                existingProp.PropName = sp.PropName;
                                isModified = true;
                            }
                            if (existingProp.PropType != sp.PropType)
                            {
                                existingProp.PropType = sp.PropType;
                                isModified = true;
                            }
                            if (isModified)
                            {
                                _dbContext.Props.Update(existingProp);
                            }
                        }
                    }

                    // Add new props and save to get IDs for foreign keys
                    if (propsToAdd.Any())
                    {
                        _dbContext.Props.AddRange(propsToAdd);
                        _dbContext.SaveChanges();
                    }

                    // Refresh existingProps dictionary with any newly added props
                    existingProps = _dbContext.Props
                        .Where(p => propUuids.Contains(p.PropUuid))
                        .ToDictionary(p => p.PropUuid);

                    // Bulk fetch existing lines by UUID
                    var lineUuids = scrapedLines.Select(sl => sl.LineUuid).ToList();
                    var existingLines = _dbContext.Lines
                        .Where(l => lineUuids.Contains(l.LineUuid))
                        .ToDictionary(l => l.LineUuid);

                    var linesToAdd = new List<Line>();

                    foreach (var sl in scrapedLines)
                    {
                        if (!existingLines.TryGetValue(sl.LineUuid, out var existingLine))
                        {
                            // New line - add it
                            linesToAdd.Add(new Line
                            {
                                LineUuid = sl.LineUuid,
                                Description = sl.Description,
                                Odd = sl.Odd,
                                SportsbookId = _sportsbook.SportsbookId,
                                PropId = existingProps[sl.PropUuid].PropId
                            });
                        }
                        else
                        {
                            // Optional update of existing line fields if changed
                            bool isModified = false;
                            if (existingLine.Description != sl.Description)
                            {
                                existingLine.Description = sl.Description;
                                isModified = true;
                            }
                            if (existingLine.Odd != sl.Odd)
                            {
                                existingLine.Odd = sl.Odd;
                                isModified = true;
                            }
                            if (isModified)
                            {
                                _dbContext.Lines.Update(existingLine);
                            }
                        }
                    }

                    if (linesToAdd.Any())
                    {
                        _dbContext.Lines.AddRange(linesToAdd);
                    }

                    _dbContext.SaveChanges();
                }

                // Save all changes after processing this league
                await _dbContext.SaveChangesAsync();

                // Dispose driver here at end of league processing
                driver.Quit();
            }
        }

        public List<ScrapedGameDto> GetPinnacleGames(ChromeDriver driver, WebDriverWait wait, string url, Sportsbook sportsbook, League league)
		{
			var gamesFound = new List<ScrapedGameDto>();

			try
			{
				driver.Navigate().GoToUrl(url);

				var gameElements = wait.Until(d =>
				{
					var elements = d.FindElements(By.XPath("//*[@id='root']/div[1]/div[2]/main/div/div[4]/div[2]/div/div"));
					return elements.Count > 0 ? elements : null;
				});

				var gameUrls = new List<String>();

				foreach (var game in gameElements)
				{
					if (game.GetAttribute("class") == "row-u9F3b9WCM3 row-k9ktBvvTsJ")
					{
						var link = game.FindElement(By.TagName("a"));
						gameUrls.Add(link.GetAttribute("href"));
					}
				}

				foreach (String gameUrl in gameUrls)
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

					var gameUuid = IdHelper.GenerateGameUuid(team1, team2, dateTimeObj);

					gamesFound.Add(new ScrapedGameDto
					{
						LeagueId = league.LeagueId,
						Team1 = team1,
						Team2 = team2,
						GameTime = dateTimeObj,
						GameUrl = new ScrapedGameUrlDto
						{
							GameUrl = gameUrl,
							SportsbookId = sportsbook.SportsbookId,
						},
						GameUuid = gameUuid
					});
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error while processing link {url}: {ex.Message}");
			}

            // Load all teams for the league once, key by lower case name
            var existingTeams = _dbContext.Teams
                .Where(t => t.LeagueId == league.LeagueId)
                .ToDictionary(t => t.TeamName.ToLowerInvariant());

            // Load all existing games for the league once, key by GameUuid
            var existingGames = _dbContext.Games
                .Include(g => g.GameUrls)
                .Where(g => g.LeagueId == league.LeagueId)
                .ToDictionary(g => g.GameUuid);

            var newTeams = new List<Team>();
            var newGames = new List<Game>();

            foreach (var scrapedGame in gamesFound)
            {
				// Normalize team names to lower invariant for dictionary key
				string team1Key = scrapedGame.Team1;
				string team2Key = scrapedGame.Team2;

                if (!existingTeams.TryGetValue(team1Key, out var team1))
                {
                    team1 = new Team { TeamName = scrapedGame.Team1, LeagueId = league.LeagueId };
                    existingTeams[team1Key] = team1;
                    newTeams.Add(team1);
                }

                if (!existingTeams.TryGetValue(team2Key, out var team2))
                {
                    team2 = new Team { TeamName = scrapedGame.Team2, LeagueId = league.LeagueId };
                    existingTeams[team2Key] = team2;
                    newTeams.Add(team2);
                }

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
                    // Optional: update existing game info (date/time or URLs) if changed
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
            }

            // Insert new teams and games in bulk
            if (newTeams.Count > 0)
            {
                _dbContext.Teams.AddRange(newTeams);
                _dbContext.SaveChanges(); // Save here so new teams get their IDs before games reference them
            }

            if (newGames.Count > 0)
            {
                _dbContext.Games.AddRange(newGames);
                _dbContext.SaveChanges();
            }

            return gamesFound;
		}

        private (List<ScrapedPropDto> props, List<ScrapedLineDto> lines) GetPinnacleProps(ChromeDriver driver, WebDriverWait wait, Game game)
        {
            var scrapedProps = new List<ScrapedPropDto>();
            var scrapedLines = new List<ScrapedLineDto>();

            try
            {
                var url = game.GameUrls.FirstOrDefault(gurl => gurl.SportsbookId == _sportsbook.SportsbookId)?.GameUrlValue;
                if (url == null)
                {
                    Console.WriteLine($"No URL found for sportsbook {_sportsbook.SportsbookName} on game {game.GameId}");
                    return (scrapedProps, scrapedLines);
                }

                driver.Navigate().GoToUrl(url);

                var showAllButton = wait.Until(driver =>
                {
                    var element = driver.FindElement(By.XPath("//*[@id='root']/div[1]/div[2]/main/div[3]/div[1]/div[1]/div[1]/div/button"));
                    return element.Displayed ? element : null;
                });

                if (showAllButton.Text.Trim() != "Hide All")
                    showAllButton.Click();

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

                        var title = StringParser.PinnacleParsePropString(rawTitleText);

                        if (title.ContainsKey("message") && title["message"] == "Prop not yet supported")
                            continue;

                        string propName = Normalizer.NormalizeFromMapping(title["prop_name"], Mapping.PropNameMapping);
                        string propType = title["prop_type"];
                        Guid propUuid = IdHelper.GeneratePropUuid(propName, propType, game.GameId); // <-- pass real GameId

                        var prop = new ScrapedPropDto
                        {
                            PropName = propName,
                            PropType = propType,
                            PropUuid = propUuid,
                            GameId = game.GameId,
                        };

                        try
                        {
                            var button = propDiv.FindElement(By.XPath("div[2]/button"));
                            if (button.Text == "Show more")
                                button.Click();
                        }
                        catch (NoSuchElementException) { }

                        string? team1 = null;
                        string? team2 = null;
                        try
                        {
                            team1 = propDiv.FindElement(By.XPath("div[2]/ul/li[1]")).Text;
                            team2 = propDiv.FindElement(By.XPath("div[2]/ul/li[2]")).Text;
                        }
                        catch (NoSuchElementException) { }

                        var lineDivs = propDiv.FindElements(By.ClassName("button-wrapper-Z7pE7Fol_T"));

                        Console.WriteLine($"Found {prop.PropType} prop: {prop.PropName} at {_sportsbook.SportsbookName}");

                        var lines = GetPinnacleLines(driver, wait, lineDivs, prop, game, team1, team2);

                        scrapedProps.Add(prop);
                        scrapedLines.AddRange(lines);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scraping game: {ex.Message}");
            }

            return (scrapedProps, scrapedLines);
        }

        private List<ScrapedLineDto> GetPinnacleLines(ChromeDriver driver, WebDriverWait wait, IList<IWebElement> lineDivs, ScrapedPropDto prop, Game game, string? team1, string? team2)
        {
            var lines = new List<ScrapedLineDto>();

            for (int i = 0; i < lineDivs.Count; i++)
            {
                var lineDiv = lineDivs[i];
                var lineInfo = lineDiv.FindElements(By.XPath("button/span"));

                if (lineInfo.Count == 0)
                    return lines;

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
                        description = $"{team1} {description}";
                    else if (!string.IsNullOrEmpty(team2))
                        description = $"{team2} {description}";
                }

                double odd = OddsConverter.DecimalToPercentage(double.Parse(lineInfo[1].Text));
                Guid lineUuid = IdHelper.GenerateLineUuid(prop.PropUuid, description, _sportsbook.SportsbookName);

                var line = new ScrapedLineDto
                {
                    LineUuid = lineUuid,
                    Odd = odd,
                    Description = StringParser.PinnacleParseLineDesc(description),
                    Sportsbook = _sportsbook.SportsbookName
                };

                Console.WriteLine($"{line.Description} at {line.Odd}");

                lines.Add(line);
            }
            return lines;
        }
    }
}
