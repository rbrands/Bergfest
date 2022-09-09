using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class StravaValidationResponse
    {
        [JsonPropertyName("hub.challenge")]
        public string HubChallenge { get; set; }
    }
}
