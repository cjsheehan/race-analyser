﻿using AngleSharp.Dom;
using Racing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RacingWebScraper
{
    public partial class SLifeRacingScraper
    {
        private Entrant ScrapeEntrant(IElement element)
        {
            var entrant = new Entrant();
            entrant.SaddleNumber = ScrapeSaddleNumber(element);
            entrant.StallNumber = ScrapeStallNumber(element);
            entrant.HorseUrl = ScrapeHorseUrl(element);
            entrant.HorseName = ScrapeHorseName(element);
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


            // SL: profile only contains races that horse successfully completed

            if (!String.IsNullOrEmpty(entrant.Form))
            {
                if(IsLastRaceFinisher(entrant.Form))
                {
                    // Finisher
                    entrant.LastRace = ScrapeLastRace(entrant.HorseUrl).Result;
                } 
                else
                {
                    // Non-Finisher
                    entrant.LastRace = new LastRace();
                    entrant.LastRace.Position = ScrapeLastPositionFromForm(entrant.Form);
                }
            }

            else
            {
                // No History
                entrant.LastRace = new LastRace();
                entrant.LastRace.Position = "No Form";
            }

            return entrant;
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

        private bool IsLastRaceFinisher(String form)
        {
            const String pattern = "\\d$";
            Regex rx = new Regex(pattern);
            Match m = rx.Match(form);
            if(m.Success)
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
            const String selector = "a.hr-racing-runner-form-trainer";
            return SITE_PREFIX + ScrapeUrl(element, selector);
        }

        private String ScrapeTrainerName(IElement element)
        {
            const String selector = "a.hr-racing-runner-form-trainer";
            const String rx = "T:\\s*(.+)";
            return ScrapeStringFromTextContent(element, selector, rx);
        }

        private String ScrapeJockeyUrl(IElement element)
        {
            const String selector = "a.hr-racing-runner-form-jockey";
            return SITE_PREFIX + ScrapeUrl(element, selector);
        }

        private String ScrapeJockeyName(IElement element)
        {
            const String selector = "span.hr-racing-runner-form-jockey-name";
            return ScrapeTextContent(element, selector);
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
            return ScrapeTextContent(element, selector);
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
            const String selector = "div.hr-racing-runner-form-watch-info";
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
            const String selector = "div.hr-racing-runner-horse-sub-info > span:nth-child(3)";
            const String rx = "OR:\\s+(.+)";
            return ScrapeIntFromTextContent(element, selector, rx);
        }

        private int ScrapeHorseAge(IElement element)
        {
            const String selector = "div.hr-racing-runner-horse-sub-info > span:nth-child(1)";
            const String rx = "(\\d+)";
            return ScrapeIntFromTextContent(element, selector, rx);
        }

        private String ScrapeHorseName(IElement element)
        {
            const String selector = "span.hr-racing-runner-horse-name > a";
            return ScrapeTextContent(element, selector);
        }

        private String ScrapeHorseUrl(IElement element)
        {
            const String selector = "span.hr-racing-runner-horse-name > a";
            return SITE_PREFIX + ScrapeUrl(element, selector);
        }

        private int ScrapeStallNumber(IElement element)
        {
            const String selector = "span.hr-racing-runner-stall-no";
            const String rx = "(\\d+)";
            return ScrapeIntFromTextContent(element, selector, rx);
        }

        private int ScrapeSaddleNumber(IElement element)
        {
            const String selector = "span.hr-racing-runner-saddle-cloth-no";
            const String rx = "(\\d+)";
            return ScrapeIntFromTextContent(element, selector, rx);
        }

        private String ScrapeOdds(IElement element)
        {
            const String selector = "a.sui-odds";
            return ScrapeTextContent(element, selector);
        }

    }

}