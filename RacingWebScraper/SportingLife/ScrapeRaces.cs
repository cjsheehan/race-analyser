using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
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
        private static readonly log4net.ILog log =
    log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler<GetRaceDataAsyncCompletedEventArgs> GetRaceDataAsyncCompleted;
        private const string POUND = "pound";
        private const string EURO = "euro";
        private const string DOLLAR = "dollar";

        private object _safeLock = new object();

        private int _numReq = 0;  // Used to calculate the increment for progress updates
        private double _progInc = 0; // The increment used in progress updates
        private double _currentProg = 0; // The current progress in progress updates
        private string _currentRace = "";

        private const int MIN_HORSES_PER_RACE = 2;


        // race info selectors
        const String raceNameSelector = "li.hr-racecard-summary-race-name";
        const String raceDistanceSelector = "li.hr-racecard-summary-race-distance";
        const String raceGoingSelector = raceDistanceSelector;
        


        //private const String runnerSelector = "di.hr-racing-runner-key-info-container";




        protected virtual void OnGetRaceDataAsyncCompleted(List<IRaceDetail> raceData)
        {
            GetRaceDataAsyncCompletedEventArgs args = new GetRaceDataAsyncCompletedEventArgs(raceData);
            EventHandler<GetRaceDataAsyncCompletedEventArgs> handler = GetRaceDataAsyncCompleted;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        private async Task<List<Race>> ScrapeRacesAsync(List<String> raceUrls)
        {
            List<Race> races = new List<Race>();
            double currentProgress = 0;
            double inc = 100 / raceUrls.Count;
            _progress.Report(new BasicUpdate(MIN_PROGRESS, ""));
            foreach (var url in raceUrls)
            {
                var config = Configuration.Default.WithDefaultLoader();
                var document = await BrowsingContext.New(config).OpenAsync(url);
                var test = document.QuerySelectorAll("ul");
                var race = ScrapeRaceDetail(document);
                var runners = ScrapeRunnersParallel(document);
                //racesDetail.Add(raceData);
                currentProgress += inc;
            }
            _progress.Report(new BasicUpdate(MAX_PROGRESS, "Completed."));
            return races;
        }

        private Race ScrapeRaceDetail(IDocument document)
        {
            Race race = new Race();
            race.Dist = ScrapeDistance(document);
            race.Title = ScrapeRaceTitle(document);
            race.NumberOfRunners = ScrapeNumberOfRunners(document);
            const String entrantsSelector = "section.hr-racing-runner-wrapper";
            var entrantsElements = document.QuerySelectorAll(entrantsSelector);
            var entrants = new List<Entrant>();
            foreach (var element in entrantsElements)
            {
                var entrant = ScrapeEntrant(element);
                if (entrant != null)
                {
                    entrants.Add(entrant);
                }
                else
                {
                    log.Error("Failed to scrape entrant from:" + document.Url);
                }
            }

            return race;
        }

        private String ScrapeRaceTitle(IDocument document)
        {
            const String selector = "li.hr-racecard-summary-race-name";
            return ScrapeTextContent(document, selector);
        }

        private String ScrapeDistance(IDocument document)
        {
            var selector = "li.hr-racecard-summary-race-distance";
            var textContent = ScrapeTextContent(document, selector);
            return textContent = Regex.Replace(textContent, ", .*", "");
        }

        private String ScrapeGoing(IDocument document)
        {
            // TODO : Going is required from meeting page if not scraped on the same day as the race as data
            // is missing from SL racecard if too early
            var selector = "li.hr-racecard-summary-race-distance";
            return ScrapeTextContent(document, selector);
        }

        private int ScrapeNumberOfRunners(IDocument document)
        {
            const String selector = "li.hr-racecard-summary-race-runners";
            var textContent = ScrapeTextContent(document, selector);
            textContent = Regex.Match(textContent, @"\d+").Value;
            

            int numRunners;
            bool res = int.TryParse(textContent, out numRunners);
            if (res == false)
            {
                return -1;
            }
            else
            {
                return numRunners;
            }
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

        private String ScrapeRunnerName(AngleSharp.Dom.IElement element)
        {
            const String selector = "span.hr-racing-runner-horse-name > a";
            return element.QuerySelector(selector).TextContent;
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
                horseData.Add(mcHorseRows[i].Groups[0].Value);
            }

            return horseData;
        }

        private List<String> ScrapeRunnersParallel(IDocument document)
        {
            const String runnerSelector = "div.hr-racing-runner-key-info-container";
            var runners = document.QuerySelectorAll(runnerSelector);

            if (runners == null | runners.Length == 0)
            {
                String msg = "no runners found";
                log.Error(msg);
                throw new InvalidScrapeException(msg);
            }

            InitProgressHandler(runners.Length);
            ConcurrentQueue<String> parallelHorses = new ConcurrentQueue<String>();
            Parallel.ForEach(runners, currentRunner =>
            {
                if (currentRunner != null)
                {
                    String horseName = ScrapeRunnerName(currentRunner);
                    parallelHorses.Enqueue(horseName);
                    ScrapeHorseCompleted(horseName);
                }
                else
                {
                    _ntf.Notify("No Horse data to scrape", Ntf.WARNING);
                }

            });

            List<String> horses = new List<String>();
            String horseOut;
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
            lock (_safeLock) // This lock may not be needed as it only does a write
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
