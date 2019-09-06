using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading.Tasks;
using System.Net;

namespace FreakonomicsDownloader
{
    class Program
    {
        private const string UrlArchive = "http://freakonomics.com/archive/";
        private const int Timeout = 120;

        static async Task Main(string[] args)
        {
            #region Load App Settings
            Console.WriteLine("\nLoading AppSettings...");
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
            AppSettings appSettings = new AppSettings();
            new ConfigureFromConfigurationOptions<AppSettings>(
                configuration.GetSection("Settings")).Configure(appSettings);
            #endregion

            #region Load Selenium Driver
            Console.WriteLine($"\nLoading Selenium Driver for Chrome in headless mode...");
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--log-level=3");
            options.AddArgument("--silent");
            IWebDriver seleniumDriver = new ChromeDriver(appSettings.ChromeDriverPath, options);
            seleniumDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(Timeout);
            #endregion

            #region Load archive episode list
            Console.WriteLine($"\nLoading archive link list...");
            seleniumDriver.Navigate().GoToUrl(UrlArchive);

            var tableTitles = seleniumDriver
                .FindElement(By.ClassName("radioarchive"))
                .FindElements(By.ClassName("green-title"));

            List<string> episodeLinks = new List<string>();

            foreach(var rowTitle in tableTitles)
            {
                string link = rowTitle.FindElement(By.TagName("a")).GetAttribute("href");
                episodeLinks.Add(link);
                Console.WriteLine(link);
            }
            #endregion

            #region Download Episodes
            Console.WriteLine($"\nDownloading episodes...");
            foreach(string epPageLink in episodeLinks)
            {
                seleniumDriver.Navigate().GoToUrl(epPageLink);

                string episodeTitle = seleniumDriver.FindElement(By.ClassName("single_title")).Text;

                string fullLink = seleniumDriver.FindElement(By.ClassName("podcast_embed")).FindElement(By.TagName("iframe")).GetAttribute("src");
                int pFrom = fullLink.IndexOf("episodes/") + "episodes/".Length;
                int pTo = fullLink.LastIndexOf("/embed");
                string episodeId = fullLink.Substring(pFrom, pTo-pFrom);
                string downloadLink = $"https://rss.art19.com/episodes/{episodeId}.mp3";

                Console.WriteLine($"Episode title: '{episodeTitle}'");
                Console.WriteLine($"Download link: '{downloadLink}'");

                episodeTitle = episodeTitle.Replace("?", "").Replace("%", "")
                .Replace("!", "").Replace(":", "-").Replace("/", "").Replace("\\", "-")
                .Replace("”", "'").Replace("“", "'");

                string destination = Path.Combine(appSettings.OutputFolder, $"{episodeTitle}.mp3");

                if(!File.Exists(destination))
                {
                    await new WebClient().DownloadFileTaskAsync(new Uri(downloadLink), destination);
                }
            }
            #endregion

            #region Close Selenium Driver
            Console.WriteLine($"\nClosing Selenium Driver...");
            seleniumDriver.Quit();
            seleniumDriver = null;
            #endregion

            Console.WriteLine($"\nDone");
        }
    }
}