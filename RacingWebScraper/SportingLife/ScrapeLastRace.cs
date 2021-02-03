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
            var lastRaceUrlSelector = "[data-test-id='profile-table'] tr:nth-of-type(1) [href*='results']"; 
            //"td:nth-child(1) > a.profile-racing-form-racecard-link";

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
                    string strId = Regex.Match(horseUrl, @"\d+").Value;
                    Int32 horseId = Int32.Parse(strId);
                    lastRace.BeatenLengths = ScrapeBeatenLengths(lastRaceDocument, position, horseId);
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

        private List<int> GetAllFinisherIds(IDocument lastRaceDocument)
        {
            var selector = "[class*='RaceCardsWrapper']:nth-of-type(1) [class^='ResultRunner'] [href^='/racing/profiles/horse/']";
            IHtmlCollection<IElement> elems = lastRaceDocument.QuerySelectorAll(selector);
            List<int> finisherIds = new List<int>();
            foreach (IElement elem in elems)
            {
                string url = elem.GetAttribute("href");
                string strId = Regex.Match(url, @"\d+").Value;
                finisherIds.Add(Int32.Parse(strId));
            }
            return finisherIds;
        }

        private int GetRunnerId(IElement runnerElem)
        {
            var selector = "[href^='/racing/profiles/horse/']";
            IElement urlElem = runnerElem.QuerySelector(selector);
            string url = urlElem.GetAttribute("href");
            string strId = Regex.Match(url, @"\d+").Value;
            return Int32.Parse(strId);
        }

        private bool IsWeighedIn(IDocument lastRaceDocument)
        {
            var selector = "[class^='RacingRacecardSummary__StyledWeighedIn']";
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
            var selector = "[class^='CourseListingHeader__StyledMainTitle']";
            const String rx = "\\d{2}:\\d{2}\\s+(.+)";
            var course = ScrapeStringFromTextContent(lastRaceDocument, selector, rx);
            return course;
        }

        private string ScrapeLastAnalysis(IDocument lastRaceDocument, int position)
        {
            var entrantElements = ScrapeResultRunnerElements(lastRaceDocument);
            var selector = "[data-test-id='ride-description']";
            var textContent = ScrapeTextContent(entrantElements[position - 1], selector);
            return textContent;
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
            var selector = "div > [class^='RacingRacecardSummary__StyledAdditionalInfo-']";
            String summary = ScrapeTextContent(lastRaceDocument, selector);

            // check class
            String rx = "Class (\\d)";
            int raceClass = ScrapeIntFromTextContent(lastRaceDocument, selector, rx);
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
            var entrantElements = ScrapeResultRunnerElements(lastRaceDocument);
            var selector = "[class^='BetLink'] span";
            var odds = ScrapeTextContent(entrantElements[position - 1], selector);
            return odds;
        }

        private string ScrapeLastWeight(IDocument lastRaceDocument, int position)
        {
            var entrantElements = ScrapeResultRunnerElements(lastRaceDocument);
            var selector = "[data-test-id='horse-sub-info'] span:nth-child(2)";
            var weight = ScrapeTextContent(entrantElements[position - 1], selector);
            return weight;
        }



        private static IHtmlCollection<IElement> ScrapeResultRunnerElements(IDocument document)
        {
            var selector = "[class^='ResultRunner__StyledResultRunnerWrapper']";
            return document.QuerySelectorAll(selector);
        }

        private double ScrapeBeatenLengths(IDocument lastRaceDocument, int position, int horseId)
        {
            // winner has no beaten lengths
            if (position == 1) return 0;

            List<int> finisherIds = GetAllFinisherIds(lastRaceDocument);
            if (!finisherIds.Contains(horseId))
            {
                return -1;
            }

            IHtmlCollection<IElement> runnerElements = ScrapeResultRunnerElements(lastRaceDocument);
            if (runnerElements.Length == 0)
            {
                log.Error(String.Format("0 entrants found in : {0}", HtmlService.GetCanonicalUrl(lastRaceDocument)));
                return -1;
            }

            // Sum the finisher distances up to and including the target horseId
            double sumDistance = 0.0;
            // start at i = 1 to skip the winner who has no beaten dist
            for (int i = 1; i < runnerElements.Length; i++)
            {
                IElement elem = runnerElements.ElementAt(i);
                var distSelector = "[class^='StyledRaceCardsWrapper']:nth-of-type(1) [class^='ResultRunner__StyledFinishDistance']";
                string strDist = ScrapeTextContent(elem, distSelector);
                if (String.IsNullOrEmpty(strDist))
                {
                    log.Error(String.Format("No distance found in element : i:{0}, selector:{1} url{2} : ", i, distSelector, lastRaceDocument.Url));
                    return -1;
                }
                var currentDistance = Distance.ConvertLengthsToDouble(strDist);
                sumDistance += currentDistance;
                log.Debug(String.Format("sum distance: idx:{0} strCur:{1} cur:{2} sum:{3}", i, strDist, currentDistance, sumDistance));
                int curHorseId = GetRunnerId(elem);
                if (curHorseId == horseId)
                {
                    break;
                }
            }
            return sumDistance;
        }

        private List<String> GetNonRunnerNames(IDocument lastRaceDocument)
        {
            var selector = "[id='nonrunners'] [data-test-id='runner-horse-name']";
            IHtmlCollection<IElement> elems = lastRaceDocument.QuerySelectorAll(selector);
            List<String> nonRunnerNames = new List<String>();
            foreach (var elem in elems)
            {
                var textContent = elem.TextContent;
            }
            return nonRunnerNames;
        }

        private List<int> GetNonRunnerClothNos(IDocument lastRaceDocument)
        {
            var selector = "[id='nonrunners'] [data-test-id='runner-cloth-number']";
            IHtmlCollection<IElement> elems = lastRaceDocument.QuerySelectorAll(selector);
            List<int> nonRunnerClothNos = new List<int>();
            foreach (var elem in elems)
            {
                var textContent = elem.TextContent;
                if (textContent != null)
                {
                    Regex rx = new Regex("(\\d+)");
                    Match match = rx.Match(textContent);
                    if (match.Success)
                    {
                        int clothNo;
                        bool res = int.TryParse(match.Groups[1].Value, out clothNo);
                        if (res == true)
                        {
                            nonRunnerClothNos.Add(clothNo);
                        }
                    }
                }
            }
            return nonRunnerClothNos;
        }

        private bool IsNonRunner(IElement element, List<int> nonRunnerClothNos)
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
            var selector = "[data-test-id='profile-table'] tr:nth-of-type(1) > td:nth-of-type(2)";
            var textContent = ScrapeTextContent(element, selector);
            return textContent;
        }

        int ScrapeLastPosition(IElement element)
        {
            var selector = "[data-test-id='profile-table'] tr:nth-of-type(1) > td:nth-of-type(2)";
            const String rx = "(.+)/.+";
            var pos = ScrapeIntFromTextContent(element, selector, rx);
            return pos;
        }


        String ScrapeLastDistance(IDocument document)
        {
            var selector = "[class^='RacingRacecardSummary__StyledAdditionalInfo']";
            var textContent = ScrapeTextContent(document, selector);
            var distance = textContent
                .Split('|').ElementAt(1)
                .Trim();
            return distance;
        }
        private string ScrapeLastGoing(IDocument document)
        {
            var selector = "[class^='RacingRacecardSummary__StyledAdditionalInfo']";
            var textContent = ScrapeTextContent(document, selector);
            var going = textContent
                .Split('|').ElementAt(2)
                .Trim();
            return going;
        }



    }
}
