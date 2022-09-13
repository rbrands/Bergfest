using System;
using System.Collections.Generic;
using System.Text;

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

        public ObjectType EventType { get; set; }
        public AspectType Aspect { get; set; }
        public ulong ObjectId { get; set; }
        public ulong AthleteId { get; set; }

    }

}
