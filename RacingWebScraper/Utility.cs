using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;

namespace RacingWebScraper
{
    public static class Utility
    {
        private static Regex rxClean = new Regex(@"^\s*(.*)\s*$");

        public static String RemoveWhitespace(String data)
        {
            Match m = rxClean.Match(data);
            if (m.Success == true)
            {
                data = m.Groups[1].Value;
            }
            return data;
        }

        public static String StdFormat(String data)
        {
            if (String.IsNullOrEmpty(data)) throw new System.ArgumentNullException("string data cannot be null");

            data = Regex.Replace(data, @"\n", "");   // Remove newlines
            data = Regex.Replace(data, @"\s+", " "); // Reduce multiple spaces to a single space
            return data;
        }
    
        public static List<DateTime> GetAllDates(DateTime startingDate, DateTime endingDate)
        {
            List<DateTime> allDates = new List<DateTime>();

            int starting = startingDate.Day;
            int ending = endingDate.Day;

            for (DateTime date = startingDate; date <= endingDate; date = date.AddDays(1))
            {
                allDates.Add(date);
            }

            return allDates;
        }

        public static List<DateTime> GetUniqueDates(List<DateTime> a, List<DateTime> b)
        {
            var uniqueInA = a.Distinct();
            var uniqueInB = b.Distinct();
            var uniqueAvB = (uniqueInA.Select(x => x.Date).Except(uniqueInB.Select(y => y.Date))).ToList(); // Enumerable -> List
            return uniqueAvB;
        }

        public static bool ExtractString(Regex rx, string dataIn, out string dataOut)
        {
            dataOut = null;
            bool success = false;
            Match m = rx.Match(dataIn);
            if (m.Success)
            {
                dataOut = m.Groups[1].Value;
                success = true;
            }
            return success;
        }

        public static bool ExtractUint(Regex rx, string dataIn, out uint dataOut)
        {
            dataOut = 0;
            bool success = false;
            Match m = rx.Match(dataIn);
            if (m.Success)
            {
                dataOut = Convert.ToUInt32(m.Groups[1].Value);
                success = true;
            }
            return success;
        }

    }

}
