using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Racing
{
    public class LastRace
    {
        public String Distance { get; set; }
        public String Going { get; set; }
        public String WinningTime { get; set; }
        public Position Position { get; set; }
        public String Weight { get; set; }
        public double BeatenLengths { get; set; }
        public String SP { get; set; }
        public String Trainer { get; set; }
        public String Jockey { get; set; }
        public String Url { get; set; }
        public String Analysis { get; set; }
        public String Course { get; set; }
        public String Class { get; set; }
        public int NumRunners { get; set; }
        public PrizeList LastRacePrizes { get; set; }
    }
}
