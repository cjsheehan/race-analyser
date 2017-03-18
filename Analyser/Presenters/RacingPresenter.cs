using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Racing;
using RacingWebScraper;

namespace Betabelter
{
    public class RacingPresenter : AsyncController
    {
        private static readonly log4net.ILog log =
log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IRacingView _view;
        private INotify _ntf;
        private IRacingModel _model;
        private DateTime _dateTime;
        private IConfigPresenter _configPresenter;
        private Progress<BasicUpdate> _progress;


        public RacingPresenter(IRacingView view)
        {


            _view = view;
            _ntf = _view as INotify;
            _model = new RacingScraperModel(_ntf);
            _configPresenter = new ConfigPresenter(new ConfigView(), _model as IConfigModel);

            //set data binding
            _view.Meetings = _model.Meetings;
            _view.Races = _model.Races;

            _progress = new Progress<BasicUpdate>();
            _progress.ProgressChanged += (s, progressArgs) =>
            {
                _view.ProgressPercent = progressArgs.ProgressPercentage;
                _view.ProgressString = progressArgs.Text;
            };


            SubscribeToViewEvents(view);
            SubscribeToModelEvents(_model);
            SetUIControls(true, false, _configPresenter.WorkDirExists(), false);
        }

        private void SubscribeToViewEvents(IRacingView view)
        {
            view.GetMeetings += GetMeetingsEventHandler;
            view.GetRaceData += GetRaceDataEventHandler;
            view.DateSelected += DateSelectedEventHandler;
            view.MeetingSelected += MeetingSelectedEventHandler;
            view.DefaultRaceTypeSelected += DefaultRaceTypeSelectedEventHandler;
            view.AllRacesSelectStateChanged += AllRacesSelectedEventHandler;
            view.AnalyseRaceData += AnalyseRaceDataEventHandler;
            view.ConfigureOptions += ConfigureOptionsEventHandler;
            view.OpenWorkDir += OpenWorkDirEventHandler;
        }

        private void UnsubscribeToViewEvents(IRacingView view)
        {
            view.GetMeetings -= GetMeetingsEventHandler;
            view.GetRaceData -= GetRaceDataEventHandler;
            view.DateSelected -= DateSelectedEventHandler;
            view.MeetingSelected -= MeetingSelectedEventHandler;
            view.DefaultRaceTypeSelected -= DefaultRaceTypeSelectedEventHandler;
            view.AllRacesSelectStateChanged -= AllRacesSelectedEventHandler;
            view.AnalyseRaceData -= AnalyseRaceDataEventHandler;
            view.ConfigureOptions -= ConfigureOptionsEventHandler;
            view.OpenWorkDir -= OpenWorkDirEventHandler;
        }

        private void SubscribeToModelEvents(IRacingModel model)
        {
            model.GetMeetingsAsyncCompleted += GetMeetingsAsyncCompletedEventHandler;
            model.GetRaceDataAsyncCompleted += GetRaceDataAsyncCompletedEventHandler;
            model.AnalyseRaceDataAsyncCompleted += AnalyseRaceDataAsyncCompletedEventHandler;
        }

        private void UnsubscribeToModelEvents(IRacingModel model)
        {
            model.GetMeetingsAsyncCompleted -= GetMeetingsAsyncCompletedEventHandler;
            model.GetRaceDataAsyncCompleted -= GetRaceDataAsyncCompletedEventHandler;
            model.AnalyseRaceDataAsyncCompleted -= AnalyseRaceDataAsyncCompletedEventHandler;
        }

        #region view event handlers

        protected virtual void GetMeetingsEventHandler(object sender, EventArgs e)
        {
            log.Debug("Entered GetMeetingsEventHandler");
            log.Info("Entered GetMeetingsEventHandler");
            try
            {
                string date = _dateTime.ToShortDateString();
                _ntf.Notify(String.Format("Getting meetings for {0}...", _dateTime.ToShortDateString()), Ntf.MESSAGE);
                SetUIControls(false, false, _configPresenter.WorkDirExists(), false);
                _model.GetMeetingsAsync(_dateTime, _progress);

            }
            catch (System.Exception)
            {
                SetUIControls(true, false, _configPresenter.WorkDirExists(), false);
                throw;
            }
        }

        protected virtual void AllRacesSelectedEventHandler(object sender, EventArgs e)
        {
            try
            {
                bool selState = _view.AllSelectedState;
                _model.SetAllRacesSelected(GetSelectedMeetingId(), selState);
            }
            catch (System.Exception)
            {
                SetUIControls(true, false, _configPresenter.WorkDirExists(), false);
                throw;
            }
        }
        protected virtual void AnalyseRaceDataEventHandler(object sender, EventArgs e)
        {
            try
            {
                SetUIControls(false, false, _configPresenter.WorkDirExists(), false);
                _ntf.Notify(String.Format("Analysing race data"), Ntf.MESSAGE);
                _model.AnalyseRaceData();
            }
            catch (System.Exception)
            {
                SetUIControls(true, true, _configPresenter.WorkDirExists(), true);
                throw;
            }
        }
        protected virtual void ConfigureOptionsEventHandler(object sender, EventArgs e)
        {
            try
            {
                _configPresenter.Load();
            }
            catch (System.Exception)
            {
                SetUIControls(true, false, _configPresenter.WorkDirExists(), false);
                throw;
            }
        }
        protected virtual void OpenWorkDirEventHandler(object sender, EventArgs e)
        {
            try
            {
                _configPresenter.OpenWorkDir();
            }
            catch (System.Exception)
            {
                // TODO : fix control options
                SetUIControls(true, false, _configPresenter.WorkDirExists(), false);
                throw;
            }
        }


        protected virtual void GetRaceDataEventHandler(object sender, EventArgs e)
        {
            try
            {
                _ntf.Notify(String.Format("Getting race data..."), Ntf.MESSAGE);
                SetUIControls(false, false, _configPresenter.WorkDirExists(), false);
                _model.GetRaceDataAsync(_progress);
                SetUIControls(true, true, _configPresenter.WorkDirExists(), true);
            }
            catch (System.Exception)
            {
                SetUIControls(true, true, _configPresenter.WorkDirExists(), false);
                throw;
            }
        }

        protected virtual void DateSelectedEventHandler(object sender, EventArgs e)
        {
            _dateTime = _view.SelectedDate;
        }

        protected virtual void MeetingSelectedEventHandler(object sender, EventArgs e)
        {
            try
            {
                _model.SelectMeeting(GetSelectedMeetingId());
            }
            catch (System.Exception)
            {
                SetUIControls(true, false, _configPresenter.WorkDirExists(), false);
                throw;
            }
        }

        protected virtual void DefaultRaceTypeSelectedEventHandler(object sender, EventArgs e)
        {
            try
            {
                _model.SetRaceTypeForMeeting(GetSelectedMeetingId(), _view.DefaultType);
            }
            catch (System.Exception)
            {
                SetUIControls(true, false, _configPresenter.WorkDirExists(), false);
                throw;
            }
        }

        #endregion view event handlers

        #region model event handlers

        protected virtual void GetMeetingsAsyncCompletedEventHandler(object sender, EventArgs e)
        {
            try
            {
                // TODO: is selection of 1st meeting required?
                if (_model.Meetings != null && _model.Meetings.Count > 0)
                {
                    String id = _model.Meetings[0];
                    _model.SelectMeeting(id);
                    //peek at 1st race to init the combobox for default type
                    _view.DefaultType = _view.Races[0].Type;
                    _ntf.Notify("SUCCESS! Meetings available", Ntf.MESSAGE);
                    SetUIControls(true, true, _configPresenter.WorkDirExists(), false);
                }
                else
                {
                    _ntf.Notify(string.Format("No meetings available for {0}", _dateTime), Ntf.WARNING);
                    SetUIControls(true, false, _configPresenter.WorkDirExists(), false);
                }
            }
            catch (System.Exception)
            {
                SetUIControls(true, false, _configPresenter.WorkDirExists(), false);
                throw;
            }
        }

        protected virtual void GetRaceDataAsyncCompletedEventHandler(object sender, EventArgs e)
        {
            SetUIControls(true, true, _configPresenter.WorkDirExists(), true);
        }

        protected virtual void AnalyseRaceDataAsyncCompletedEventHandler(object sender, EventArgs e)
        {
            _ntf.Notify("SUCCESS! Race analysis is complete", Ntf.MESSAGE);
            SetUIControls(true, true, _configPresenter.WorkDirExists(), false);
        }



        #endregion // model event handlers


        protected override void OnException(Exception ex)
        {
            _ntf.Notify(ex.Message, Ntf.ERROR);
            SetUIControls(true, false, _configPresenter.WorkDirExists(), false);
        }

        private string GetSelectedMeetingId()
        {
            return _view.SelectedMeeting;
        }

        private void SetUIControls(bool meetingEnabled, bool raceEnabled, bool openWorkDirEnabled, bool analysisEnabled)
        {
            SetUIMeetingControlsEnabled(meetingEnabled);
            SetUIRaceControlsEnabled(raceEnabled);
            SetUIOpenWorkDirControlEnabled(openWorkDirEnabled);
            SetUIAnalysisControlsEnabled(analysisEnabled);
        }

        private void SetUIMeetingControlsEnabled(bool enabled)
        {
            _view.MeetingControlsEnabled = enabled;
        }

        private void SetUIRaceControlsEnabled(bool enabled)
        {
            _view.RaceControlsEnabled = enabled;
        }

        private void SetUIAnalysisControlsEnabled(bool enabled)
        {
            _view.AnalysisControlsEnabled = enabled;
        }

        private void SetUIOpenWorkDirControlEnabled(bool enabled)
        {
            _view.OpenWorkDirControlEnabled = enabled;
        }

    }
}
