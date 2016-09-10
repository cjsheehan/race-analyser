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
         
    }
}
