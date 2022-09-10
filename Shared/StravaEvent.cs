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
        public string ObjectId { get; set; }
        public string AthleteId { get; set; }

    }

}
