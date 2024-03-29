﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class StravaSegmentWithEfforts
    {
        public const int MIN_ITEMS_TO_SHOW = 3;
        [JsonPropertyName("stravaSegment")]
        public StravaSegment StravaSegment { get; set; }
        [JsonPropertyName("mostRecentEffort")]
        public DateTime MostRecentEffort { get; set; } = DateTime.MinValue;
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
