using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorApp.Shared
{
    public class StravaSegmentEffort : CosmosDBEntity
    {
        public ulong SegmentEffortId { get; set; }
        public ulong SegmentId { get; set; }
        public string SegmentName { get; set; }
        public ulong AthleteId { get; set; }
        public ulong ActivityId { get; set; }   
        public long ElapsedTime { get; set; }
        public DateTime StartDateLocal { get; set; }
        public string GetSegmentLink()
        {
            string segmentLink = $"https://www.strava.com/segments/{SegmentId}";
            return segmentLink;
        }
        public string GetActivityLink()
        {
            string link = $"https://www.strava.com/activities/{ActivityId}";
            return link;
        }
        public string GetAhtleteLink()
        {
            string link = $"https://www.strava.com/athletes/{AthleteId}";
            return link;
        }
        public string GetActivitySegmentLink()
        {
            string link = $"https://www.strava.com/activities/{ActivityId}/segments/{SegmentEffortId}";
            return link;
        }
    }
}
