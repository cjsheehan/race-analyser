using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Racing
{
    public enum RaceType
    {
        HURDLES,
        FENCES,
        FLAT,
        UNKNOWN
    }

    public enum Going
    {
        HARD,
        FIRM,
        GOOD_TO_FIRM,
        GOOD,
        GOOD_TO_SOFT,
        SOFT,
        SOFT_TO_HEAVY,
        HEAVY,
        STD,
        YIELDING,
        UNKNOWN
    }

    public enum Currency
    {
        GBP,
        EUR,
        USD,
        UNKNOWN
    }

}
