using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Racing
{
    public interface IHorse
    {
        string Name { get; set; }
        string Url { get; set; }
        string ExtraInfo { get; set; }
        string Jockey { get; set; }
        string JockeyUrl { get; set; }
        string JockeyPen { get; set; }
        string Weight { get; set; }
        string Trainer { get; set; }
        string TrainerUrl { get; set; }
        string Age { get; set; }
        string Placings { get; set; }
        string Rating { get; set; }
        string Hist { get; set; }
        string Naps { get; set; }
        string LastRaceDistance { get; set; }
        string LastRaceGoing { get; set; }
        string LastRaceWinTime { get; set; }
        string LastRacePos { get; set; }
        string LastRaceWeight { get; set; }
        string LastRaceBeatenLengths { get; set; }
        string LastRaceSP { get; set; }
        string LastRaceTrainer { get; set; }
        string LastRaceJockey { get; set; }
        string LastRaceUrl { get; set; }
        string LastRaceAnalysis { get; set; }
        string LastRaceCourse { get; set; }
        string LastRaceClass { get; set; }
        string LastRaceNumRunners { get; set; }
        PrizeList LastRacePrizes { get; set; }
    }
}
