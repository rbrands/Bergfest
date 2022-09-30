using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class ChallengeSegmentEffort : CosmosDBEntity
    {
        [JsonPropertyName("challengeId")]
        public string ChallengeId { get; set; }
        [JsonPropertyName("athleteId")]
        public ulong AthleteId { get; set; }
        [JsonPropertyName("segmentEffortId")]
        public ulong SegmentEffortId { get; set; }
        [JsonPropertyName("segmentId")]
        public ulong SegmentId { get; set; }
        [JsonPropertyName("athleteSex")]
        public string AthleteSex { get; set; }
        [JsonPropertyName("athleteName")]
        public string AthleteName { get; set; }
        [JsonPropertyName("profileImageLink")]
        public string ProfileImageLink { get; set; }
        [JsonPropertyName("activityId")]
        public ulong ActivityId { get; set; }
        [JsonPropertyName("activityName")]
        public string ActivityName { get; set; }
        [JsonPropertyName("elapsedTime")]
        public long ElapsedTime { get; set; }
        [JsonPropertyName("startDateLocal")]
        public DateTime StartDateLocal { get; set; }
        [JsonPropertyName("rankingPoints")]
        public double RankingPoints { get; set; }
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
