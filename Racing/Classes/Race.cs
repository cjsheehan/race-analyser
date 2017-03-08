using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Racing
{
    public class Race
    {
        public int Id { get; set; }
        public string Course { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }

        private string title;
        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                // RaceType is extracted from race title
                if (value.ToUpper().Contains("HURDLE"))
                {
                    Type = RaceType.HURDLES;
                }
                else if (value.ToUpper().Contains("CHASE"))
                {
                    Type = RaceType.FENCES;
                }
                else
                {
                    Type = RaceType.FLAT;
                }

                this.title = value;
            }
        }
        public string Info { get; set; }
        public string Url { get; set; }
        public RaceType Type { get; set; }
        public uint MaxOR { get; set; }
        public uint? MinAge { get; set; }
        public uint? MaxAge { get; set; }
        public PrizeList Prizes { get; set; }
        public bool Flat { get; set; }
        public string Distance { get; set; }
        public uint TotalYds { get; set; }
        public string Going { get; set; }
        public Going Egoing { get; set; }
        public string Category { get; set; }
        public int NumberOfRunners { get; set; }
        public List<Entrant> Entrants { get; set; }
        public Race()
        {
            Entrants = new List<Entrant>();
            Prizes = new PrizeList();
            MinAge = 0;
            MaxAge = 0;
            Type = RaceType.UNKNOWN;
        }

        public override string ToString()
        {
            string s =
                "Course : " + Course + Environment.NewLine + "\t\t"
                + "Time :" + Time + Environment.NewLine + "\t\t"
                + "Title :" + Title + Environment.NewLine + "\t\t"
                + "RaceType :" + Type.ToString() + Environment.NewLine + "\t\t"
                + "RaceId :" + Id + Environment.NewLine + "\t\t"
                + "Info :" + Info + Environment.NewLine + "\t\t"
                + "MaxOR :" + MaxOR + Environment.NewLine + "\t\t"
                + "MinAge :" + MinAge + Environment.NewLine + "\t\t"
                + "MaxAge :" + MaxAge + Environment.NewLine + "\t\t"
                + "Prizes :" + string.Join(",", Prizes) + Environment.NewLine + "\t\t"
                + "Date :" + Date + Environment.NewLine + "\t\t"
                + "Flat :" + Flat + Environment.NewLine + "\t\t"
                + "Dist :" + Distance + Environment.NewLine + "\t\t"
                + "TotalYds :" + TotalYds + Environment.NewLine + "\t\t"
                + "Going :" + Egoing + Environment.NewLine + "\t\t"
                + "Class :" + Category + Environment.NewLine + "\t\t"
                + "Runners :" + NumberOfRunners + Environment.NewLine + "\t\t"
                + "Url :" + Url + Environment.NewLine;

            return s;
        }
    }



    public class RaceHeader : IRaceHeader
    {
        private RaceHeader()
        {
        }

        public string Category { get; private set; }
        public int Id { get; private set; }
        public string Course { get; private set; }
        public string Going { get; private set; }
        public string Date { get; private set; }
        public string Time { get; private set; }
        public string Title { get; private set; }
        public string Info { get; private set; }
        public string Url { get; private set; }
        private RaceType _raceType = RaceType.UNKNOWN;
        public  RaceType Type
        {
            get
            {
                return _raceType;
            }

            set
            {
                _raceType = value;
                NotifyPropertyChanged("Type");
            }
        }


        private bool _selected = false;
        public bool Selected
        {
            get
            {
                return _selected;
            }

            set
            {
                _selected = value;
                NotifyPropertyChanged("Selected");
            }
        }

        public RaceHeader(int id, string course, string going, String category, string date, string time, string title, string info, string url, RaceType type)
        {
            Id = id;
            Course = course;
            Going = going;
            Category = category;
            Date = date;
            Time = time;
            Title = title;
            Info = info;
            Url = url;
            Type = type;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteString(Id.ToString());
            writer.WriteString(Course);
            writer.WriteString(Date);
            writer.WriteString(Time);
            writer.WriteString(Title);
            writer.WriteString(Info);
            writer.WriteString(Url);
            writer.WriteString(Type.ToString());
        }
  
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class RaceInfo : IRaceDetail
    {
        public string Url { get; set; }
        public uint MaxOR { get; set; }
        public uint? MinAge { get; set; }
        public uint? MaxAge { get; set; }
        public PrizeList Prizes { get; set; }
        public bool Flat { get; set; }
        public string Dist { get; set; }
        public uint TotalYds { get; set; }
        public string Going { get; set; }
        public Going Egoing { get; set; }
        public string Class { get; set; }
        public int Runners { get; set; }
        public List<Entrant> Horses { get; set; }
    }

    public class PrizeList
    {
        public Currency Currency { get; set; }
        public List<decimal> PrizeMoney;

        public PrizeList()
        {
            Currency = Currency.UNKNOWN;
            PrizeMoney = new List<decimal>();
        }

    }

}
