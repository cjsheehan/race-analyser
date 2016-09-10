using Racing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RacingWebScraper
{
    public partial class SLifeRacingScraper
    {
        private BackgroundWorker _safeHeaderWorker;

        private const int MAX_PROGRESS = 100;
        private const int MIN_PROGRESS = 0;
        private const int WORK_STARTED_PROGRESS = 5;
        public List<IRaceHeader> GetAvailableHeaders(DateTime dt)
        {
            List<IRaceHeader> headers = null;
            headers = new List<IRaceHeader>();
            String date = String.Format("{0:dd-MM-yyyy}", dt);
            String uri = ROOT_CARDS_URI + date;

            String meetingsPage = WebPage.Get(uri);
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
                RaceType type = RaceType.UNKNOWN;

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
                    IRaceHeader header = new RaceHeader(id, course, date, time, title, info, url, type);
                    headers.Add(header);
                }
                currentProgress += inc;
                _safeHeaderWorker.ReportProgress((int)currentProgress);
            }
            _safeHeaderWorker.ReportProgress(MAX_PROGRESS);

            return headers;
        }

        public async Task<List<IRaceHeader>> GetAvailableHeadersAsync(DateTime dt)
        {
            List<IRaceHeader> headers = null;
            headers = new List<IRaceHeader>();
            String date = String.Format("{0:dd-MM-yyyy}", dt);
            String uri = ROOT_CARDS_URI + date;

            String meetingsPage = await WebPage.GetA(uri);
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
                RaceType type = RaceType.UNKNOWN;

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
                    IRaceHeader header = new RaceHeader(id, course, date, time, title, info, url, type);
                    headers.Add(header);
                }
                currentProgress += inc;
                _safeHeaderWorker.ReportProgress((int)currentProgress);
            }
            _safeHeaderWorker.ReportProgress(MAX_PROGRESS);

            return headers;
        }

        private void bgwScrapeMeetings_DoWorkEventHandler(object sender, DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            _safeHeaderWorker = sender as BackgroundWorker;
            _safeHeaderWorker.ReportProgress(MIN_PROGRESS);
            _safeHeaderWorker.ReportProgress(WORK_STARTED_PROGRESS);
            e.Result = GetAvailableHeadersAsync((DateTime)e.Argument);
        }


    }
}
