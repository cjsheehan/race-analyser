using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RacingWebScraper
{
    internal static class RXSL
    {
        public static Regex rxMeetingSec = new Regex(@"<section class=""rac-dgp"">(.+?)</section>");
        public static Regex rxMeeting = new Regex(@"<a href=""/racing/meeting/(.+?)/(.+?)"" class=""sui-meeting""");
        public static Regex rxRaceCard = new Regex(@"<li class=""rac-cards""(.+?)</li>");
        public static Regex rxRaceStubBoundary = new Regex(@"<li class=""rac-cards(.+?)class=""ix ixv""");
        public static Regex rxRaceStub = new Regex(@"(\d\d:\d\d).+(/racing/racecards/(.+?)/(.+?)/racecard/(\d+)/.+?)"" class=""ixa""> (.+?) </a>");  // date,course,id,race,info


        // Race Info
        // Short link
        public static Regex rxShortLk = new Regex(@"http://www.sportinglife.com/(.+)");

        // Content Header
        public static Regex rxContent = new Regex(@"<div class=""content-header"">.+?<li>\((.+?)\)</li>(.+?)<li><strong>Going:</strong>(.+?)</li>.+?</div>");
        public static Regex rxContentAlt1 = new Regex(@"<div class=""content-header"">.+?<li>\((.+?)\)</li>");
        
        //Prize money
        //private Regex rxPrizes = new Regex(@"racecard-.+?&(euro|pound|dollar);(\d\s+?)");
        public static Regex rxPrizes = new Regex(@"&(euro|pound);\s*(\d\S+\d)");

        // Number runners
        public static Regex rxRunners = new Regex(@"(\d+)\s*runners");

        // Class
        public static Regex rxClass = new Regex(@"lass\s*(\d)");

        // Going
        public static Regex rxGoing = new Regex(@"<strong>Going:</strong>(.+)");

        // Info
        //private Regex rxInfo = new Regex(@"<div class=""race_name"" id=""race_title_hdr"">(.+?)</div>?");

        // Horse profile page parser
        public static Regex rxHorseRow = new Regex(@"<tr(.+?racing/profiles/horse.+?)</tr>");
        public static Regex rxHorseCell = new Regex(@"<td.+?</td>");
        public static Regex rxListItemCln = new Regex(@"<li>(.+?):(.+?)</li>");
        public static Regex rxListItem = new Regex(@"<li>(.+?)</li>");
        public static Regex rxInvalidItem = new Regex(@"<");
        public static Regex rxHorse = new Regex(@"(/racing/profiles/horse/\d+/.+?)"">(.+?)</a>"); // e.g. <a href="/racing/profiles/horse/710390/corton-lad"> Corton Lad </a>
        public static Regex rxJockey = new Regex(@"(/racing/profiles/jockey/\d+/.+?)"">(.+?)</a>(.+?)<td"); // e.g. a href="/racing/profiles/trainer/97286/k-dalgleish">K Dalgleish </a>
        public static Regex rxJockeyPen = new Regex(@"""note"">\((\d)\)</em>");
        public static Regex rxTrainer = new Regex(@"(/racing/profiles/trainer/\d+/.+?)"">(.+?)</a>"); // e.g. <a href="/racing/profiles/jockey/103/t-eaves"> T Eaves </a>
        public static Regex rxNoDays = new Regex(@">\s*(\S+)\s*</td>");
        public static Regex rxHist = new Regex(@"img src=""/Images/History.+alt=""(.+)""");
        public static Regex rxPrevRaceRow = new Regex(@"<tr class=""fltr-item(.+?)</tr>");
        public static Regex rxPrevRaceCell = new Regex(@"<td(.+?)</td>");

        // previous race link:  required to get the winning time
        public static Regex rxPrevRaceLk = new Regex(@"href=""(.+?)"">");
        // Prev Race performance comments
        public static Regex rxCmt = new Regex(@"title=""(.+?)""");
        // position
        public static Regex rxPrevRacePos = new Regex(@">.*?(\S+).*</td>");
        // Dist, Going, Class
        public static Regex rxPrevRaceDGC = new Regex(@">(.+?)</td>");
        // weight
        public static Regex rxPrevRaceWgt = new Regex(@"(\d+-\d+)");
        // distance
        //private Regex rxPrevRaceDist = new Regex(@">.*?(\S+).*</td>");
        // Course
        public static Regex rxPrevRaceCourse = new Regex(@"pf-cse"">(.+?)<");
        // going
        public static Regex rxPrevRaceGoing = new Regex(@">.*?(\S+).*</td>");
        // class
        public static Regex rxPrevRaceClass = new Regex(@">.*?(\S+).*</td>");
        // distance beaten
        public static Regex rxPrevRaceBeaten = new Regex(@">.*?(\S+).*</td>");
        // Data from actual last race page as horse profile doesn't give complete distance
        public static Regex rxDiv = new Regex(@"<div.+?</div>");
        public static Regex rxSecHdr = new Regex(@"<section class=""racecard-header"">(.+?)</section>");
        // Data from actual last race page as horse profile doesn't give complete distance
        public static Regex rxSpan = new Regex(@"<span.+?</span>");
        // Prev Race distance : To be found in a match from rxDiv
        public static Regex rxPrevRaceDist = new Regex(@"race_details"">.+?,.+?,(.+?),.+numrunners?");
        // Winning time : To be found in a match from rxSpan
        public static Regex rxPrevRaceWinTime = new Regex(@"inning time:\s*(.+)");
    }
}
