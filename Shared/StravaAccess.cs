﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorApp.Shared
{
    public class StravaAccess : CosmosDBEntity
    {
        public long AthleteId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Sex { get; set; }
        public DateTime ExpirationAt { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string ProfileImageLink { get; set; }
        public string ProfileSmallImageLink { get; set; }
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
