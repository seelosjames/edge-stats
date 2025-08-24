using EdgeStats.Dtos;
using EdgeStats.Interfaces;
using EdgeStats.Models;
using EdgeStats.Factories;
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
        private readonly IScraperRepository _scraperRepository;
        private readonly Sportsbook _sportsbook;

        public PinnacleScraper(Sportsbook sportsbook, IScraperRepository scraperRepository)
        {
            _sportsbook = sportsbook ?? throw new ArgumentNullException(nameof(sportsbook));
            _scraperRepository = scraperRepository ?? throw new ArgumentNullException(nameof(scraperRepository));
        }

        public async Task Scrape(List<string> leagues)
        {
            var driver = WebDriverFactory.CreateDriver(headless: false);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            try
            {
                foreach (var leagueCode in leagues)
                {
                    var league = await _scraperRepository.GetLeagueByCodeAsync(leagueCode);
                    var sportsbookUrl = await _scraperRepository.GetSportsbookUrlAsync(league.LeagueId, _sportsbook.SportsbookId);

                    var scrapedGames = GetPinnacleGames(driver, wait, sportsbookUrl.Url, _sportsbook, league);

                    await _scraperRepository.SaveScrapedGamesAsync(scrapedGames, league);

                    var gameUuids = scrapedGames.Select(g => g.GameUuid).ToList();
                    var gamesFromDb = await _scraperRepository.GetGamesByUuidsAsync(gameUuids);

                    foreach (var game in gamesFromDb)
                    {
                        var (scrapedProps, scrapedLines) = GetPinnacleProps(driver, wait, game);

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

        public List<ScrapedGameDto> GetPinnacleGames(ChromeDriver driver, WebDriverWait wait, string url, Sportsbook sportsbook, League league)
        {
            var scrapedGames = new List<ScrapedGameDto>();

            try
            {
                driver.Navigate().GoToUrl(url);
                var gameElements = wait.Until(d =>
                {
                    var elements = d.FindElements(By.XPath("//*[@id='root']/div[1]/div[2]/main/div/div[4]/div[2]/div/div"));
                    return elements.Count > 0 ? elements : null;
                });

                var gameUrls = new List<string>();

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

                    string dateTimeStr = wait.Until(d =>
                        d.FindElement(By.XPath("//*[@id='root']/div[1]/div[2]/main/div[1]/div[2]/div[2]/div/span"))
                    ).Text;

                    DateTime dateTimeObj = DateTime.ParseExact(
                        dateTimeStr,
                        "dddd, MMMM d, yyyy 'at' HH:mm",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal
                    );

                    dateTimeObj = DateTime.SpecifyKind(dateTimeObj.ToUniversalTime(), DateTimeKind.Utc);

                    var gameUuid = IdHelper.GenerateGameUuid(team1, team2, dateTimeObj);

                    scrapedGames.Add(new ScrapedGameDto
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
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while processing link {url}: {ex.Message}");
            }

            return scrapedGames;
        }

        private (List<ScrapedPropDto> props, List<ScrapedLineDto> lines) GetPinnacleProps(
            ChromeDriver driver,
            WebDriverWait wait,
            Game game)
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

                // expand "Show All" section if available
                var showAllButton = wait.Until(d =>
                {
                    var element = d.FindElement(By.XPath(
                        "//*[@id='root']/div[1]/div[2]/main/div[3]/div[1]/div[1]/div[1]/div/button"
                    ));
                    return element.Displayed ? element : null;
                });

                if (showAllButton.Text.Trim() != "Hide All")
                    showAllButton.Click();

                // all prop category containers
                var propCategoryDivs = wait.Until(d =>
                {
                    var elements = d.FindElements(By.XPath("//*[@id='root']/div[1]/div[2]/main/div[3]/div"));
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

                        var titleParts = StringParser.PinnacleParsePropString(rawTitleText);

                        if (titleParts.ContainsKey("message") && titleParts["message"] == "Prop not yet supported")
                            continue;

                        string propName = Normalizer.NormalizeFromMapping(
                            titleParts["prop_name"],
                            Mapping.PropNameMapping
                        );
                        string propType = titleParts["prop_type"];

                        Guid propUuid = IdHelper.GeneratePropUuid(propName, propType, game.GameId);

                        var propDto = new ScrapedPropDto
                        {
                            PropName = propName,
                            PropType = propType,
                            PropUuid = propUuid,
                            GameId = game.GameId
                        };

                        // expand prop "Show more" if exists
                        try
                        {
                            var button = propDiv.FindElement(By.XPath("div[2]/button"));
                            if (button.Text == "Show more") button.Click();
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

                        var lines = GetPinnacleLines(driver, wait, lineDivs, propDto, game, team1, team2);

                        scrapedProps.Add(propDto);
                        scrapedLines.AddRange(lines);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scraping game {game.GameId}: {ex.Message}");
            }

            return (scrapedProps, scrapedLines);
        }

        private List<ScrapedLineDto> GetPinnacleLines(
            ChromeDriver driver,
            WebDriverWait wait,
            IList<IWebElement> lineDivs,
            ScrapedPropDto prop,
            Game game,
            string? team1,
            string? team2)
        {
            var lines = new List<ScrapedLineDto>();

            for (int i = 0; i < lineDivs.Count; i++)
            {
                var lineDiv = lineDivs[i];
                var lineInfo = lineDiv.FindElements(By.XPath("button/span"));
                if (lineInfo.Count == 0) continue;

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

                // attach team labels if relevant
                if (!string.IsNullOrEmpty(team1) || !string.IsNullOrEmpty(team2))
                {
                    if (i % 2 == 0 && !string.IsNullOrEmpty(team1))
                        description = $"{team1} {description}";
                    else if (!string.IsNullOrEmpty(team2))
                        description = $"{team2} {description}";
                }

                double odd = OddsConverter.DecimalToPercentage(double.Parse(lineInfo[1].Text));

                Guid lineUuid = IdHelper.GenerateLineUuid(prop.PropUuid, description, _sportsbook.SportsbookName);

                var lineDto = new ScrapedLineDto
                {
                    LineUuid = lineUuid,
                    PropUuid = prop.PropUuid,
                    Odd = odd,
                    Description = StringParser.PinnacleParseLineDesc(description),
                    Sportsbook = _sportsbook.SportsbookName
                };

                lines.Add(lineDto);
            }

            return lines;
        }
    }
}
