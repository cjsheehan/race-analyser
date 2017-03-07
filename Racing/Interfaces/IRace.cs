using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Racing
{
    public interface IRace
    {
        int Id { get; set; }
        string Course { get; set; }
        string Date { get; set; }
        string Time { get; set; }
        string Title { get; set; }
        string Info { get; set; }
        string Url { get; set; }
        RaceType Type { get; set; }
        uint MaxOR { get; set; }
        uint? MinAge { get; set; }
        uint? MaxAge { get; set; }
        PrizeList Prizes { get; set; }
        string Dist { get; set; }
        uint TotalYds { get; set; }
        string Going { get; set; }
        Going Egoing { get; set; }
        string Class { get; set; }
        string Runners { get; set; }
        List<IHorse> Horses { get; set; }
    }

    public interface IRaceHeader : INotifyPropertyChanged, System.Xml.Serialization.IXmlSerializable
    {
        int Id { get; }
        string Course { get; }
        string Going { get; }
        string Date { get; }
        string Time { get; }
        string Title { get; }
        string Info { get; }
        string Url { get; }
        RaceType Type { get; set; }
        bool Selected { get; set; }
    }

    public interface IRaceDetail
    {
        string Url { get; set; }
        uint MaxOR { get; set; }
        uint? MinAge { get; set; }
        uint? MaxAge { get; set; }
        PrizeList Prizes { get; set; }
        string Dist { get; set; }
        uint TotalYds { get; set; }
        string Going { get; set; }
        Going Egoing { get; set; }
        string Class { get; set; }
        int Runners { get; set; }
        List<Entrant> Horses { get; set; }
    }
}
