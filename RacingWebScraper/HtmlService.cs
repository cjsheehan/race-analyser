using AngleSharp;
using AngleSharp.Parser.Html;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Xml;

namespace RacingWebScraper
{

    public static class HtmlService
    {
			private static readonly log4net.ILog log =
log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();
        private static readonly AngleSharp.Parser.Html.HtmlParser _htmlParser = new AngleSharp.Parser.Html.HtmlParser();
        private static readonly IWebDriver driver = null;
       

        static HtmlService()
        {
            // EdgeOptions edgeOptions = new EdgeOptions();
            //edgeOptions.= true;
            //edgeOptions.BinaryLocation = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
            //edgeOptions.AddArgument("headless");
            //edgeOptions.AddArgument("disable-gpu");
            // var msedgedriverDir = @"E:\webdriver";
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments(new List<string>() {
                "--silent-launch",
                "--no-startup-window",
                "no-sandbox",
                "headless"
            });
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;    // This is to hidden the console.
            driver = new ChromeDriver(chromeDriverService, chromeOptions);
        }
        public static async Task<AngleSharp.Dom.IDocument> GetDocumentAsync(string uri)
        {
            if (String.IsNullOrWhiteSpace(uri)) throw new ArgumentNullException("uri is empty or null");
			    String page = null;

            try
            {
                driver.Navigate().GoToUrl(uri);
                page = driver.PageSource;
                //var parser = new HtmlParser();
                //var document = parser.Parse(spage);
                // page = await _httpClient.GetStringAsync(new Uri(uri)).ConfigureAwait(false);
            }
            catch (System.Exception e)
            {
			    log.Error(String.Format("Failed to get page : {0}, {1}", uri, e.Message));
            }
            return  await _htmlParser.ParseAsync(page).ConfigureAwait(false);
        }
        public static async Task<AngleSharp.Dom.IDocument> GetSeleniumDocumentAsync(string uri)
        {
            if (String.IsNullOrWhiteSpace(uri)) throw new ArgumentNullException("uri is empty or null");
            String page = null;

            try
            {
                page = await _httpClient.GetStringAsync(new Uri(uri)).ConfigureAwait(false);
            }
            catch (System.Exception e)
            {
                log.Error(String.Format("Failed to get page : {0}, {1}", uri, e.Message));
            }
            return await _htmlParser.ParseAsync(page).ConfigureAwait(false);
        }
        public static String GetCanonicalUrl(AngleSharp.Dom.IDocument document)
        {
            if (document == null) throw new ArgumentNullException("document is null");
            var url = document.QuerySelector("link[rel='canonical']").GetAttribute("href");
            if (url == null)
            {
                return "";
            }
            else
            {
                return url;
            }
        }
    }
}
