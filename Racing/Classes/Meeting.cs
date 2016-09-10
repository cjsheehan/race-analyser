using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Racing
{
    public class Meeting : IMeeting
    {
        public string Course { get; set; }
        public string CourseId { get; set; }
        public string Date { get; set; }
        public string Going { get; set; }
        public string Weather { get; set; }
        public List<IRace> RaceCards { get; set; }


        public Meeting(String course, String going, String weather, List<IRace> raceCards)
            : this(course, going, weather)
        {
            RaceCards = raceCards;
        }

        public Meeting(String course, String going, String weather)
        {
            Course = course;
            Going = going;
            Weather = weather;
            RaceCards = new List<IRace>();
        }

        //public int AddRace(Race race)
        //{
        //    Races.Add(race);
        //    return Races.Count;
        //}

        //public override string ToString()
        //{
        //    string s =
        //        "Course : " + Course + Environment.NewLine + "\t"
        //        + CourseId + Environment.NewLine + "\t";

        //    for (int i = 0; i < Races.Count; i++)
        //    {
        //        s += "Race [" + i + "] :" + Races[i].ToString();
        //    }
        //    return s;
        //}
    }
}
