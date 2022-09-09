using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bergfest_Webhook.Utils
{
    public class Constants
    {
        public const string VERSION = "2022-09-07";
        public const int STRAVA_TTL_ACCESS = 3 * 30 * 24 * 3600; // 3 month TTL for Strava Access
        public const string STRAVA_WEBHOOK_VERIFY_TOKEN = "Bergfest-Webhook";

        
    }
}
