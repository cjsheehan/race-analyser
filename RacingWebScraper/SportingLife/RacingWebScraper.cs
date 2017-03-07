using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Racing;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Parser.Html;

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


        private const String meetingSelector = "ul.meetings > li";
        private const String raceSelector = "ul.hr-meeting-races-container > li";
        private const String goingSelector = "span.hr-meeting-meta-value";
        private const String timeSelector = "span.hr-meeting-race-time";
        private const String racenameSelector = "div.hr-meeting-race-name-star";
        private const String raceinfoSelector = racenameSelector + "> span";
        private const String raceurlSelector = "li > a";
        private const String courseSelector = "div.dividerRow > h2";


        public SLifeRacingScraper(INotify ntf)
        {
            if (ntf == null) throw new ArgumentNullException("INotify is null in SLifeRacingScraper constructor");
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
                    String date = String.Format("{0:yyyy-MM-dd}", dt);
                    String uri = ROOT_CARDS_URI + date;




                    //var config = Configuration.Default.WithDefaultLoader();
                    //var document = await BrowsingContext.New(config).OpenAsync(uri);
                    var document = await WebPage.GetDocumentAsync(uri).ConfigureAwait(false);
                    var meetings = document.QuerySelectorAll(meetingSelector);

                    double currentProgress = 0;
                    double inc = 0;
                    if (meetings.Length > 0)
                    {
                        inc = 100 / meetings.Length;
                    }

                    foreach (var meeting in meetings)
                    {
                        int id = 0;
                        var course = meeting.QuerySelector(courseSelector).TextContent; // div.dividerRow > h2
                        var going = meeting.QuerySelector(goingSelector).TextContent;
                        var races = meeting.QuerySelectorAll(raceSelector);

                        foreach (var race in races)
                        {
                            String rxRacename = "\\s*\\(.*\\)";
                            var title = race.QuerySelector(racenameSelector).TextContent;
                            title = Regex.Replace(title, rxRacename, "");
                            var time = race.QuerySelector(timeSelector).TextContent;
                            var info = race.QuerySelector(racenameSelector).TextContent;
                            var url = SITE_PREFIX + race.QuerySelector(raceurlSelector).GetAttribute("href");


                            RaceType type;
                            if (info.ToUpper().Contains("HURDLE"))
                                type = RaceType.HURDLES;
                            else if (info.ToUpper().Contains("CHASE"))
                                type = RaceType.FENCES;
                            else
                                type = RaceType.FLAT;

                            IRaceHeader header = new RaceHeader(id, course, going, date, time, title, info, url, type);
                            headers.Add(header);

                        }
                        currentProgress += inc;
                        progress.Report(new BasicUpdate((int)currentProgress, string.Format("getting meeting : {0}", course)));
                    }

                    progress.Report(new BasicUpdate(MAX_PROGRESS, "completed."));
                    return headers;
                }).ConfigureAwait(false);
            }
            catch (System.Exception)
            {
                _isRunning = false;
                throw;
            }

            _isRunning = false;
            return availHeaders;
        }

        public async Task<List<Race>> GetRaceDataAsync(List<Dictionary<String, String>> raceData, IProgress<BasicUpdate> progress)
        {
            if (raceData == null) throw new ArgumentNullException("raceData is be null");
            if (progress == null) throw new ArgumentNullException("progress is be null");

            _progress = progress;

            if (_isRunning)
            {
                _ntf.Notify("Async Task already running e.g. GetMeetings/GetRaces.  Please wait for that task to finish and try again", Ntf.ERROR);
                return null;
            }

            List<Race> races = null;
            _isRunning = true;

            try
            {
                races = await ScrapeRacesAsync(raceData).ConfigureAwait(false);
                //races = await Task.Run<List<Race>>(() =>
                //{
                //    var racesScraped = await ScrapeRacesAsync(raceData).ConfigureAwait(false);
                //    return racesScraped;
                //}).ConfigureAwait(false);
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
