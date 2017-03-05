using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using Racing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacingWebScraper
{
    public partial class SLifeRacingScraper
    {

        //public String Distance { get; set; }
        //public String Going { get; set; }
        //public String WinTime { get; set; }
        //public int Pos { get; set; }
        //public String Weight { get; set; }
        //public String BeatenLengths { get; set; }
        //public String SP { get; set; }
        //public String Trainer { get; set; }
        //public String Jockey { get; set; }
        //public String Url { get; set; }
        //public String Analysis { get; set; }
        //public String Course { get; set; }
        //public String Class { get; set; }
        //public int NumRunners { get; set; }
        //public PrizeList LastRacePrizes { get; set; }

        async Task<LastRace> ScrapeLastRace(String horseUrl)
        {
            // Get horse profile page
            var config = Configuration.Default.WithDefaultLoader();
            var profilePage = WebPage.GetAsync(horseUrl);
            var parser = new HtmlParser();
            var profileDoc = parser.Parse(profilePage.Result);
    
            //var profileDoc = BrowsingContext.New(config).OpenAsync(horseUrl).Result;
            // Get required data from profile
            var position = ScrapeLastPosition(profileDoc.DocumentElement);

            // Get page for last race
            const String lastRaceUrlSelector = "td:nth-child(1) > a.hr-racing-form-racecard-link";
            var lastRaceUrl = SITE_PREFIX + ScrapeUrl(profileDoc, lastRaceUrlSelector);
            var lastRacePage = WebPage.GetAsync(lastRaceUrl);
            var lastRaceDocument = parser.Parse(lastRacePage.Result);
            //var lastRaceDocument = await BrowsingContext.New(config).OpenAsync(lastRaceUrl);

            // Get dat from profile



            // Scrape data
            LastRace lastRace = new LastRace();
            lastRace.Distance = ScrapeLastDistance(lastRaceDocument);
            lastRace.Going = ScrapeLastGoing(lastRaceDocument);
            lastRace.BeatenLengths = ScrapeBeatenLengths(lastRaceDocument, position);
            lastRace.Weight = ScrapeLastWeight(lastRaceDocument, position);
            lastRace.Odds = ScrapeLastOdds(lastRaceDocument, position);
            lastRace.WinningTime = ScrapeLastWinningTime(lastRaceDocument);
            lastRace.Analysis = ScrapeLastAnalysis(lastRaceDocument);

            return lastRace;
        }

        private string ScrapeLastAnalysis(AngleSharp.Dom.Html.IHtmlDocument lastRaceDocument)
        {
            const String selector = "div.hr-racing-runner-ride-desc-info";
            return ScrapeTextContent(lastRaceDocument, selector);
        }

        private String ScrapeLastWinningTime(IDocument lastRaceDocument)
        {
            const String selector = "span.hr-racecard-weighed-in-wt > span";
            return ScrapeTextContent(lastRaceDocument, selector);
        }

        private String ScrapeLastOdds(AngleSharp.Dom.Html.IHtmlDocument lastRaceDocument, int position)
        {
            const String entrantSelector = "div.hr-racing-runner-position-container";
            var entrantElements = lastRaceDocument.QuerySelectorAll(entrantSelector);

            const String oddsSelector = "span.hr-racing-runner-betting-info";
            return ScrapeTextContent(entrantElements[position - 1], oddsSelector);
        }

        private string ScrapeLastWeight(AngleSharp.Dom.Html.IHtmlDocument lastRaceDocument, int position)
        {
            const String entrantSelector = "div.hr-racing-runner-position-container";
            var entrantElements = lastRaceDocument.QuerySelectorAll(entrantSelector);

            const String selector = "div.hr-racing-runner-horse-sub-info > span:nth-child(2)";
            return ScrapeTextContent(entrantElements[position - 1], selector);
        }

        private double ScrapeBeatenLengths(IDocument lastRaceDocument, int position)
        {
            // winner has no beaten lengths
            if (position == 1) return 0;

            const String entrantSelector = "div.hr-racing-runner-position-container";
            var entrantElements = lastRaceDocument.QuerySelectorAll(entrantSelector);

            //const String beatenLengthsSelector = "div.hr-racing-runner-space-from-winner";
            


            // Get the individual finishing distances between runners
            double sumDistance = 0.0;
            IEnumerable<double[]> entrantDistances = entrantElements.Skip(1).Select(e => 
            {
                const String select = "div.hr-racing-runner-space-from-winner";
                var textContent = ScrapeTextContent(e, select);

                double currentDistance = 0.0;
                try
                {
                    currentDistance = Distance.ConvertLengthsToDouble(textContent);
                    sumDistance += currentDistance;
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return new double[2] {currentDistance, sumDistance};
            });
            var entrantDistancesList = entrantDistances.ToList();
            entrantDistancesList.ForEach(i => Console.WriteLine("{0}", i));

            // min position value = 2
            var beatenLengths = entrantDistancesList[position - 2][1];


            return beatenLengths;
        }

        int ScrapeLastPosition(IElement element)
        {
            const String selector = "table.horse-results-table > tbody > tr:nth-child(1) > td:nth-child(2)";
            const String rx = "(.+)/.+";
            return ScrapeIntFromTextContent(element, selector, rx);
        }

        String ScrapeLastDistance(IDocument document)
        {
            const String selector = "li.hr-racecard-summary-race-distance";
            const String rx = "(.+),.+$";
            return ScrapeStringFromTextContent(document, selector, rx);
        }
        private string ScrapeLastGoing(IDocument document)
        {
            const String selector = "li.hr-racecard-summary-race-distance";
            const String rx = ".+,(.+)$";
            return ScrapeStringFromTextContent(document, selector, rx);
        }



    }
}
