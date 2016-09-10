using Racing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RacingWebScraper 
{
    public partial class SLifeRacingScraper : IRacingScraper
    {
        public event EventHandler<GetRaceDataAsyncCompletedEventArgs> GetRaceDataAsyncCompleted;
        private const string POUND = "pound";
        private const string EURO = "euro";
        private const string DOLLAR = "dollar";

        private object _safeLock  = new object();

        private int _numReq = 0;  // Used to calculate the increment for progress updates
        private double _progInc = 0; // The increment used in progress updates
        private double _currentProg = 0; // The current progress in progress updates
        private string _currentRace = "";

        private const int MIN_HORSES_PER_RACE = 2;
        protected virtual void OnGetRaceDataAsyncCompleted(List<IRaceDetail> raceData)
        {
            GetRaceDataAsyncCompletedEventArgs args = new GetRaceDataAsyncCompletedEventArgs(raceData);
            EventHandler<GetRaceDataAsyncCompletedEventArgs> handler = GetRaceDataAsyncCompleted;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        private async Task<List<IRaceDetail>> ScrapeRacesAsync(List<String> raceUrls)
        {
            List<IRaceDetail> racesDetail = null;
	        racesDetail = new List<IRaceDetail>();
            double currentProgress = 0;
            double inc = 100 / raceUrls.Count;
            _progress.Report(new BasicUpdate(MIN_PROGRESS, ""));
	        foreach (var url in raceUrls)
	        {
                String raceContent = await WebPage.GetAsync(url);
	            raceContent = PreFormatPage(raceContent);
	            IRaceDetail raceData = ScrapeRaceDetail(raceContent);
	            raceData.Url = url;
                _currentRace = raceData.Url;
                IList<string> horseData = GetHorseDataAsStrings(raceContent);
                raceData.Horses = ScrapeHorsesParallel(horseData);
	            racesDetail.Add(raceData);
                currentProgress += inc;
	        }
            _progress.Report(new BasicUpdate(MAX_PROGRESS, "Completed."));
            return racesDetail;
        }

        private IRaceDetail ScrapeRaceDetail(String raceContent)
        {
            IRaceDetail race = new RaceInfo();

            // Grouped Content : Info, Distance, Class, Num Runners + a match for going
            String content = null;
            string prizeData = null;
            string[] items = null;
            Match mCont = RXSL.rxContent.Match(raceContent);
            if (mCont.Success)
            {
                content = mCont.Groups[1].Value;
                // Going
                race.Going = Utility.RemoveWhitespace(mCont.Groups[2].Value);
                items = content.Split(',');
            }
            else
            {
                mCont = RXSL.rxContentAlt1.Match(raceContent);
            }

            if (mCont.Success)
            {
                content = mCont.Groups[1].Value;
                prizeData = mCont.Groups[2].Value;
                // Going
                race.Going = Utility.RemoveWhitespace(mCont.Groups[3].Value);
                items = content.Split(',');
            }

            if (items != null)
            {
                //race.Info = items[0];
                string info = items[0];
                race.Dist = items[1];

                foreach (String s in items)
                {
                    // Class
                    Match mClass = RXSL.rxClass.Match(s);
                    if (mClass.Success)
                    {
                        race.Class = Utility.RemoveWhitespace(mClass.Groups[1].Value);
                    }

                    // Number runners
                    Match mRunners = RXSL.rxRunners.Match(s);
                    if (mRunners.Success)
                    {
                        race.Runners = Utility.RemoveWhitespace(mRunners.Groups[1].Value);
                    }

                }
            }

            PrizeList pl = GetPrizeMoney(prizeData);
            if (pl != null)
                race.Prizes = pl;

            return race;
        }
        private PrizeList GetPrizeMoney(String data)
        {
            PrizeList pl = null;
            List<string> prizes = new List<string>();
            MatchCollection mcPrizes = RXSL.rxPrizes.Matches(data);

            if (mcPrizes.Count > 0)
            {
                pl = new PrizeList();
                switch (mcPrizes[0].Groups[1].Value)
                {
                    case POUND:
                        pl.Currency = Currency.GBP;
                        break;

                    case EURO:
                        pl.Currency = Currency.EUR;
                        break;

                    case DOLLAR:
                        pl.Currency = Currency.USD;
                        break;

                    default:
                        pl.Currency = Currency.UNKNOWN;
                        break;
                }

                foreach (Match m in mcPrizes)
                {
                    string s = Regex.Replace(m.Groups[2].Value, @",", "");   // Remove ","
                    decimal d;
                    if (decimal.TryParse(s, out d)) // Is distance is just an int?
                    {
                        pl.PrizeMoney.Add(d);
                    }
                    else
                    {
                        pl.PrizeMoney.Add(0);
                    }
                }
            }

            return pl;
        }

        private IList<string> GetHorseDataAsStrings(String page)
        {
            if (String.IsNullOrEmpty(page)) throw new NullReferenceException("page == null or empty at ScrapeHorsesParallel()");

            List<IHorse> horses = null;
            horses = new List<IHorse>();
            MatchCollection mcHorseRows = RXSL.rxHorseRow.Matches(page);
            int numHorses = mcHorseRows.Count;

            IList<string> horseData = new List<string>();
            for (int i = 0; i < numHorses; i++)
            {
                horseData.Add( mcHorseRows[i].Groups[0].Value);
            }

            return horseData;
        }

        private List<IHorse> ScrapeHorsesParallel(IList<string> horseData)
        {
            if (horseData.Count < MIN_HORSES_PER_RACE)
            {
                throw new ArgumentOutOfRangeException("A Race requires at least 2 horses");
            }

            InitProgressHandler(horseData.Count);
	        ConcurrentQueue<IHorse> parallelHorses = new ConcurrentQueue<IHorse>();
            Parallel.ForEach(horseData, horseIn =>
	        {
                if (!String.IsNullOrEmpty(horseIn))
	            {
                    IHorse phorse = ScrapeHorse(horseIn);
	                parallelHorses.Enqueue(phorse);
                    ScrapeHorseCompleted(phorse.Name);
	            }
	            else
	            {
	                _ntf.Notify("No Horse data to scrape", Ntf.WARNING);
	            }

	        });

            List<IHorse> horses = new List<IHorse>();
	        IHorse horseOut;
            while (parallelHorses.TryDequeue(out horseOut))
	        {
                horses.Add(horseOut);
	        }


            return horses;
        }

        private void InitProgressHandler(int numRequired)
        {
            _currentProg = MIN_PROGRESS;
            _numReq = numRequired;
            _progInc = 100 / numRequired;
        }

        private void ScrapeHorseCompleted(string horseName)
        {
            lock(_safeLock) // This lock may not be needed as it only does a write
            {
                _currentProg += _progInc;
                _progress.Report(new BasicUpdate((int)_currentProg, String.Format("{0} : {1}", _currentRace, horseName)));
            }
        }

        private string ExtractClass(String[] items)
        {
            string raceClass = null;
            foreach (String s in items)
            {
                Match mClass = RXSL.rxClass.Match(s);
                if (mClass.Success)
                {
                    raceClass = Utility.RemoveWhitespace(mClass.Groups[1].Value);
                    break;
                }
            }
            return raceClass;
        }

        private string ExtractRunners(String[] items)
        {
            string runners = null;
            foreach (String s in items)
            {
                Match mRunners = RXSL.rxRunners.Match(s);
                if (mRunners.Success)
                {
                    runners = Utility.RemoveWhitespace(mRunners.Groups[1].Value);
                }
            }
            return runners;
        }

    }
}
