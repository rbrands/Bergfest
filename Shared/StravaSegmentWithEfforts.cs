using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class StravaSegmentWithEfforts
    {
        [JsonPropertyName("stravaSegment")]
        public StravaSegment StravaSegment { get; set; }
        [JsonPropertyName("efforts")]
        public IEnumerable<StravaSegmentEffort> Efforts { get; set; }

        public StravaSegmentWithEfforts()
        {
            StravaSegment = new StravaSegment();
        }
        public StravaSegmentWithEfforts(StravaSegment stravaSegment)
        {
            StravaSegment = stravaSegment;
        }
        public StravaSegmentWithEfforts(StravaSegment stravaSegment, IEnumerable<StravaSegmentEffort> efforts) : this(stravaSegment)
        {
            Efforts = efforts;
        }
    }
}
