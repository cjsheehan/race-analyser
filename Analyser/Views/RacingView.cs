using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Racing;
using RacingWebScraper;

namespace Betabelter
{
    public partial class RacingForm : View, IRacingView
    {
        private RacingPresenter _presenter;
        private BindingList<String> _meetings;
        private BindingList<IRaceHeader> _races;
        private RaceType _defaultRaceType;
        private String _selectedMeeting;
        private DateTime _selectedDate;
        private bool _allSelectedState = false;
        private BindingSource _bindRaces;
        private BindingSource _bindMeetings;
        private bool _racesBound = false;
        private bool _meetingsBound = false;
        private const int MAX_PROGRESS = 100;
        private const int MIN_PROGRESS = 0;
        private const string PROCESSING = "Processing : ";

        private const String GET_RACE_DATA_HEADER = "Selection";
        private const int GET_DATA_COLUMN_IDX = 0;

        public RacingForm()
        {
            InitializeComponent();
            _bindRaces = new BindingSource();
            _bindMeetings = new BindingSource();
            _presenter = new RacingPresenter(this);
            dateTimePicker1.MinDate = DateTime.Today;
            labelProgress.Text = "";
            OnDateSelected(); // Ensure that current datetime is passed to presenter
        }

        #region IRacingView Members

        public event EventHandler<EventArgs> GetMeetings;
        public event EventHandler<EventArgs> GetRaceCards;
        public event EventHandler<EventArgs> GetRaceData;
        public event EventHandler<EventArgs> DateSelected;
        public event EventHandler<EventArgs> MeetingSelected;
        public event EventHandler<EventArgs> AllRacesSelectStateChanged;
        public event EventHandler<EventArgs> AnalyseRaceData;
        public event EventHandler<EventArgs> ConfigureOptions;
        public event EventHandler<EventArgs> OpenWorkDir;
        public BindingList<String> Meetings
        {
            get
            {
                return _meetings;
            }
            set
            {
                if (!_meetingsBound)
                {
                    MethodInvoker uiDelegate = delegate
                    {
                        InitRaceCardViewControl();
                        _meetings = value;
                        _bindMeetings.DataSource = _meetings;
                        listBoxMeetings.DataSource = _bindMeetings;
                        listBoxMeetings.DisplayMember = "Course";
                    };
                    UpdateUI(uiDelegate);
                    _meetingsBound = true;
                }
            }
        }

        public event EventHandler<EventArgs> DefaultRaceTypeSelected;

        public String SelectedMeeting
        { 
            get
            {
                if (Meetings.Count > 0)
                {

                    return this.listBoxMeetings.SelectedItem as String;
                }
                else
                {
                    return null;
                }
            }
        }

        public DateTime SelectedDate
        {
            get{ return _selectedDate; }
            private set { _selectedDate = value; }
        }

        public BindingList<IRaceHeader> Races
        {
            get
            {
                return _races;
            }
            set
            {
                if (!_racesBound)
                {
                    MethodInvoker uiDelegate = delegate
                    {
                        _races = value;
                        _bindRaces.DataSource = value;
                        dataGridViewRaceCards.DataSource = _bindRaces;
                    };

                    UpdateUI(uiDelegate);
                    _racesBound = true;
                }
            }
        }

        public RaceType DefaultType
        {
            get
            {
                return _defaultRaceType;
            }
            set
            {
                MethodInvoker uiDelegate = delegate
                {
                    comboBoxDefaultRaceType.SelectedItem = value;
                    _defaultRaceType = value;
                };
                UpdateUI(uiDelegate);
            }
        }

        public bool AllSelectedState 
        {
            get
            {
                return _allSelectedState;
            }
            private set
            {
                _allSelectedState = value;
            }
        }

        public bool MeetingControlsEnabled
        {
            set
            {
                groupBoxMeetings.Enabled = value;
            }
        }

        public bool RaceControlsEnabled
        {
            set
            {
                groupBoxCards.Enabled = value;
            }
        }

        public bool OpenWorkDirControlEnabled
        {
            set
            {
                buttonOpenWorkDir.Enabled = value; 
            }
        }

        public bool AnalysisControlsEnabled
        {
            set
            {
                buttonAnalyse.Enabled = value;
            }
        }

        public int ProgressPercent
        {
            set
            {
                MethodInvoker uiDelegate = delegate
                {
                    progressBar.Value = value;
                };
                UpdateUI(uiDelegate);
            }
        }
        public String ProgressString
        {
            set
            {
                MethodInvoker uiDelegate = delegate
                {
                    DisplayProgress(value);
                };
                UpdateUI(uiDelegate);
            }
        }

        #endregion // IRacingView Members

        public void Notify(String ntf, Ntf type)
        {
            MethodInvoker uiDelegate = delegate
            {
                string s;
                switch (type)
                {
                    case Ntf.MESSAGE:
                        s = "I : ";
                        break;
                    case Ntf.WARNING:
                        s = "W : ";
                        break;
                    case Ntf.ERROR:
                        s = "E :  ";
                        break;
                    default:
                        s = "I :  ";
                        break;
                }

                textBoxOutput.AppendText(string.Format("{0}{1}{2}", s,  ntf, System.Environment.NewLine));
            };
            UpdateUI(uiDelegate);
        }

        

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            OnDateSelected();
        }

        protected void OnDateSelected()
        {
            _selectedDate = new DateTime(this.dateTimePicker1.Value.Year, this.dateTimePicker1.Value.Month, this.dateTimePicker1.Value.Day);
            EventHandler<EventArgs> handler = DateSelected;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected void OnMeetingSelected()
        {
            _selectedMeeting = listBoxMeetings.SelectedItem as String;
            EventHandler<EventArgs> handler = MeetingSelected;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected void OnGetRaceData()
        {
            EventHandler<EventArgs> handler = GetRaceData;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        protected void OnDefaultRaceTypeSelected(RaceType type)
        {
            EventHandler<EventArgs> handler = DefaultRaceTypeSelected;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void buttonGetRaceCards_Click(object sender, EventArgs e)
        {
            EventHandler<EventArgs> handler = GetRaceCards;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void buttonGetRaceData_Click(object sender, EventArgs e)
        {
            OnGetRaceData();
        }

        private void listBoxMeetings_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnMeetingSelected();
        }

        private void InitRaceCardViewControl()
        {
            MethodInvoker uiDelegate = delegate
            {
                //dataGridViewRaceCards.RowHeadersVisible = false;
                dataGridViewRaceCards.AutoGenerateColumns = false;
                //dataGridViewRaceCards.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.None);


                // Check box indicating required data
                DataGridViewColumn column = CreateCheckBoxColumn();
                column.Name = "Select";
                column.DataPropertyName = "Selected";
                dataGridViewRaceCards.Columns.Add(column);


                // Add Time
                column = new DataGridViewTextBoxColumn();
                column.DataPropertyName = "Time";
                column.Name = "Time";
                dataGridViewRaceCards.Columns.Add(column);

                // Add Type
                column = CreateComboBoxWithEnums();
                column.DataPropertyName = "Type";
                column.Name = "Type";
                dataGridViewRaceCards.Columns.Add(column);

                // Add Info
                column = new DataGridViewTextBoxColumn();
                column.DataPropertyName = "Info";
                column.Name = "Info";
                dataGridViewRaceCards.Columns.Add(column);

                // Combo box to apply a value top all column entries
                comboBoxDefaultRaceType.DataSource = Enum.GetValues(typeof(RaceType));
                comboBoxDefaultRaceType.Name = "Default Race Type";
                comboBoxDefaultRaceType.SelectedIndex = 0;
            };

            UpdateUI(uiDelegate);
        }

        private DataGridViewComboBoxColumn CreateComboBoxWithEnums()
        {
            DataGridViewComboBoxColumn combo = new DataGridViewComboBoxColumn();
            combo.DataSource = Enum.GetValues(typeof(RaceType));
            combo.DataPropertyName = "Race Type";
            combo.Name = "Type";
            combo.FlatStyle = FlatStyle.Standard;
            combo.Width = 150;
            return combo;
        }

        private DataGridViewCheckBoxColumn CreateCheckBoxColumn()
        {
            DataGridViewCheckBoxColumn column = new DataGridViewCheckBoxColumn();
            column.HeaderText = GET_RACE_DATA_HEADER;
            column.CellTemplate = new DataGridViewCheckBoxCell();
            column.CellTemplate.Style.BackColor = Color.Beige;           
            return column;
        }


        private void AddGetRaceDataColumn()
        {
            DataGridViewCheckBoxColumn column = new DataGridViewCheckBoxColumn();
            {
                column.HeaderText = GET_RACE_DATA_HEADER;
                //column.Name = "Select";
                column.FlatStyle = FlatStyle.Standard;
                column.ThreeState = false;
                column.CellTemplate = new DataGridViewCheckBoxCell();
                column.CellTemplate.Style.BackColor = Color.Beige;
            }

            dataGridViewRaceCards.Columns.Insert(GET_DATA_COLUMN_IDX, column);
        }

        private void buttonGetMeetings_Click(object sender, EventArgs e)
        {
            EventHandler<EventArgs> handler = GetMeetings;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void comboBoxDefaultRaceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            DefaultType = (RaceType)comboBoxDefaultRaceType.SelectedItem;
            OnDefaultRaceTypeSelected((RaceType)comboBoxDefaultRaceType.SelectedItem);
        }

        private void checkBoxSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
	            AllSelectedState = checkBoxSelectAll.Checked;
	            OnAllRacesSelectStateChanged();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        protected void OnAllRacesSelectStateChanged()
        {
            EventHandler<EventArgs> handler = AllRacesSelectStateChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected void OnAnalyseData()
        {
            EventHandler<EventArgs> handler = AnalyseRaceData;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected void OnConfigureOptions()
        {
            EventHandler<EventArgs> handler = ConfigureOptions;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected void OnOpenWorkDir()
        {
            EventHandler<EventArgs> handler = OpenWorkDir;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void buttonAnalyse_Click(object sender, EventArgs e)
        {
            OnAnalyseData();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnConfigureOptions();
        }

        private void buttonOpenWorkDir_Click(object sender, EventArgs e)
        {
            OnOpenWorkDir();
        }

        private void DisplayProgress(String value)
        {
            if (String.IsNullOrEmpty(value))
            {
                labelProgress.Text = "";
            }
            else
            {
                labelProgress.Text = PROCESSING + value;
            }
        }



    } // class RacingForm

    public class View : Form
    {
        protected void UpdateUI(MethodInvoker uiDelegate)
        {
            if (InvokeRequired)
                this.Invoke(uiDelegate);
            else
                uiDelegate();
        }
    }
}
