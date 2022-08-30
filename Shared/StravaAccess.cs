using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorApp.Shared
{
    public class StravaAccess : CosmosDBEntity
    {
        public int AthleteId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Sex { get; set; }
        public string AthleteSummary { get; set; }
        public string AthleteKey { get; set; }

        public DateTime ExpirationAt { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
