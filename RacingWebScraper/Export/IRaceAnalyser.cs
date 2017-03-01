using System;
using System.Collections.Generic;

namespace RacingWebScraper
{
    interface IRaceAnalyser
    {
        System.Threading.Tasks.Task Analyse(List<Racing.Race> races);
    }
}
