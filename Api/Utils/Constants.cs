using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorApp.Api.Utils
{
    public class Constants
    {
        public const string VERSION = "2022-09-12";
        public const int TTL_STRAVA_ACCESS = 3 * 30 * 24 * 3600; // 3 month TTL for Strava Access

    }
}