using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Racing
{
    public interface IRacingModel
    {
        event EventHandler<EventArgs> GetMeetingsAsyncCompleted;
        event EventHandler<EventArgs> GetRaceDataAsyncCompleted;
        event EventHandler<EventArgs> AnalyseRaceDataAsyncCompleted;
        Task GetMeetingsAsync(DateTime dt, IProgress<BasicUpdate> progress);
        Task GetRaceDataAsync(IProgress<BasicUpdate> progress);
        void SelectMeeting(string meetingId);
        void SetRaceTypeForMeeting(string meetingId, RaceType raceType);
        void SetRaceTypeForRace(string meetingId, int raceId, RaceType raceType);
        void SetRaceSelected(string meetingId, int raceId, bool selected);
        void SetAllRacesSelected(string meetingId, bool selected);
        Task AnalyseRaceData();

        BindingList<String> Meetings { get; }
        BindingList<IRaceHeader> Races { get; }
    }

    public class GetRaceDataAsyncCompletedEventArgs : EventArgs
    {
        public List<IRaceDetail> RaceData { get; private set; }

        public GetRaceDataAsyncCompletedEventArgs(List<IRaceDetail> raceData)
        {
            RaceData = raceData;
        }
    }
}
