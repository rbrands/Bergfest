using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorApp.Shared
{
    public class StravaSegment : CosmosDBEntity
    {
        public long SegmentId { get; set; }
        public string SegmentName { get; set; }
        // Optional description for segment
        public string Description {get; set;}
        // Comma-separated list of tags to filter segments for presentation. E.g. "scuderia,dsd"
        public string Tags { get; set; }
        // Comma-separated list of labels to used for display labels when presenting the results. E.g. "Sprint,Cat1" 
        public string Labels { get; set; }
        public string GetSegmentLink()
        {
            string segmentLink = $"https://www.strava.com/segments/{SegmentId}";
            return segmentLink;
        }
    }
}