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
        async Task<List<Entrant>> ScrapeEntrantsAsync(IDocument document)
        {

            var entrantsElements = ScrapeEntrantsElements(document);
            const String dateSelector = "div#hr-course-header-title > .page-main-subtitle";
            String strRaceDate = ScrapeTextContent(document, dateSelector);
            var raceDate = Convert.ToDateTime(strRaceDate);

            log.Debug("Scraping Entrants :  " + document.Url);
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
            const String dateSelector = "div.profile-results > div > table > tbody > tr:nth-child(1) > td:nth-child(1) > a";
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
            const String selector = ".hr-racing-runner-horse-sub-info > a:nth-of-type(2)";
            return SITE_PREFIX + ScrapeUrl(element, selector);
        }

        private String ScrapeTrainerName(IElement element)
        {
            const String selector = ".hr-racing-runner-horse-sub-info > a:nth-of-type(2)";
            const String rx = "T:\\s*(.+)";
            return ScrapeStringFromTextContent(element, selector, rx);
        }

        private String ScrapeJockeyUrl(IElement element)
        {
            const String selector = ".hr-racing-runner-horse-sub-info > a:nth-of-type(1)";
            return SITE_PREFIX + ScrapeUrl(element, selector);
        }

        private String ScrapeJockeyName(IElement element)
        {
            const String selector = ".hr-racing-runner-horse-sub-info > a:nth-of-type(1) > span";
            const String rx = "J:\\s*(.+)";
            return ScrapeStringFromTextContent(element, selector, rx);
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
            const String selector = "div.hr-racing-runner-horse-sub-info > span:nth-child(2)";
            const String rx = "Weight:\\s*(.+)";
            return ScrapeStringFromTextContent(element, selector, rx);
        }

        private int ScrapeHorseLastRan(IElement element)
        {
            const String selector = "sup.hr-racing-runner-horse-last-ran";
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
            const String selector = ".hr-racing-runner-form-watch-info-full";
            return ScrapeTextContent(element, selector);
        }

        private String ScrapeHorseForm(IElement element)
        {
            const String selector = "label.hr-racing-runner-form-stat";
            const String rx = "Form:\\s+(.+)";
            return ScrapeStringFromTextContent(element, selector, rx);
        }



        private int ScrapeHorseOfficialRating(IElement element)
        {
            const String selector = ".hr-racing-runner-horse-sub-info > span:nth-child(5)";
            const String rx = ".*OR:\\s+(.+)";
            return ScrapeIntFromTextContent(element, selector, rx);
        }

        private int ScrapeHorseAge(IElement element)
        {
            const String selector = ".hr-racing-runner-horse-sub-info > span:nth-child(1)";
            const String rx = "(\\d+)";
            return ScrapeIntFromTextContent(element, selector, rx);
        }

        private String ScrapeHorseName(IElement element)
        {
            const String selector = ".hr-racing-runner-horse-name > a";
            return ScrapeTextContent(element, selector);
        }

        private String ScrapeHorseUrl(IElement element)
        {
            const String selector = ".hr-racing-runner-horse-name > a";
            return SITE_PREFIX + ScrapeUrl(element, selector);
        }

        private int ScrapeStallNumber(IElement element)
        {
            const String selector = ".hr-racing-runner-stall-no";
            const String rx = "(\\d+)";
            return ScrapeIntFromTextContent(element, selector, rx);
        }

        private int ScrapeSaddleNumber(IElement element)
        {
            const String selector = ".hr-racing-runner-saddle-cloth-no";
            const String rx = "(\\d+)";
            return ScrapeIntFromTextContent(element, selector, rx);
        }

        private String ScrapeOdds(IElement element)
        {
            const String selector = ".hr-racing-runner-betting-link.sui-odds";
            return ScrapeTextContent(element, selector);
        }

    }

}
