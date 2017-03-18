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
        async Task<LastRace> ScrapeLastRace(String horseUrl, String horseName)
        {
            // Get horse profile page
            var profileDocument = await WebPage.GetDocumentAsync(horseUrl).ConfigureAwait(false);
    
            // Get required data from profile
            var positionInField = ScrapeLastPositionInField(profileDocument.DocumentElement);
            var position = ScrapeLastPosition(profileDocument.DocumentElement);
            var lastClass = ScrapeLastClass(profileDocument);

            // Get page for last race
            const String lastRaceUrlSelector = "td:nth-child(1) > a.hr-racing-form-racecard-link";
            var lastRaceUrl = SITE_PREFIX + ScrapeUrl(profileDocument, lastRaceUrlSelector);
            var lastRaceDocument = await WebPage.GetDocumentAsync(lastRaceUrl).ConfigureAwait(false);

            // Scrape data
            LastRace lastRace = new LastRace();
            if (!IsWeighedIn(lastRaceDocument)) return lastRace;
            lastRace.Distance = ScrapeLastDistance(lastRaceDocument);
            lastRace.Going = ScrapeLastGoing(lastRaceDocument);
            lastRace.Course = ScrapeLastCourse(lastRaceDocument);
            log.Debug("Scraping beaten for " + horseName);

            if(position > 0)
            {
                lastRace.Weight = ScrapeLastWeight(lastRaceDocument, position);
                lastRace.Odds = ScrapeLastOdds(lastRaceDocument, position);
                lastRace.Analysis = ScrapeLastAnalysis(lastRaceDocument, position);
                try
                {
                    if (position == 1)
                    {
                        lastRace.BeatenLengths = 0;
                    }
                    else if (position > 1)
                    {
                        lastRace.BeatenLengths = ScrapeBeatenLengths(lastRaceDocument, position);
                    }
                }
                catch (System.Exception e)
                {
                    log.Error("Failed to scrape beaten lengths: " + e.Message);
                }
            }
            else
            {
                log.Info("Unknown position " + position);
            }


            lastRace.WinningTime = ScrapeLastWinningTime(lastRaceDocument);
            lastRace.Class = lastClass;
            lastRace.Position = positionInField;
            return lastRace;
        }

        private bool IsWeighedIn(IDocument lastRaceDocument)
        {
            const String selector = "div.hr-racecard-weighed-in-wrapper";
            var element = ScrapeTextContent(lastRaceDocument, selector);
            if(!String.IsNullOrEmpty(element))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private String ScrapeLastClass(IDocument lastRaceDocument)
        {
            const String selector = "table.horse-results-table > tbody > tr:nth-child(1) > td:nth-child(5)";
            const String rx = ".+,\\s+(.+)$";
            return ScrapeStringFromTextContent(lastRaceDocument, selector, rx);
        }

        private string ScrapeLastCourse(IDocument lastRaceDocument)
        {
            const String selector = "section.hr-racing-racecard-top-section > h1";
            const String rx = "\\d{2}:\\d{2}\\s+(.+)";
            return ScrapeStringFromTextContent(lastRaceDocument, selector, rx);
        }

        private string ScrapeLastAnalysis(IDocument lastRaceDocument, int position)
        {
            const String entrantSelector = "div.hr-racing-runner-form-trainer-jockey-block";
            var entrantElements = lastRaceDocument.QuerySelectorAll(entrantSelector);

            const String analysisSelector = "div.hr-racing-runner-ride-desc-info";
            return ScrapeTextContent(entrantElements[position - 1], analysisSelector);
        }

        private String ScrapeLastWinningTime(IDocument lastRaceDocument)
        {
            const String selector = "span.hr-racecard-weighed-in-wt > span";
            return ScrapeTextContent(lastRaceDocument, selector);
        }

        private String ScrapeLastOdds(IDocument lastRaceDocument, int position)
        {
            const String entrantSelector = "div.hr-racing-runner-position-container";
            var entrantElements = lastRaceDocument.QuerySelectorAll(entrantSelector);

            const String oddsSelector = "span.hr-racing-runner-betting-info";
            return ScrapeTextContent(entrantElements[position - 1], oddsSelector);
        }

        private string ScrapeLastWeight(IDocument lastRaceDocument, int position)
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
            if(entrantElements.Length == 0)
            {
                log.Error(String.Format("0 entrants found in : {0}", lastRaceDocument.Url));
                return -1;
            }
            // Get the individual finishing distances between runners
            double sumDistance = 0.0;
            IEnumerable<double[]> entrantDistances = entrantElements.Skip(1).Take(position - 1).Select(e => 
            {
                
                if(IsNonRunner(e))
                {
                    return new double [] {-1, -1};
                }

                const String select = "div.hr-racing-runner-space-from-winner";
                var textContent = ScrapeTextContent(e, select);
                if (String.IsNullOrEmpty(textContent))
                {
                    log.Error(String.Format("No distance found in element : {0}, with selector : {1}m url : ", e, select, lastRaceDocument.Url));
                    return new double[] { -1, -1 };
                }

                double currentDistance = 0.0;
                try
                {
                    currentDistance = Distance.ConvertLengthsToDouble(textContent);
                    sumDistance += currentDistance;
                }
                catch (Exception ex)
                {
                    log.Error(String.Format("Failed to conver lengths to double : {0} , {1}", lastRaceDocument.Url, ex.Message));
                }
                return new double[2] {currentDistance, sumDistance};
            });
            var entrantDistancesList = entrantDistances.ToList();

            if (entrantDistancesList.Count == 0)
            {
                log.Error(String.Format("0 distances found in : {0}", lastRaceDocument.Url));
                return -1;
            }

            // min position value = 2
            var beatenLengths = entrantDistancesList[position - 2][1];
            log.Info("Position " + position + " " + entrantDistancesList.Count);


            return beatenLengths;
        }

        private bool IsNonRunner(IElement element)
        {
            const String select = "span.hr-racing-nonrunner-position-no";
            var textContent = ScrapeTextContent(element, select);
            if(!String.IsNullOrEmpty(textContent))
            {
                return true;
            }
            else
            {
                return false;
            }


        }

        String ScrapeLastPositionInField(IElement element)
        {
            const String selector = "table.horse-results-table > tbody > tr:nth-child(1) > td:nth-child(2)";
            //const String rx = "(.+)/.+";
            return ScrapeTextContent(element, selector);
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
            const String rx = ".+,\\s+(.+)$";
            return ScrapeStringFromTextContent(document, selector, rx);
        }



    }
}
