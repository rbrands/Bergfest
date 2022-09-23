using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class StravaEvent
    {
        public enum ObjectType
        {
            Activity,
            Athlete
        }
        public enum AspectType
        {
                Create,
                Update,
                Delete,
                Deauthorize
        }
        [JsonPropertyName("EventType")]
        public ObjectType EventType { get; set; }
        [JsonPropertyName("Aspect")]
        public AspectType Aspect { get; set; }
        [JsonPropertyName("ObjectId")]
        public ulong ObjectId { get; set; }
        [JsonPropertyName("AthleteId")]
        public ulong AthleteId { get; set; }

    }

}
