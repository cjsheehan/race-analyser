using AngleSharp.Dom;
using Racing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using AngleSharp.Parser.Html;
using System.Collections.Concurrent;

namespace RacingWebScraper
{
    public partial class SLifeRacingScraper
    {
        //const String entrantsSelector = "section.hr-racing-runner-wrapper";
        async Task<List<Entrant>> ScrapeEntrantsAsync(IDocument document, String docUrl)
        {

            var entrantsElements = ScrapeRacecardRunnerElements(document);
            var dateSelector = "[data-test-id='racecard-title'] [class^= 'CourseListingHeader__StyledMainSubTitle']";
            String strRaceDate = ScrapeTextContent(document, dateSelector);
            var raceDate = Convert.ToDateTime(strRaceDate);

            log.Debug("Scraping Entrants :  " + docUrl);
            var parallelEntrants = new ConcurrentQueue<Entrant>();
            var tasks = entrantsElements.Select(async element =>
            {
                var entrant = new Entrant();
                try
                {
                    entrant.HorseName = ScrapeHorseName(element);
                    entrant.SaddleNumber = ScrapeSaddleNumber(element);
                    entrant.StallNumber = ScrapeStallNumber(element);
                    entrant.HorseUrl = ScrapeHorseUrl(element);
                    entrant.Age = ScrapeHorseAge(element);
                    entrant.Rating = ScrapeHorseOfficialRating(element);
                    entrant.Form = ScrapeHorseForm(element);
                    entrant.FormWatch = ScrapeHorseFormWatch(element);
                    entrant.LastRan = ScrapeHorseLastRan(element);
                    entrant.Weight = ScrapeHorseWeight(element);
                    entrant.JockeyName = ScrapeJockeyName(element);
                    entrant.JockeyUrl = ScrapeJockeyUrl(element);
                    entrant.JockeyClaim = ScrapeJockeyClaim(element);
                    entrant.TrainerName = ScrapeTrainerName(element);
                    entrant.TrainerUrl = ScrapeTrainerUrl(element);
                    entrant.Odds = ScrapeOdds(element);

                    log.Debug("In Entrant " + entrant.HorseName);
                    // Test whether profile contains valid last ran data
                    // SL: only displays races where runner finished race (If horse is
                    // non-finisher data is in different table for also rans)
                    // Therefore needs compared to last ran days from racecard
                    var isValid = await IsLastRanDataValidAsync(element, raceDate).ConfigureAwait(false);

                    if (isValid)
                    {
                        entrant.LastRace = await ScrapeLastRace(entrant.HorseUrl, entrant.HorseName).ConfigureAwait(false);
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(entrant.Form))
                        {
                            entrant.LastRace = new LastRace();
                            entrant.LastRace.Position = ScrapeLastPositionFromForm(entrant.Form);
                        }
                        else
                        {
                            entrant.LastRace = new LastRace();
                            entrant.LastRace.Position = "No Form";
                        }
                    }

                    parallelEntrants.Enqueue(entrant);
                }
                catch (InvalidScrapeException e)
                {
                    log.Error(String.Format("Failed to scrape entrant : {0}, {1}", entrant.HorseUrl, document.Url, e.Message));
                }
                catch (Exception e)
                {
                    log.Error(String.Format("Failed to scrape entrant : {0}, {1}", entrant.HorseUrl, document.Url, e.Message));
                }

            });
            await Task.WhenAll(tasks).ConfigureAwait(false);

            var entrants = new List<Entrant>();
            Entrant entrantOut;
            while (parallelEntrants.TryDequeue(out entrantOut))
            {
                entrants.Add(entrantOut);
            }

            entrants = entrants.OrderBy(x => x.SaddleNumber).ToList();

            return entrants;
        }

        async Task<bool> IsLastRanDataValidAsync(IElement element, DateTime dtRaceDate)
        {
            // Get last ran page form horse profile
            var profileUrl = ScrapeHorseUrl(element);
            var profileDocument = await HtmlService.GetDocumentAsync(profileUrl).ConfigureAwait(false);

            // Get date of last race from profile
            var dateSelector = "[class^='FormTable'] > a:nth-of-type(1)";
            String lastRaceDateOnProfile = ScrapeTextContent(profileDocument, dateSelector);
            if (String.IsNullOrEmpty(lastRaceDateOnProfile)) return false;

            // Get last ran value from racecard element
            int lastRan = ScrapeHorseLastRan(element);

            DateTime dtLast = Convert.ToDateTime(lastRaceDateOnProfile);
            double difference = (dtRaceDate - dtLast).TotalDays;

            // lastRan -1 >= difference <= lastRan + 1 
            if (difference >= lastRan - 1 && difference <= lastRan + 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private static IHtmlCollection<IElement> ScrapeRacecardRunnerElements(IDocument document)
        {
            var selector = "[class^='PreRace__RacecardRunner'] > div";
            return document.QuerySelectorAll(selector);
        }

        private String ScrapeLastPositionFromForm(String form)
        {
            const String pattern = "(\\S)$";
            Regex rx = new Regex(pattern);
            Match m = rx.Match(form);
            if (m.Success)
            {
                return m.Groups[1].Value;
            }
            else
            {
                return "";
            }
        }

        private bool IsLastRaceFormFinisher(String form)
        {
            const String pattern = "\\d$";
            Regex rx = new Regex(pattern);
            Match m = rx.Match(form);
            if (m.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private String ScrapeTrainerUrl(IElement element)
        {
            var selector = "[data-test-id='horse-sub-info'] [href*='trainer']";
            var url = SITE_PREFIX + ScrapeUrl(element, selector);
            return url;
        }

        private String ScrapeTrainerName(IElement element)
        {
            var selector = "[data-test-id='horse-sub-info'] [href*='trainer'] span";
            const String rx = "T:\\s*(.+)";
            var textContent = ScrapeStringFromTextContent(element, selector, rx);
            return textContent;
        }

        private String ScrapeJockeyUrl(IElement element)
        {
            var selector = "[data-test-id='horse-sub-info'] [href*='jockey']";
            var url = SITE_PREFIX + ScrapeUrl(element, selector);
            return url;
        }

        private String ScrapeJockeyName(IElement element)
        {
            var selector = "[data-test-id='horse-sub-info'] [href*='jockey'] span";
            const String rx = "J:\\s*(.+)";
            var textContent = ScrapeStringFromTextContent(element, selector, rx);
            return textContent;
        }


        private int ScrapeJockeyClaim(IElement element)
        {
            const String selector = "a.hr-racing-runner-form-jockey";
            const String rx = "J:\\s*.+?\\s*(\\d)";
            var claim = ScrapeIntFromTextContent(element, selector, rx);
            if (claim != -1)
            {
                return claim;
            }

            return 0;
        }

        private String ScrapeHorseWeight(IElement element)
        {
            const String rx = "Weight:\\s*(.+)";
            var selector = "[data-test-id='horse-sub-info'] > span:nth-child(2)";
            var textContent = ScrapeStringFromTextContent(element, selector, rx);
            return textContent;
        }

        private int ScrapeHorseLastRan(IElement element)
        {
            var selector = "[data-test-id='last-ran']";
            const String rx = "(\\d+)";
            var lastRan = ScrapeIntFromTextContent(element, selector, rx);

            if (lastRan > 0)
            {
                return lastRan;
            }

            return -1;
        }

        private String ScrapeHorseFormWatch(IElement element)
        {
            var selector = "[class^='Runner__StyledCommentary']";
            var textContent = ScrapeTextContent(element, selector);
            return textContent;
        }

        private String ScrapeHorseForm(IElement element)
        {
            var selector = "[data-test-id='show-form'] label";
            const String rx = "Form:\\s+(.+)";
            var textContent = ScrapeStringFromTextContent(element, selector, rx);
            return textContent;
        }



        private int ScrapeHorseOfficialRating(IElement element)
        {
            var selector = "[data-test-id='horse-sub-info']  > span:nth-child(5)";
            const String rx = ".*OR:\\s+(.+)";
            var rating = ScrapeIntFromTextContent(element, selector, rx);
            return rating;
        }

        private int ScrapeHorseAge(IElement element)
        {
            var selector = "[data-test-id='horse-sub-info'] > span:nth-of-type(1)";
            const String rx = "(\\d+)";
            var age = ScrapeIntFromTextContent(element, selector, rx);
            return age;
        }

        private String ScrapeHorseName(IElement element)
        {
            var selector = "[class^='Runner__StyledHorseName'] [id^='horse-number']";
            var textContent = ScrapeTextContent(element, selector);
            return textContent;
        }

        private String ScrapeHorseUrl(IElement element)
        {
            var selector = "[class^='Runner__StyledHorseName'] > a";
            var url = SITE_PREFIX + ScrapeUrl(element, selector);
            return url;
        }

        private int ScrapeStallNumber(IElement element)
        {
            var selector = "[class^='SaddleAndStall__StyledStallNo']";
            const String rx = "(\\d+)";
            var stallNo = ScrapeIntFromTextContent(element, selector, rx);
            return stallNo;
         }

        private int ScrapeSaddleNumber(IElement element)
        {
            var selector = "[class^='SaddleAndStall__StyledSaddleClothNo']";
            const String rx = "(\\d+)";
            var saddleNo = ScrapeIntFromTextContent(element, selector, rx);
            return saddleNo;
        }

        private String ScrapeOdds(IElement element)
        {
            var selector = "[data-metrics-betlink-id]";
            var textContent = ScrapeTextContent(element, selector);
            return textContent;
        }

    }

}
