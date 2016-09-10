using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Racing
{
    public interface IRacingView : INotify
    {
        event EventHandler<EventArgs> GetMeetings;
        event EventHandler<EventArgs> GetRaceCards;
        event EventHandler<EventArgs> GetRaceData;
        event EventHandler<EventArgs> DateSelected;
        event EventHandler<EventArgs> MeetingSelected;
        event EventHandler<EventArgs> AllRacesSelectStateChanged;
        event EventHandler<EventArgs> DefaultRaceTypeSelected;
        event EventHandler<EventArgs> AnalyseRaceData;
        event EventHandler<EventArgs> ConfigureOptions;
        event EventHandler<EventArgs> OpenWorkDir;

        DateTime SelectedDate { get; }
        BindingList<String> Meetings { get; set; }
        String SelectedMeeting { get; }
        BindingList<IRaceHeader> Races { get; set; }
        RaceType DefaultType { get; set; }
        bool AllSelectedState { get; }
        bool MeetingControlsEnabled { set; }
        bool RaceControlsEnabled { set; }
        bool AnalysisControlsEnabled { set; }
        bool OpenWorkDirControlEnabled { set; }

        int ProgressPercent { set; }
        String ProgressString { set; }
    }



}
