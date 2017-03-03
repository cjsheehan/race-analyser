using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Racing
{
    public class Entrant
    {
        public String TrainerName { get; set; }
        public String TrainerUrl { get; set; }
        public String JockeyName { get; set; }
        public String JockeyUrl { get; set; }
        public int JockeyClaim { get; set; }
        public String HorseName { get; set; }
        public String HorseUrl { get; set; }
        public int SaddleNumber { get; set; }
        public int StallNumber { get; set; }
        public int Age { get; set; }
        public int Rating { get; set; }
        public String Form { get; set; }
        public String FormWatch { get; set; }
        public int LastRan { get; set; }
        public String Weight { get; set; }
        public String Odds { get; set; }
        public LastRace LastRace { get; set; }


    }
}
