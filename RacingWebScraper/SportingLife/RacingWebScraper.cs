using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Racing;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Threading.Tasks;

namespace RacingWebScraper
{
    public partial class SLifeRacingScraper : IRacingScraper
    {
        public List<IRaceHeader> Headers 
        {
            get
            {
                return _headers;
            }
        }

        private const int MAX_PROGRESS = 100;
        private const int MIN_PROGRESS = 0;
        private const int WORK_STARTED_PROGRESS = 5;

        private INotify _ntf;
        private bool _isRunning = false; // Allow only a single long running asyn operation at a time

        private const String SITE_PREFIX = "http://www.sportinglife.com";
        private const String HURDLE = "Hurdle";
        private const String CHASE = "Chase";
        private String ROOT_CARDS_URI = @"http://www.sportinglife.com/racing/racecards/";
        private List<IRaceHeader> _headers = null;
        private IProgress<BasicUpdate> _progress;
        public SLifeRacingScraper(INotify ntf)
        {
            if(ntf == null) throw new ArgumentNullException("INotify is null in SLifeRacingScraper constructor");
            _ntf = ntf;
        }


        public async Task<List<IRaceHeader>> GetHeadersAsync(DateTime dt, IProgress<BasicUpdate> progress)
        {
            if (progress == null) throw new ArgumentNullException("progress can't be null");

            if (_isRunning)
            {
                _ntf.Notify("Async Task already running e.g. GetMeetings/GetRaces.  Please wait for that task to finish and try again", Ntf.ERROR);
                return null;
            }

            List<IRaceHeader> availHeaders = null;
            try
            {
                _isRunning = true;
                availHeaders = await Task.Run<List<IRaceHeader>>(async () =>
	            {
	                progress.Report(new BasicUpdate(MIN_PROGRESS, ""));
	                progress.Report(new BasicUpdate(WORK_STARTED_PROGRESS, "Getting Meetings..."));
	
	                List<IRaceHeader> headers = null;
	                headers = new List<IRaceHeader>();
	                String date = String.Format("{0:dd-MM-yyyy}", dt);
	                String uri = ROOT_CARDS_URI + date;
	
	                String meetingsPage = await WebPage.GetAsync(uri);
	                meetingsPage = PreFormatPage(meetingsPage);
	
	                MatchCollection mcRaceBoundary = RXSL.rxRaceStubBoundary.Matches(meetingsPage);
	                double currentProgress = 0;
	                double inc = 100 / mcRaceBoundary.Count;
	                foreach (Match raceBoundary in mcRaceBoundary)
	                {
	                    int id = 0;
	                    string course = null;
	                    string time = null;
	                    string title = null;
	                    string info = null;
	                    string url = null;
	                    
	
	                    Match m = RXSL.rxRaceStub.Match(raceBoundary.Groups[1].Value);
	                    if (m.Success)
	                    {
	                        title = m.Groups[0].Value;
	                        time = m.Groups[1].Value;
	                        url = SITE_PREFIX + m.Groups[2].Value;
	                        //raceCard.Date = m.Groups[3].Value;
	                        course = m.Groups[4].Value;
	                        id = Convert.ToInt32(m.Groups[5].Value);
	                        info = m.Groups[6].Value;

                            RaceType type;
                            if (info.Contains(HURDLE))
                                type = RaceType.HURDLES;
                            else if (info.Contains(CHASE))
                                type = RaceType.FENCES;
                            else
                                type = RaceType.FLAT;

	                        IRaceHeader header = new RaceHeader(id, course, date, time, title, info, url, type);
	                        headers.Add(header);
	                    }
	                    currentProgress += inc;
	                    progress.Report(new BasicUpdate((int)currentProgress, String.Format("Getting Meeting : {0}", course)));
	                }
	                progress.Report(new BasicUpdate(MAX_PROGRESS, "Completed."));
	                return headers;
	            });
            }
            catch (System.Exception)
            {
                _isRunning = false;
                throw;
            }

            _isRunning = false;
            return availHeaders;
        }

        public async Task<List<IRaceDetail>> GetRaceDataAsync(List<String> urls, IProgress<BasicUpdate> progress)
        {
            if (urls == null) throw new ArgumentNullException("urls cannot be null");
            if (progress == null) throw new ArgumentNullException("progress cannot be null");

            _progress = progress;

            if (_isRunning)
            {
                _ntf.Notify("Async Task already running e.g. GetMeetings/GetRaces.  Please wait for that task to finish and try again", Ntf.ERROR);
                return null;
            }

            List<IRaceDetail> races = null;
            _isRunning = true;

            try
            {
                races = await Task.Run<List<IRaceDetail>>(async () =>
                {
                    List<IRaceDetail> racesScraped = await ScrapeRacesAsync(urls);
                    return racesScraped;
                });
            }
            catch (System.Exception)
            {
                _isRunning = false;
                throw;
            }

            _isRunning = false;
            return races;
        }

        private void InitAsyncTask(IProgress<BasicUpdate> progress)
        {
            _progress = progress;
            _progress.Report(new BasicUpdate(MIN_PROGRESS, ""));
        }

        private void ProcessedHorse(String name)
        {
            
        }

        private String PreFormatPage(String page)
        {
            if (String.IsNullOrEmpty(page)) throw new System.ArgumentNullException("String page cannot be null or empty");
            page = Regex.Replace(page, @"\s+", " ", RegexOptions.Multiline);
            return page;
        }

    }
}
