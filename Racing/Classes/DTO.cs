using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Racing
{
    [Serializable]
    public class DTO_Race
    {
        public DTO_Race()
        {
        }
        public int Id { get; set; }
        public string Course { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Title { get; set; }
        public string Info { get; set; }
        public string Url { get; set; }
        public RaceType Type { get; set; }
        public uint MaxOR { get; set; }
        public uint? MinAge { get; set; }
        public uint? MaxAge { get; set; }
        public PrizeList Prizes { get; set; }
        public string Dist { get; set; }
        public uint TotalYds { get; set; }
        public string Going { get; set; }
        public Going Egoing { get; set; }
        public string Class { get; set; }
        public string Runners { get; set; }
        public List<DTO_Horse> Horses { get; set; }
    }

    public class DTO_Horse
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string ExtraInfo { get; set; }
        public string Jockey { get; set; }
        public string JockeyUrl { get; set; }
        public string JockeyPen { get; set; }
        public string Weight { get; set; }
        public string Trainer { get; set; }
        public string TrainerUrl { get; set; }
        public string Age { get; set; }
        public string Placings { get; set; }
        public string Rating { get; set; }
        public string Hist { get; set; }
        public string Naps { get; set; }
        public string LastRaceDistance { get; set; }
        public string LastRaceGoing { get; set; }
        public string LastRaceWinTime { get; set; }
        public string LastRacePos { get; set; }
        public string LastRaceWeight { get; set; }
        public string LastRaceBeatenLengths { get; set; }
        public string LastRaceSP { get; set; }
        public string LastRaceTrainer { get; set; }
        public string LastRaceJockey { get; set; }
        public string LastRaceUrl { get; set; }
        public string LastRaceAnalysis { get; set; }
        public string LastRaceCourse { get; set; }
        public string LastRaceClass { get; set; }
        public string LastRaceNumRunners { get; set; }
        public PrizeList LastRacePrizes { get; set; }
    }
}
