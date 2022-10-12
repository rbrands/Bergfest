using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorApp.Api.Utils
{
    public class Constants
    {
        public const string VERSION = "2022-10-12";
        public const int TTL_STRAVA_ACCESS = 3 * 30 * 24 * 3600; // 3 month TTL for Strava Access
        public const double DAYS_IN_PAST_FOR_EFFORTS = 7.0; // Get the efforts for the last x days
        public const int STRAVA_MESSAGE_VISIBILITY_TIMEOUT = 9;

    }
}