using AngleSharp.Dom;
using Racing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Racing;
using System.Text.RegularExpressions;

namespace RacingWebScraper
{
    public partial class SLifeRacingScraper
    {
        private Entrant ScrapeEntrant(IElement element)
        {
            var saddleNumber = ScrapeSaddleNumber(element);
            var stallNumber = ScrapeStallNumber(element);
            var horseUrl = ScrapeHorseUrl(element);
            var horseName = ScrapeHorseName(element);
            var horseAge = ScrapeHorseAge(element);
            var horseOfficialRating = ScrapeHorseOfficialRating(element);
            var horseForm = ScrapeHorseForm(element);
            var horseFormWatch = ScrapeHorseFormWatch(element);
            var horseLastRan = ScrapeHorseLastRan(element);
            var horseWeight = ScrapeHorseWeight(element);
            var jockeyName = ScrapeJockeyName(element);
            var jockeyUrl = ScrapeJockeyUrl(element);
            var jockeyClaim = ScrapeJockeyClaim(element);
            var trainerName = ScrapeTrainerName(element);
            var trainerUrl = ScrapeTrainerUrl(element);
            var lastRace = ScrapeLastRace(horseUrl);
            var entrant = new Entrant();
            return null;
        }


        private String ScrapeLastRace(String horseUrl)
        {
            const String selector = "a.hr-racing-runner-form-trainer";
            return null;
        }


        private String ScrapeTrainerUrl(IElement element)
        {
            const String selector = "a.hr-racing-runner-form-trainer";
            return SITE_PREFIX + ScrapeUrl(element, selector);
        }

        private String ScrapeTrainerName(IElement element)
        {
            const String selector = "a.hr-racing-runner-form-trainer";
            var textContent = ScrapeTextContent(element, selector);

            if (textContent != null)
            {
                return Regex.Replace(textContent, "T:\\s*", "");
            }
            else
            {
                return "";
            }
        }

        private String ScrapeJockeyUrl(IElement element)
        {
            const String selector = "a.hr-racing-runner-form-jockey";
            return SITE_PREFIX + ScrapeUrl(element, selector);
        }

        private String ScrapeJockeyName(IElement element)
        {
            const String selector = "a.hr-racing-runner-form-jockey";
            var textContent = ScrapeTextContent(element, selector);

            if (textContent != null)
            {
                Regex rxName = new Regex("J:\\s*(.+?)\\s*\\(");
                Match match = rxName.Match(textContent);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            return "";
   
        }


        private int ScrapeJockeyClaim(IElement element)
        {
            const String selector = "a.hr-racing-runner-form-jockey";
            return 0;
        }

        private String ScrapeHorseWeight(IElement element)
        {
            const String selector = "";
            return null;
        }

        private int ScrapeHorseLastRan(IElement element)
        {
            const String selector = "";
            return 0;
        }

        private String ScrapeHorseFormWatch(IElement element)
        {
            const String selector = "";
            return null;
        }

        private String ScrapeHorseForm(IElement element)
        {
            const String selector = "";
            return null;
        }

        private int ScrapeHorseOfficialRating(IElement element)
        {
            const String selector = "";
            return 0;
        }

        private int ScrapeHorseAge(IElement element)
        {
            const String selector = "";
            return 0;
        }

        private String ScrapeHorseName(IElement element)
        {
            const String selector = "";
            return null;
        }

        private String ScrapeHorseUrl(IElement element)
        {
            const String selector = "";
            return null;
        }

        private int ScrapeStallNumber(IElement element)
        {
            const String selector = "";
            return 0;
        }

        private int ScrapeSaddleNumber(IElement element)
        {
            const String selector = "";
            return 0;
        }

        private String ScrapeUrl(IElement element, String selector)
        {
            return element.QuerySelector(selector).GetAttribute("href");
        }

    }

}
