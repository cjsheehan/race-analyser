using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Racing.Classes;

namespace Racing_Domain_Tests
{
    [TestClass]
    public class HtmlServiceTests
    {
        [TestMethod]
        public void TestGetCanonicalUrl()
        {
            // Arrange
            string html = @"<!doctype html><html lang=""en-us"">
                            <head>
                                <link 
                                    href=""https://www.sportinglife.com/"">
                                <link rel=""canonical"" 
                                    href=""https://www.sportinglife.com/racing/racecards/2016-10-14/haydock-park/racecard/206462/david-smith-famous-christmas-parties-handicap"" data-react-helmet=""true"">
                            </head>"; AngleSharp.Parser.Html.HtmlParser _htmlParser = new AngleSharp.Parser.Html.HtmlParser(); AngleSharp.Dom.Html.IHtmlDocument doc = _htmlParser.Parse(html); // Act String actual = RacingWebScraper.HtmlService.GetCanonicalUrl(doc); // Assert
            String expected = "https://www.sportinglife.com/racing/racecards/2016-10-14/haydock-park/racecard/206462/david-smith-famous-christmas-parties-handicap";
        }

        [TestMethod]
        public async Task TestUserAgent()
        {
            var httpClient = new HttpClient();
            var htmlParser = new AngleSharp.Parser.Html.HtmlParser();
            // httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Android 10; Mobile; rv:68.0) (KHTML, like Gecko)");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.97 Mobile Safari/537.36");
            var page = await httpClient.GetStringAsync(new Uri("https://www.whatismybrowser.com/detect/what-is-my-user-agent"));
            var document = await htmlParser.ParseAsync(page).ConfigureAwait(false);
            var actual = document.QuerySelector("div#detected_value > a").TextContent;
            Assert.AreEqual("xyz", actual);
        }

        [TestMethod]
        public async Task TestGetMeetingSummaries()
        {
            var httpClient = new HttpClient();
            var htmlParser = new AngleSharp.Parser.Html.HtmlParser();
            var selector = "script#__NEXT_DATA__";
            var page = await httpClient.GetStringAsync(new Uri("https://www.sportinglife.com/racing/racecards/tomorrow"));
            var document = await htmlParser.ParseAsync(page).ConfigureAwait(false);
            var json = document.QuerySelector(selector).TextContent;
            List<Meeting> meetings = JsonConvert.DeserializeObject<RootObject>(json).Props.pageProps.meetings;
            Assert.IsTrue(meetings.Count > 0);
        }
    }

}

