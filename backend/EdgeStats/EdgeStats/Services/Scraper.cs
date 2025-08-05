using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading.Tasks;

namespace EdgeStats.Services
{
    public class Scraper
    {
        public async void Scrape()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless"); // Optional: Run headless for servers
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");

            using var driver = new ChromeDriver(options);
        }
    }
}
