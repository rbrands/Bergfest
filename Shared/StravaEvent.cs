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
        public long ObjectId { get; set; }
        public long AthleteId { get; set; }

    }

}
