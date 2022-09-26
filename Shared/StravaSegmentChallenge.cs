using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class StravaSegmentChallenge : CosmosDBEntity
    {
        [JsonPropertyName("challengeTitle")]
        public string ChallengeTitle { get; set; }
        [JsonPropertyName("imageLink")]
        public string ImageLink { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("urlTitle")]
        public string UrlTitle { get; set; }
        [JsonPropertyName("startDateUTC")]
        public DateTime StartDateUTC { get; set; }
        [JsonPropertyName("endDateUTC")]
        public DateTime EndDateUTC { get; set; }
        [JsonPropertyName("isPublicVisible")]
        public bool IsPublicVisible { get; set; } = true;
        [JsonPropertyName("invitationRequired")]
        public bool InvitationRequired { get; set; } = false;
        [JsonPropertyName("invitationLink")]
        public string InvitationLink { get; set; }
    }
}
