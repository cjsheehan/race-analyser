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
using AngleSharp.Dom.Html;
using AngleSharp.Dom;
using Racing.Classes;
using Newtonsoft.Json;

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

        private const String SITE_PREFIX = "https://www.sportinglife.com";
        private const String HURDLE = "Hurdle";
        private const String CHASE = "Chase";
        private String ROOT_CARDS_URI = @"https://www.sportinglife.com/racing/racecards";
        private List<IRaceHeader> _headers = null;
        private IProgress<BasicUpdate> _progress;


        // private const String meetingSelector = "ul.meetings > li";
        private const String meetingSelector = "ul.meetings div.hr-meeting-section";
        private const String meetingTabSelector = "span.generic-tabs-nav-item";
        private const String ukFilterSelector = "span.hr-racing-racecards-filter.filter-uk";

        private const String raceSelector = "ul.hr-meeting-races-container > li";
        private const String goingSelector = "span.hr-meeting-meta-value";
        private const String timeSelector = "span.hr-meeting-race-time";
        // private const String racenameSelector = "div.hr-meeting-race-name-star";
        private const String racenameSelector = "span.hr-meeting-race-name";
        private const String raceinfoSelector = racenameSelector + "> span";
        private const String raceurlSelector = "li > a";
        private const String courseSelector = "div.dividerRow > h2";


        public SLifeRacingScraper(INotify ntf)
        {
            if (ntf == null) throw new ArgumentNullException("INotify is null in SLifeRacingScraper constructor");
            _ntf = ntf;
        }

        private async Task<List<Meeting>> GetMeetings(string url)
        {
            var selector = "script#__NEXT_DATA__";
            var doc = await HtmlService.GetDocumentAsync("https://www.sportinglife.com/racing/racecards/tomorrow");
            var json = doc.QuerySelector(selector).TextContent;
            List<Meeting> meetings = new List<Meeting>();
            try
            {
                meetings = JsonConvert.DeserializeObject<RootObject>(json).Props.pageProps.meetings;
            }
            catch (JsonSerializationException e)
            {
                throw;
            } 
            return meetings;
        }
        private string GenRacecardUrl(Racing.Classes.Race race)
        {
            string name = Regex.Replace(race.name, @"\s+", "-").ToLower();
            name = Regex.Replace(name, "[^a-z0-9-]", "");
            string url = String.Format("{0}/{1}/{2}/racecard/{3}/{4}"
                , ROOT_CARDS_URI, race.date, race.course_name.ToLower(), race.raceSummaryReference.id, name);
            return url;
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
                    String uri = ROOT_CARDS_URI + "/" + date;
                    var meetings = await GetMeetings(uri);

                    double currentProgress = 0;
                    double inc = 0;
                    if (meetings.Count > 0)
                    {
                        inc = 100 / meetings.Count();
                    }

                    foreach (var meeting in meetings)
                    {
                        int id = 0;
                        var course = meeting.meetingSummary.course.name;
                        var going = meeting.meetingSummary.going;
                        var races = meeting.races;

                        foreach (var race in races)
                        {
                            RaceType type;
                            if (race.name.ToUpper().Contains("HURDLE"))
                                type = RaceType.HURDLES;
                            else if (race.name.ToUpper().Contains("CHASE"))
                                type = RaceType.FENCES;
                            else
                                type = RaceType.FLAT;

                            race.url = GenRacecardUrl(race);
                            IRaceHeader header = new RaceHeader(
                                id, course, going, race.raceClass,
                                race.date, race.time, race.name,
                                String.Format("{0}, {1} Runners, Class {2}, {3}", race.name, race.rideCount, race.raceClass, race.distance),
                                race.url, type);
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

        private String ScrapeCategory(AngleSharp.Dom.IElement element)
        {
            const String selector = "div.hr-meeting-race-name-star > span";
            const String rx = "Class (\\d)";
            return ScrapeStringFromTextContent(element, selector, rx);
        }

        public async Task<List<Racing.Race>> GetRaceDataAsync(List<Dictionary<String, String>> raceData, IProgress<BasicUpdate> progress)
        {
            if (raceData == null) throw new ArgumentNullException("raceData is be null");
            if (progress == null) throw new ArgumentNullException("progress is be null");

            _progress = progress;

            if (_isRunning)
            {
                _ntf.Notify("Async Task already running e.g. GetMeetings/GetRaces.  Please wait for that task to finish and try again", Ntf.ERROR);
                return null;
            }

            List<Racing.Race> races = null;
            _isRunning = true;

            try
            {
                races = await ScrapeRacesAsync(raceData).ConfigureAwait(false);
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
