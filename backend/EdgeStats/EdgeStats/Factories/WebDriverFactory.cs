using OpenQA.Selenium.Chrome;

namespace EdgeStats.Factories
{
    public static class WebDriverFactory
    {
        public static ChromeDriver CreateDriver(
            bool headless = true,
            Action<ChromeOptions>? configureOptions = null)
        {
            var options = new ChromeOptions();

            // ✅ Base options
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");

            if (headless)
                options.AddArgument("--headless=new");

            // ✅ Extra overrides/extensions
            configureOptions?.Invoke(options);

            return new ChromeDriver(options);
        }
    }

}
