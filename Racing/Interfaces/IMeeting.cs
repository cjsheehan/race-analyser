using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Racing
{
    public interface IMeeting
    {
        string Course { get; set; }
        string Date { get; set; }
        string Going { get; set; }
        string Weather { get; set; }
        List<IRace> RaceCards { get; }
    }
}
