using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using Racing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RacingWebScraper
{
    public partial class SLifeRacingScraper
    {
        async Task<LastRace> ScrapeLastRace(String horseUrl, String horseName)
        {
            // Get horse profile page
            var profileDocument = await HtmlService.GetDocumentAsync(horseUrl).ConfigureAwait(false);

            // Get required data from profile
            var positionInField = ScrapeLastPositionInField(profileDocument.DocumentElement);
            var position = ScrapeLastPosition(profileDocument.DocumentElement);
            //var lastClass = ScrapeLastClass(profileDocument);

            // Get page for last race
            const String lastRaceUrlSelector = "td:nth-child(1) > a.profile-racing-form-racecard-link";

            String lastRaceCardUrl = "";
            try
            {
                lastRaceCardUrl = SITE_PREFIX + ScrapeUrl(profileDocument, lastRaceUrlSelector);
            }
            catch (System.Exception ex)
            {
                log.Error("Failed to get last race card url for : " + horseName);
                throw ex;
            }

            var lastRaceResultUrl = lastRaceCardUrl.Replace("/racecards/", "/results/")
                                        .Replace("/racecard/", "/");
            IDocument lastRaceDocument = null;
            try
            {
                lastRaceDocument = await HtmlService.GetDocumentAsync(lastRaceResultUrl).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                log.Error("Failed to get last race doc from url : " + lastRaceResultUrl);
                throw ex;
            }

            // Scrape data
            LastRace lastRace = new LastRace();
            String errMsg = "Failed to scrape last race data for " + horseName;
            if (!IsWeighedIn(lastRaceDocument)) return lastRace;


            try
            {
                lastRace.Class = ScrapeLastClass(lastRaceDocument);
            }
            catch (System.Exception ex)
            {
                log.Error(errMsg + ": class");
            }

            try
            {
                lastRace.Distance = ScrapeLastDistance(lastRaceDocument);
            }
            catch (System.Exception ex)
            {
                log.Error(errMsg + ": distance");
            }

            try
            {
                lastRace.Going = ScrapeLastGoing(lastRaceDocument);
            }
            catch (System.Exception ex)
            {
                log.Error(errMsg + ": going");
            }

            try
            {
                lastRace.Course = ScrapeLastCourse(lastRaceDocument);
            }
            catch (System.Exception ex)
            {
                log.Error(errMsg + ": course");
            }

            if (position > 0)
            {
                try
                {
                    lastRace.Weight = ScrapeLastWeight(lastRaceDocument, position);
                }
                catch (System.Exception ex)
                {
                    log.Error(errMsg + ": weight" + ex.Message);
                }

                try
                {
                    lastRace.Odds = ScrapeLastOdds(lastRaceDocument, position);
                }
                catch (System.Exception ex)
                {
                    log.Error(errMsg + ": odds");
                }

                try
                {
                    lastRace.Analysis = ScrapeLastAnalysis(lastRaceDocument, position);
                }
                catch (System.Exception ex)
                {
                    log.Error(errMsg + ": analysis");
                }



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

            try
            {
                lastRace.WinningTime = ScrapeLastWinningTime(lastRaceDocument);
            }
            catch (System.Exception ex)
            {
                log.Error(errMsg + ": odds");
            }

            try
            {
                lastRace.LastRacePrizes = ScrapePrizes(lastRaceDocument);
            }
            catch (System.Exception ex)
            {
                log.Error(errMsg + ": prizes");
            }

            //lastRace.Class = lastClass;
            lastRace.Position = positionInField;
            return lastRace;
        }

        private bool IsWeighedIn(IDocument lastRaceDocument)
        {
            const String selector = "div.hr-racecard-weighed-in-wrapper";
            var element = ScrapeTextContent(lastRaceDocument, selector);
            if (!String.IsNullOrEmpty(element))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string ScrapeLastCourse(IDocument lastRaceDocument)
        {
            const String selector = "div.hr-racing-racecard-heading-text > h1";
            const String rx = "\\d{2}:\\d{2}\\s+(.+)";
            return ScrapeStringFromTextContent(lastRaceDocument, selector, rx);
        }

        private string ScrapeLastAnalysis(IDocument lastRaceDocument, int position)
        {
            var entrantElements = ScrapeEntrantsElements(lastRaceDocument);
            const String analysisSelector = "div.hr-racing-runner-ride-desc-info";
            return ScrapeTextContent(entrantElements[position - 1], analysisSelector);
        }

        private String ScrapeLastWinningTime(IDocument lastRaceDocument)
        {
            const String selector = "span.hr-racecard-weighed-in-wt > span";
            return ScrapeTextContent(lastRaceDocument, selector);
        }

        private String ScrapeLastClass(IDocument lastRaceDocument)
        {
            String classInfo = "";
            // (Grade 3) (National Course) (Class 1)
            const String selector = "li.hr-racecard-summary-race-name.hr-racecard-summary-always-open";
            String summary = ScrapeTextContent(lastRaceDocument, selector);

            // check class
            String rx = "\\(Class (\\d)\\)";
            int raceClass = ScrapeIntFromTextContent(lastRaceDocument.DocumentElement, selector, rx);
            classInfo = "C" + raceClass;

            if (raceClass < 1)
            {
                return "";
            }
            else if (raceClass > 1)
            {
                return classInfo;
            }
            else if (raceClass.Equals(1))
            {
                rx = "\\(Grade (\\d)\\)";
                int grade = ScrapeIntFromTextContent(lastRaceDocument.DocumentElement, selector, rx);

                if (grade.Equals(-1))
                {
                    if (summary.Contains("(Listed)"))
                    {
                        return classInfo + " Listed";
                    }
                }
                else
                {
                    return classInfo + " G" + grade;
                }
            }

            return classInfo;
        }

        private String ScrapePrizes(IDocument lastRaceDocument)
        {
            const String selector = "li.hr-racecard-summary-prizes > span:nth-child(1)";
            return ScrapeTextContent(lastRaceDocument, selector).Replace("Winner", "");
        }

        private String ScrapeLastOdds(IDocument lastRaceDocument, int position)
        {
            var entrantElements = ScrapeEntrantsElements(lastRaceDocument);
            const String oddsSelector = "span.hr-racing-runner-betting-info";
            return ScrapeTextContent(entrantElements[position - 1], oddsSelector);
        }

        private string ScrapeLastWeight(IDocument lastRaceDocument, int position)
        {
            var entrantElements = ScrapeEntrantsElements(lastRaceDocument);
            const String selector = "div.hr-racing-runner-horse-sub-info > span:nth-child(2)";
            return ScrapeTextContent(entrantElements[position - 1], selector);
        }

        private static IHtmlCollection<IElement> ScrapeEntrantsElements(IDocument document)
        {
            var selector = "[class^='PreRace__RacecardRunner'] > div";
            return document.QuerySelectorAll(selector);
        }


        private double ScrapeBeatenLengths(IDocument lastRaceDocument, int position)
        {
            // winner has no beaten lengths
            if (position == 1) return 0;

            var entrantElements = ScrapeEntrantsElements(lastRaceDocument);
            if (entrantElements.Length == 0)
            {
                log.Error(String.Format("0 entrants found in : {0}", HtmlService.GetCanonicalUrl(lastRaceDocument)));
                return -1;
            }
            // Get the individual finishing distances between runners
            double sumDistance = 0.0;
            IEnumerable<double[]> entrantDistances = entrantElements.Skip(1).Take(position - 1).Select(e =>
            {

                if (IsNonRunner(e))
                {
                    return new double[] { -1, -1 };
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
                    log.Error(String.Format("Failed to conver lengths to double : {0} , {1}", HtmlService.GetCanonicalUrl(lastRaceDocument), ex.Message));
                }
                return new double[2] { currentDistance, sumDistance };
            });
            var entrantDistancesList = entrantDistances.ToList();

            if (entrantDistancesList.Count == 0)
            {
                log.Error(String.Format("0 distances found in : {0}", HtmlService.GetCanonicalUrl(lastRaceDocument)));
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
            if (!String.IsNullOrEmpty(textContent))
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
            const String selector = "div.profile-results > div > table > tbody > tr:nth-child(1) > td:nth-child(2)";
            return ScrapeTextContent(element, selector);
        }

        int ScrapeLastPosition(IElement element)
        {
            const String selector = "div.profile-results > div > table > tbody > tr:nth-child(1) > td:nth-child(2)";
            const String rx = "(.+)/.+";
            return ScrapeIntFromTextContent(element, selector, rx);
        }


        String ScrapeLastDistance(IDocument document)
        {
            // li.hr-racecard-summary-race-distance.hr-racecard-summary-always-open
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
