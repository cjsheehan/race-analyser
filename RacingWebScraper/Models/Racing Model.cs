using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Racing;
using System.ComponentModel;
using System.Windows;
using RacingWebScraper.Properties;
using System.Threading.Tasks;

namespace RacingWebScraper
{
    public class RacingScraperModel : IRacingModel, IConfigModel, INotifyPropertyChanged
    {
        private IRacingScraper _scraper;
        private BindingList<String> _boundMeetings;
        private BindingList<IRaceHeader> _boundRaces;
        private INotify _ntf;
        private List<Race> _scrapedRaces;
        private List<IRaceHeader> _raceHeaders;
        private int _currentTaskProgress;

        public RacingScraperModel(INotify ntf)
        {
            CurrentTaskProgress = 0;
            _scraper = new SLifeRacingScraper(ntf);
            _boundRaces = new BindingList<IRaceHeader>();
            _boundMeetings = new BindingList<String>();
            _scrapedRaces = new List<Race>();
            _ntf = ntf;
            _scraper.GetRaceDataAsyncCompleted += GetRaceDataCompletedEventHandler;
        }

        public string WorkDir 
        {
            get
            {
                return Settings.Default.WorkDir;
            }
            set
            {
                Settings.Default.WorkDir = value;
                Properties.Settings.Default.Save();
            }
        }

#region IRacingModel impl

        public event EventHandler<EventArgs> GetMeetingsAsyncCompleted;
        public event EventHandler<EventArgs> GetRaceDataAsyncCompleted;
        public event EventHandler<EventArgs> AnalyseRaceDataAsyncCompleted;

        public async Task GetMeetingsAsync(DateTime dt, IProgress<BasicUpdate> progress)
        {
            _raceHeaders = await _scraper.GetHeadersAsync(dt, progress);
            IEnumerable<String> meetings = GetUniqueCourses(_raceHeaders);
            UpdateBoundMeetings(meetings.ToList());
            OnGetMeetingsAsyncCompleted(); // TODO : Remove "Completed" events as using async await
        }

        protected virtual void OnGetMeetingsAsyncCompleted()
        {
            EventHandler<EventArgs> handler = GetMeetingsAsyncCompleted;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected virtual void GetRaceDataCompletedEventHandler(object sender, GetRaceDataAsyncCompletedEventArgs e)
        {
            if (e.RaceData != null)
            {
                _scrapedRaces.Clear();
                foreach (var detail in e.RaceData)
                {
                    IRaceHeader header = GetRaceHeader(detail.Url);
                    Race race = null;
                    if (header != null)
                    {
                        RacingFactory.CreateRace(header, detail, ref race);
                    }

                    if (race != null)
                    {
                        _scrapedRaces.Add(race);
                    }
                }
            }
            OnGetRaceDataAsyncCompleted();
        }



        private IRaceHeader GetRaceHeader(string url)
        {
            return (_raceHeaders.SingleOrDefault(x => x.Url == url));
        }

        protected virtual void OnGetRaceDataAsyncCompleted()
        {
            EventHandler<EventArgs> handler = GetRaceDataAsyncCompleted;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }


        public void UpdateBoundMeetings(List<String> source)
        {
            Meetings.Clear();
            foreach (var v in source) 
                Meetings.Add(v);
        }

        public void UpdateBoundRaces(String meetingId)
        {
            Races.Clear();
            foreach (var race in _raceHeaders)
            {
                if (race.Course == meetingId)
                    Races.Add(race);
            }
        }


        public async Task GetRaceDataAsync(IProgress<BasicUpdate> progress)
        {
            CurrentTaskProgress = 0; // TODO : Remove old progress impl
            List<Dictionary<String, String>> allRacesData = new List<Dictionary<string, string>>();
            //race.Course = raceData["course"];
            //race.Class = raceData["class"];
            //race.Time = raceData["time"];
            //race.Url = raceData["url"];
            foreach (IRaceHeader race in _raceHeaders)
            {
                if (race.Selected == true)
                {
                    var raceData = new Dictionary<string,string>();
                    raceData.Add("url", race.Url);
                    raceData.Add("going", race.Going);
                    raceData.Add("course", race.Course);
                    raceData.Add("category", race.Category);
                    raceData.Add("time", race.Time);
                    raceData.Add("date", race.Date);
                    allRacesData.Add(raceData);
                }
            }
            if (allRacesData.Count > 0)
            {
                var races = await _scraper.GetRaceDataAsync(allRacesData, progress).ConfigureAwait(false);
                _scrapedRaces.Clear();
                foreach (var race in races)
                {
                    //IRaceHeader header = GetRaceHeader(detail.Url);
                    //IRace race = null;
                    //if (header != null)
                    //{
                    //    RacingFactory.CreateRace(header, detail, ref race);
                    //}

                    if (race != null)
                    {
                        _scrapedRaces.Add(race);
                    }
                }
                _ntf.Notify("SUCCESS! Race data is now available", Ntf.MESSAGE);
            }
            else
            {
                _ntf.Notify("FAILED! No races selected for data extraction", Ntf.WARNING);
            }

            OnGetRaceDataAsyncCompleted();
        }

        public void SetRaceTypeForMeeting(string meetingId, RaceType raceType)
        {
            foreach (IRaceHeader race in _raceHeaders)
            {
                if(race.Course == meetingId)
                    race.Type = raceType;
            }
        }
        
        public void SetRaceTypeForRace(string meetingId, int raceId, RaceType raceType)
        {
            foreach (IRaceHeader race in _raceHeaders)
            {
                if (race.Course == meetingId && race.Id == raceId)
                    race.Type = raceType;
            }
        }

        public void SetAllRacesSelected(string meetingId, bool selected)
        {
            foreach (IRaceHeader race in _raceHeaders)
            {
                if(race.Course == meetingId)
                    race.Selected = selected;
            }
        }
        public void SetRaceSelected(string meetingId, int raceId, bool selected)
        {
            foreach (var race in _raceHeaders)
            {
                if (race.Course == meetingId && race.Id == raceId)
                    race.Selected = selected;
            }

        }

        public void SelectMeeting(string meetingId)
        {
            if (_boundMeetings != null)
            {
                UpdateBoundRaces(meetingId);
            }
            else
            {
                throw new NullReferenceException("No meetings available for selection");
            }
        }

        public async Task AnalyseRaceData()
        {
            PerlAnalyser pa = new PerlAnalyser(_ntf) { WorkDir = this.WorkDir };
            await pa.Analyse(_scrapedRaces);
            OnAnalyseRaceDataAsyncCompleted(this, EventArgs.Empty);
        }

        protected virtual void OnAnalyseRaceDataAsyncCompleted(object sender, EventArgs e)
        {
            EventHandler<EventArgs> handler = AnalyseRaceDataAsyncCompleted;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public BindingList<String> Meetings 
        { 
            get
            {
                return _boundMeetings;
            }
            private set
            {
                if (value != null)
                {
                    _boundMeetings = value;
                    NotifyPropertyChanged("Meetings");
                }
            }
        }

        public BindingList<IRaceHeader> Races
        {
            get
            {
                return _boundRaces;
            }
            private set 
            {
                if(_boundRaces == null)
                {


                }
                if (value != null)
                {
                    _boundRaces = value;
                    _boundRaces.Clear();
                    foreach (var item in value)
                    {
                        Races.Add(item);
                    }
                    NotifyPropertyChanged("Races");
                }
            }
        }

        public int CurrentTaskProgress
        {
            get
            {
                return _currentTaskProgress;
            }

            private set
            {
                _currentTaskProgress = value;
                NotifyPropertyChanged("CurrentTaskProgress");
            }
        }

#endregion // IRacingModel


#region INotifyPropertyChanged impl

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property. 
        // The CallerMemberName attribute that is applied to the optional propertyName 
        // parameter causes the property name of the caller to be substituted as an argument. 
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


#endregion // INotifyPropertyChanged impl


        private IList<IRaceHeader> GetCardsForMeeting(string course)
        {
            List<IRaceHeader> cards = new List<IRaceHeader>();
            foreach (var card in _raceHeaders)
            {
                if (card.Course == course)
                {
                    cards.Add(card);
                }
            }
            return cards;
        }

        List<IRaceHeader> SelectMeeting(string course, IEnumerable<IRaceHeader> headers)
        {
            List<IRaceHeader> cards = new List<IRaceHeader>();    
            foreach (var card in headers)
            {
                if (card.Course == course)
                {
                    cards.Add(card);
                }
            }
            return cards;
        }

        private IRaceHeader GetRaceCard(int raceId)
        {
            return (Races.SingleOrDefault(x => x.Id == raceId));
        }



        private IEnumerable<String> GetUniqueCourses(IList<IRaceHeader> headers)
        {
            return headers.Select(x => x.Course).Distinct();
        }

    }// class RacingWebScraper


} 
