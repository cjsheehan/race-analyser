using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Racing
{
    public class Position
    {
        public int Place { get; set; }
        public int NumberOfRunners { get; set; }

        public Position(int place, int numberOfRunners)
        {
            Place = place;
            NumberOfRunners = numberOfRunners;
        }
    }
}
