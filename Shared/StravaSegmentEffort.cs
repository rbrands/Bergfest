using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorApp.Shared
{
    public class StravaSegmentEffort : CosmosDBEntity
    {
        public long SegmentId { get; set; }
        public string SegmentName { get; set; }
        public long AthleteId { get; set; }
        public long ActivityId { get; set; }   
        public int ElapsedTime { get; set; }
        public DateTime StartDateLocal { get; set; }


    }
}
