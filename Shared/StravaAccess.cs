using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class StravaAccess : CosmosDBEntity
    {
        [JsonPropertyName("athleteId")]
        public long AthleteId { get; set; }
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
        [JsonPropertyName("sex")]
        public string Sex { get; set; }
        [JsonPropertyName("expirationAt")]
        public DateTime ExpirationAt { get; set; }
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("profileImageLink")]
        public string ProfileImageLink { get; set; }
        [JsonPropertyName("profileSmallImageLink")]
        public string ProfileSmallImageLink { get; set; }
        [JsonPropertyName("stravaAuthorizationIsPending")]
        public bool StravaAuthorizationIsPending { get; set; }
        public string GetAhtleteLink()
        {
            string link = $"https://www.strava.com/athletes/{AthleteId}";
            return link;
        }
        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
