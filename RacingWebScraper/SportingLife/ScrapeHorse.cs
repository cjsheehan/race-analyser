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
        private Entrant ScrapeHorse(String data)
        {
            Entrant horse = new Entrant();

            // Split horse data around html list items "li"
            MatchCollection HorseElements = RXSL.rxListItemCln.Matches(data);

            foreach (Match match in HorseElements)
            {
                if (match.Success)
                {
                    String id = match.Groups[1].ToString();
                    String val = match.Groups[2].ToString();
                    //Console.WriteLine(String.Format(id + val));

                    // Check if invalid
                    Match mInvalid = RXSL.rxInvalidItem.Match(val);
                    if (!mInvalid.Success)
                    {
                        switch (id)
                        {
                            case "T":
                                horse.Trainer = Utility.RemoveWhitespace(val);
                                break;

                            case "J":
                                horse.Jockey = Utility.RemoveWhitespace(val);
                                break;

                            case "Form":
                                horse.Placings = Utility.RemoveWhitespace(val);
                                break;

                            case "Age":
                                horse.Age = Utility.RemoveWhitespace(val);
                                break;

                            case "Weight":
                                horse.Weight = Utility.RemoveWhitespace(val);
                                break;

                            case "OR":
                                horse.Rating = Utility.RemoveWhitespace(val);
                                break;

                            case "Naps":
                                horse.Naps = Utility.RemoveWhitespace(val);
                                break;

                            default:

                                break;
                        }
                    }
                }
            } // foreach (Match match in HorseElements)

            // Get horse (name/link), trainer(link), jockey(link)
            Match mHorse = RXSL.rxHorse.Match(data);
            if (mHorse.Success)
            {
                horse.Name = mHorse.Groups[2].Value;
                horse.Url = SITE_PREFIX + mHorse.Groups[1].Value;
            }

            Match mTrainer = RXSL.rxTrainer.Match(data);
            if (mTrainer.Success)
            {
                horse.TrainerUrl = SITE_PREFIX + mTrainer.Groups[1].Value;
                if (String.IsNullOrEmpty(horse.Trainer))
                {
                    horse.Trainer = mTrainer.Groups[2].Value; // 2nd bite at cherry
                }
            }

            Match mJockey = RXSL.rxJockey.Match(data);
            if (mJockey.Success)
            {
                horse.JockeyUrl = SITE_PREFIX + mJockey.Groups[1].Value;
                Match mJockeyPen = RXSL.rxJockeyPen.Match(mJockey.Groups[3].Value);
                if (mJockeyPen.Success)
                    horse.JockeyClaim = mJockeyPen.Groups[1].Value;
                else
                    horse.JockeyClaim = "0";

                if (String.IsNullOrEmpty(horse.Jockey))
                {
                    horse.Jockey = mJockey.Groups[2].Value; // 2nd bite at cherry
                }
            }

            //ScrapeHorseProfile(horse);

            //if (String.IsNullOrEmpty(horse.LastRaceUrl) == false)
            //{
            //    ScrapeLastRaceData(horse);
            //}

            return horse;
        } // CreateHorse

        private void ScrapeHorseProfile(IHorse horse)
        {
            String page = WebPage.Get(horse.Url);
            page = PreFormatPage(page);

            // Break the horse profile page into valid data
            Match m = RXSL.rxPrevRaceRow.Match(page);

            if (m.Success == true)
            {
                MatchCollection mc = RXSL.rxPrevRaceCell.Matches(m.Groups[0].Value);
                int i = 0; // 16 cells total.  Only need certain row data so use an index i
                foreach (Match mCell in mc)
                {
                    String cell = mCell.Groups[0].Value;
                    // Link

                    // Get Wgt, DB, StartPrice when i=0
                    if (i == 0)
                    {
                        // Split horse data around html list items "li"
                        MatchCollection RaceElements = RXSL.rxListItemCln.Matches(m.Groups[0].Value);
                        foreach (Match mElem in RaceElements)
                        {
                            if (mElem.Success)
                            {
                                String id = mElem.Groups[1].ToString();
                                String val = mElem.Groups[2].ToString();
                                //Console.WriteLine(String.Format(id + val));
                                switch (id)
                                {
                                    case "T":
                                        horse.LastRaceTrainer = Utility.RemoveWhitespace(val);
                                        break;

                                    case "J":
                                        horse.LastRaceJockey = Utility.RemoveWhitespace(val);
                                        break;

                                    case "Wt":
                                        horse.LastRaceWeight = Utility.RemoveWhitespace(val);
                                        break;

                                    case "DB":
                                        horse.LastRaceBeatenLengths = Utility.RemoveWhitespace(val);
                                        break;

                                    case "SP":
                                        horse.LastRaceSP = Utility.RemoveWhitespace(val);
                                        break;

                                    default:

                                        break;
                                }
                            }
                        }// foreach (Match match in HorseElements)

                        // Get prev race link
                        Match mPrevRaceLk = RXSL.rxPrevRaceLk.Match(cell);
                        if (mPrevRaceLk.Success)
                            horse.LastRaceUrl = SITE_PREFIX + Utility.RemoveWhitespace(mPrevRaceLk.Groups[1].Value);

                        // Get comment
                        Match mCmt = RXSL.rxCmt.Match(cell);
                        if (mCmt.Success)
                            horse.LastRaceAnalysis = Utility.RemoveWhitespace(mCmt.Groups[1].Value);
                    }
                    // position
                    else if (i == 1)
                    {
                        Match mPos = RXSL.rxPrevRacePos.Match(cell);
                        if (mPos.Success)
                            horse.LastRacePos = Utility.RemoveWhitespace(mPos.Groups[1].Value);
                    }
                    // Weight
                    else if (i == 2)
                    {
                        Match mWgt = RXSL.rxPrevRaceWgt.Match(cell);
                        if (mWgt.Success)
                            horse.LastRaceWeight = Utility.RemoveWhitespace(mWgt.Groups[1].Value);
                    }
                    // Course
                    else if (i == 4)
                    {
                        Match mCourse = RXSL.rxPrevRaceCourse.Match(cell);
                        if (mCourse.Success)
                            horse.LastRaceCourse = Utility.RemoveWhitespace(mCourse.Groups[1].Value);
                        break;
                    }
                    i++;
                }
            }
        }

        private void ScrapeLastRaceData(IHorse horse)
        {
            String page = WebPage.Get(horse.LastRaceUrl);
            page = PreFormatPage(page);
            // page horse data around html list items "li"
            Match raceHeader = RXSL.rxSecHdr.Match(page);
            if (raceHeader.Success)
            {
                MatchCollection RaceElements = RXSL.rxListItem.Matches(raceHeader.Groups[1].Value);
                int i = 0;
                foreach (Match m in RaceElements)
                {
                    if (i == 0)
                    {
                        String[] items = m.Groups[1].Value.Split(',');
                        if (items != null)
                        {
                            if (items.Length == 4)
                            {
                                horse.LastRaceAnalysis += Utility.RemoveWhitespace(items[0]); // Age info e.g. 3yo+
                                horse.LastRaceDistance = Utility.RemoveWhitespace(items[1]); // prev distance
                                horse.LastRaceClass = ExtractClass(items);
                                horse.LastRaceNumRunners = ExtractRunners(items);
                            }
                            else if (items.Length == 3)
                            {
                                horse.LastRaceDistance = Utility.RemoveWhitespace(items[1]); // prev distance
                                horse.LastRaceNumRunners = ExtractRunners(items);
                            }

                        }
                    }
                    else if (i == 1)
                    {
                        PrizeList pl = GetPrizeMoney(m.Groups[1].Value);
                        if (pl != null) horse.LastRacePrizes = pl;
                    }
                    else if (i == 2)
                    {
                        Match mGoing = RXSL.rxGoing.Match(m.Groups[1].Value);
                        if (mGoing.Success)
                            horse.LastRaceGoing = Utility.RemoveWhitespace(mGoing.Groups[1].Value);
                    }
                    else if (i == 4)
                    {
                        Match mWinTime = RXSL.rxPrevRaceWinTime.Match(m.Groups[1].Value);
                        if (mWinTime.Success)
                            horse.LastRaceWinTime = Utility.RemoveWhitespace(mWinTime.Groups[1].Value);
                    }
                    i++;
                }
            }
        }
    }
}
