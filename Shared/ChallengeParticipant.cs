using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class ChallengeParticipant : CosmosDBEntity
    {
        [JsonPropertyName("challengeId")]
        public string ChallengeId { get; set; }
        [JsonPropertyName("athleteId")]
        public ulong AthleteId { get; set; }
        [JsonPropertyName("athleteSex")]
        public string AthleteSex { get; set; }
        [JsonPropertyName("athleteName")]
        public string AthleteName { get; set; }
        [JsonPropertyName("rank")]
        public int Rank { get; set; }
        [JsonPropertyName("totalPoints")]
        public int TotalPoints { get; set; }
        public string GetAhtleteLink()
        {
            string link = $"https://www.strava.com/athletes/{AthleteId}";
            return link;
        }
    }
}
