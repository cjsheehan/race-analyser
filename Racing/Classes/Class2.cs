using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Racing.Classes
{
    public class RootObject
    {
        public Props Props { get; set; }
    }

    [JsonObject("props")]
    public class Props
    {
        public Pageprops pageProps { get; set; }
    }
    [JsonObject("pageProps")]
    public class Pageprops
    {
        public List<Meeting> meetings { get; set; }
    }
    public class Meeting 
    {
        [JsonProperty("meeting_summary")]
        public MeetingSummary meetingSummary { get; set; }
        public List<Race> races { get; set; }
    }

    [JsonObject("meeting_summary")]
    public class MeetingSummary
    {
        [JsonProperty("meeting_reference")]
        public MeetingReference meetingReference { get; set; }
        public string date { get; set; }
        public string status { get; set; }
        public Course course { get; set; }
        public string going { get; set; }
        [JsonProperty("surface_summary")]
        public string surfaceSummary { get; set; }
        public bool visible { get; set; }
    }

    [JsonObject("meeting_reference")]
    public class MeetingReference
    {
        public int id { get; set; }
        [JsonProperty("external_reference")]
        public List<object> ExternalReference { get; set; }
    }

    [JsonObject("course")]
    public class Course
    {
        [JsonProperty("course_reference")]
        public CourseReference courseReference { get; set; }
        public string name { get; set; }
        public Country country { get; set; }
        [JsonProperty("feed_source")]
        public string feedSource { get; set; }
    }

    [JsonObject("course_reference")]
    public class CourseReference
    {
        public int id { get; set; }
        public List<object> externalReference { get; set; }
    }

    [JsonObject("country")]
    public class Country
    {
        [JsonProperty("country_reference")]
        public CountryReference countryReference { get; set; }
        [JsonProperty("long_name")]
        public string longName { get; set; }
    }


    [JsonObject(Title="country_reference")]
    public class CountryReference
    {
        public int id { get; set; }
        [JsonProperty("external_reference")]
        public List<object> externalReference { get; set; }
    }

    public class Race
    {
        [JsonProperty("meeting_summary_reference")]
        public MeetingSummaryReference meetingSummaryReference { get; set; }
        [JsonProperty("race_summary_reference")]
        public RaceSummaryReference raceSummaryReference { get; set; }
        public string name { get; set; }
        public string course_name { get; set; }
        [JsonProperty("course_shortcode")]
        public string courseShortcode { get; set; }
        [JsonProperty("course_surface")]
        public CourseSurface courseSurface { get; set; }
        public string age { get; set; }
        [JsonProperty("race_class")]
        public string raceClass { get; set; }
        public string distance { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public string url { get; set; }
        [JsonProperty("ride_count")]
        public int rideCount { get; set; }
        [JsonProperty("race_stage")]
        public string raceStage { get; set; }
        [JsonProperty("has_handicap")]
        public bool hasHandicap { get; set; }
        public string verdict { get; set; }
        public bool hidden { get; set; }
    }

    [JsonObject("meeting_summary_reference")]
    public class MeetingSummaryReference
    {
        public int id { get; set; }
        [JsonProperty("external_reference")]
        public List<object> externalReference { get; set; }
    }

    [JsonObject("race_summary_reference")]
    public class RaceSummaryReference
    {
        public int id { get; set; }
        [JsonProperty("external_reference")]
        public List<object> externalReference { get; set; }
    }

    [JsonObject("course_surface")]
    public class CourseSurface
    {
        public string surface { get; set; }
    }
}
