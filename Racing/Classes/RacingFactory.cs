using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Racing
{
    public static class RacingFactory
    {
        public static void CreateRace(IRaceHeader header, IRaceDetail details, ref Race race)
        {
            if(header == null) throw new NullReferenceException("IRaceHeader cannot be null at RaceFactory.CreateRace()");
            if(details == null) throw new NullReferenceException("IRaceDetail cannot be null at RaceFactory.CreateRace()");
            
            race = new Race();
            race.Id = header.Id;
            race.Course = header.Course;
            race.Date = header.Date;
            race.Time = header.Time;
            race.Title = header.Title;
            race.Info = header.Info;
            race.Url = header.Url;
            race.Type = header.Type;
            race.MaxOR = details.MaxOR;
            race.MinAge = details.MaxOR;
            race.MaxAge = details.MaxOR;
            race.Prizes  = details.Prizes;
            race.Dist  = details.Dist;
            race.TotalYds = details.TotalYds;
            race.Going = details.Going; 
            race.Class = details.Class;
            race.NumberOfRunners  = details.Runners;
            race.Horses = details.Horses;
        }

        public static void  CreateRaceDTO(IRace raceIn, ref DTO_Race raceOut)
        {
            raceOut = new DTO_Race();
            raceOut.Id = raceIn.Id;
            raceOut.Course = raceIn.Course;
            raceOut.Date = raceIn.Date;
            raceOut.Time = raceIn.Time;
            raceOut.Title = raceIn.Title;
            raceOut.Info = raceIn.Info;
            raceOut.Url = raceIn.Url;
            raceOut.Type = raceIn.Type;
            raceOut.MaxOR = raceIn.MaxOR;
            raceOut.MinAge = raceIn.MinAge;
            raceOut.MaxAge = raceIn.MaxAge;
            raceOut.Prizes = raceIn.Prizes;
            raceOut.Dist = raceIn.Dist;
            raceOut.TotalYds = raceIn.TotalYds;
            raceOut.Going = raceIn.Going;
            raceOut.Egoing = raceIn.Egoing;
            raceOut.Class = raceIn.Class;
            raceOut.Runners = raceIn.Runners;
            raceOut.Horses = new List<DTO_Horse>();
            foreach (var horseIn in raceIn.Horses)
            {
                DTO_Horse horseOut = null;
                CreateHorseDTO(horseIn, ref horseOut);
                if (horseOut != null)
                {
                    raceOut.Horses.Add(horseOut);
                }
                
            }
        }

        public static void CreateHorseDTO(IHorse horseIn, ref DTO_Horse horseOut)
        {
           horseOut = new DTO_Horse();
           horseOut.Name = horseIn.Name;
           horseOut.Url = horseIn.Url;
           horseOut.ExtraInfo = horseIn.ExtraInfo;
           horseOut.Jockey = horseIn.Jockey;
           horseOut.JockeyUrl = horseIn.JockeyUrl;
           horseOut.JockeyPen = horseIn.JockeyPen;
           horseOut.Weight = horseIn.Weight;
           horseOut.Trainer = horseIn.Trainer;
           horseOut.TrainerUrl = horseIn.TrainerUrl;
           horseOut.Age = horseIn.Age;
           horseOut.Placings = horseIn.Placings;
           horseOut.Rating = horseIn.Rating;
           horseOut.Hist = horseIn.Hist;
           horseOut.Naps = horseIn.Naps;
           horseOut.LastRaceDistance = horseIn.LastRaceDistance;
           horseOut.LastRaceGoing = horseIn.LastRaceGoing;
           horseOut.LastRaceWinTime = horseIn.LastRaceWinTime;
           horseOut.LastRacePos = horseIn.LastRacePos;
           horseOut.LastRaceWeight = horseIn.LastRaceWeight;
           horseOut.LastRaceBeatenLengths = horseIn.LastRaceBeatenLengths;
           horseOut.LastRaceSP = horseIn.LastRaceSP;
           horseOut.LastRaceTrainer = horseIn.LastRaceTrainer;
           horseOut.LastRaceJockey = horseIn.LastRaceJockey;
           horseOut.LastRaceUrl = horseIn.LastRaceUrl;
           horseOut.LastRaceAnalysis = horseIn.LastRaceAnalysis;
           horseOut.LastRaceCourse = horseIn.LastRaceCourse;
           horseOut.LastRaceClass = horseIn.LastRaceClass;
           horseOut.LastRaceNumRunners = horseIn.LastRaceNumRunners;
           horseOut.LastRacePrizes = horseIn.LastRacePrizes;
        }

    }
}
