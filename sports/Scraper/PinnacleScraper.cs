using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scraper
{
    internal class PinnacleScraper
    {
        public class ScrapedGame
        {
            public string League { get; set; }
            public string Sportsbook { get; set; }
            public string Team1 { get; set; }
            public string Team2 { get; set; }
            public DateTime GameTime { get; set; }
            public string GameUrl { get; set; }
            public string GameUuid { get; set; }
        }

        public List<ScrapedGame> GetPinnacleGames(string url, string sportsbook, string league)
        {
            var gamesFound = new List<ScrapedGame>();

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

                //Thread.Sleep(10000);

                var gameElements = wait.Until(d =>
                {
                    var elements = d.FindElements(By.XPath("//*[@id='root']/div[1]/div[2]/main/div/div[4]/div[2]/div/div"));
                    return elements.Count > 0 ? elements : null;
                });

                var gameUrls = new List<string>();

                Console.WriteLine("Games Found: " + gameElements.Count);
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


                    var team1 = Normalizers.NormalizeFromMapping((wait.Until(d =>
                        d.FindElement(By.XPath("//*[@id='root']/div[1]/div[2]/main/div[1]/div[2]/div[3]/div[2]/div/label"))
                    ).Text), Mappings.TeamMapping);

                    var team2 = Normalizers.NormalizeFromMapping((wait.Until(d =>
                        d.FindElement(By.XPath("//*[@id='root']/div[1]/div[2]/main/div[1]/div[2]/div[4]/div[2]/div/label"))
                    ).Text), Mappings.TeamMapping);

                    String dateTimeStr = wait.Until(d =>
                        d.FindElement(By.XPath("//*[@id='root']/div[1]/div[2]/main/div[1]/div[2]/div[2]/div/span"))
                    ).Text;

                    DateTime dateTimeObj = DateTime.ParseExact(
                        dateTimeStr,
                        "dddd, MMMM d, yyyy 'at' HH:mm",
                        CultureInfo.InvariantCulture
                    );

                    
                    var gameUuid = GenerateGameUUID(team1, team2, dateTimeObj);

                    Console.WriteLine($"Found {league} game: {team1} vs. {team2} on {dateTimeObj} on {sportsbook}");

                    gamesFound.Add(new ScrapedGame
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

        DateTime NormalizeTime(DateTime actualTime, int roundToMinutes = 5)
        {
            int roundedMinutes = (actualTime.Minute / roundToMinutes) * roundToMinutes;
            return new DateTime(
                actualTime.Year,
                actualTime.Month,
                actualTime.Day,
                actualTime.Hour,
                roundedMinutes,
                0
            );
        }

        string GenerateGameUUID(string team1, string team2, DateTime actualTime)
        {
            DateTime normalizedTime = NormalizeTime(actualTime);
            string keyString = $"{team1}_{team2}_{normalizedTime:yyyyMMdd_HHmm}";
            return CreateUUIDFromString(keyString);
        }

        string CreateUUIDFromString(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                return new Guid(hash).ToString();
            }
        }
    }
}
