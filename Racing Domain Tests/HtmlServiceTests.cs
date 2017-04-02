using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                            </head>";
            AngleSharp.Parser.Html.HtmlParser _htmlParser = new AngleSharp.Parser.Html.HtmlParser();
            AngleSharp.Dom.Html.IHtmlDocument doc = _htmlParser.Parse(html);

            // Act
            String actual = RacingWebScraper.HtmlService.GetCanonicalUrl(doc);

            // Assert
            String expected = "https://www.sportinglife.com/racing/racecards/2016-10-14/haydock-park/racecard/206462/david-smith-famous-christmas-parties-handicap";
            Assert.AreEqual(expected, actual);
        }
    }
}
