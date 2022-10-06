using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class ChallengeWithEfforts
    {
        [JsonPropertyName("challenge")]
        public StravaSegmentChallenge Challenge { get; set; }
        [JsonPropertyName("efforts")]
        public IList<ChallengeSegmentEffort> Efforts { get; set; }  
    }
}
