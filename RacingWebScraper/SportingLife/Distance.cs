using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RacingWebScraper
{
    public class Distance
    {

        // conversions
        const int YDS_PER_MILE = 1760;
        const int YDS_PER_FURLONG = 220;

        // regex
        const String patternMFY = "(\\d+)m *(\\d+)f *(\\d+)y";
        const String patternMF = "(\\d+)m *(\\d+)f";
        const String patternMY = "(\\d+)m *(\\d+)y";
        const String patternM = "(\\d+)m *";
        const String patternFY = "(\\d+)f *(\\d+)y";
        const String patternF = "(\\d+)f *";
        const String patternY = "(\\d+)y *";

        const String patternDecimal = "(\\d+\\.?\\d*)";
        const String patternWholeAndFrac = "(\\d+) *(\\d)/(\\d)$";
        const String patternFracOnly = "(\\d)/(\\d)$";

        public static double ToYards(String distance)
        {
            double yards = 0.0;

            Regex rxMFY = new Regex(patternMFY);
            Match matchMFY = rxMFY.Match(distance);

            Regex rxMF = new Regex(patternMF);
            Match matchMF = rxMF.Match(distance);

            Regex rxMY = new Regex(patternMY);
            Match matchMY = rxMY.Match(distance);

            Regex rxM = new Regex(patternM);
            Match matchM = rxM.Match(distance);

            Regex rxFY = new Regex(patternFY);
            Match matchFY = rxFY.Match(distance);

            Regex rxF = new Regex(patternF);
            Match matchF = rxMFY.Match(distance);

            Regex rxY = new Regex(patternY);
            Match matchY = rxY.Match(distance);


            if (matchMFY.Success)
            {
                yards = (YDS_PER_MILE * int.Parse(matchMFY.Groups[1].Value))
                        + (YDS_PER_FURLONG * int.Parse(matchMFY.Groups[2].Value))
                        + int.Parse(matchMFY.Groups[3].Value);

            }
            else if (matchMF.Success)
            {
                yards = (YDS_PER_MILE * int.Parse(matchMF.Groups[1].Value))
                        + (YDS_PER_FURLONG * int.Parse(matchMF.Groups[2].Value));
            }
            else if (matchMY.Success)
            {
                yards = (YDS_PER_MILE * int.Parse(matchMY.Groups[1].Value)) + int.Parse(matchMY.Groups[2].Value);
            }
            else if (matchM.Success)
            {
                yards = YDS_PER_MILE * int.Parse(matchM.Groups[1].Value);
            }
            else if (matchFY.Success)
            {
                yards = (YDS_PER_FURLONG * int.Parse(matchFY.Groups[1].Value) + int.Parse(matchFY.Groups[2].Value));
            }
            else if (matchF.Success)
            {
                yards = (YDS_PER_FURLONG * int.Parse(matchF.Groups[1].Value));
            }
            else if (matchY.Success)
            {
                yards = int.Parse(matchY.Groups[1].Value);
            }
            else
            {
                throw new ArgumentException("distance is invalid - must be at least 1 value for miles/furlongs/yards");
            }

            return yards;
        }

        public static double ConvertLengthsToDouble(String distance)
        {
            if (String.IsNullOrEmpty(distance)) throw new ArgumentException("distance is null or empty");

            double converted = 0.0;
            if (distance.Equals("dh"))
            {
                converted = 0.0;
            }
            else if (distance.Equals("nse"))
            {
                converted = 0.01;
            }
            else if (distance.Equals("shd"))
            {
                converted = 0.1;
            }
            else if (distance.Equals("hd"))
            {
                converted = 0.2;
            }
            else if (distance.Equals("nk"))
            {
                converted = 0.3;
            }
            else if (distance.Equals("dis") || distance.Equals("dist"))
            {
                converted = 50.0;
            }
            else
            {
                distance = distance.Normalize(NormalizationForm.FormKD).Replace("\u2044", "/");

                Regex rxDecimal = new Regex(patternDecimal);
                Match matchDecimal = rxDecimal.Match(distance);

                Regex rxFrac = new Regex(patternFracOnly);
                Match matchFrac = rxFrac.Match(distance);

                Regex rxWholeAndFrac = new Regex(patternWholeAndFrac);
                Match matchWholeAndFrac = rxWholeAndFrac.Match(distance);

                if (matchWholeAndFrac.Success)
                {
                    double whole = Double.Parse(matchWholeAndFrac.Groups[1].Value);
                    double numerator = Double.Parse(matchWholeAndFrac.Groups[2].Value);
                    double denominator = Double.Parse(matchWholeAndFrac.Groups[3].Value);
                    converted = whole + numerator / denominator;
                }
                else if (matchFrac.Success)
                {
                    double numerator = Double.Parse(matchFrac.Groups[1].Value);
                    double denominator = Double.Parse(matchFrac.Groups[2].Value);
                    converted = numerator / denominator;
                }
                else if (matchDecimal.Success)
                {
                    converted = Double.Parse(matchDecimal.Groups[1].Value);
                }
                else
                {
                    throw new ArgumentException("distance :" + distance + " is invalid format");
                }
            }

            return converted;
        }

        //public const boolean isValid(String distance) {
        //    Regex mMFY = pMFY.matcher(distance);
        //    Regex mMF = pMF.matcher(distance);
        //    Regex mMY = pMY.matcher(distance);
        //    Regex mM = pM.matcher(distance);
        //    Regex mFY = pFY.matcher(distance);
        //    Regex mF = pF.matcher(distance);
        //    Regex mY = pY.matcher(distance);

        //    if (mMFY.matches()) {
        //        return true;
        //    } else if (mMF.matches()) {
        //        return true;
        //    } else if (mMY.matches()) {
        //        return true;
        //    } else if (mM.matches()) {
        //        return true;
        //    } else if (mFY.matches()) {
        //        return true;
        //    } else if (mF.matches()) {
        //        return true;
        //    } else if (mY.matches()) {
        //        return true;
        //    } else {
        //        return false;
        //    }
        //}


    }
}
