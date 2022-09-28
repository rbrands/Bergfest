using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class StravaSegmentChallenge : CosmosDBEntity
    {
        [JsonPropertyName("challengeTitle")]
        public string ChallengeTitle { get; set; }
        [JsonPropertyName("imageLink")]
        public string ImageLink { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("urlTitle")]
        public string UrlTitle { get; set; }
        [JsonPropertyName("startDateUTC")]
        public DateTime StartDateUTC { get; set; }
        [JsonPropertyName("endDateUTC")]
        public DateTime EndDateUTC { get; set; }
        [JsonPropertyName("isPublicVisible")]
        public bool IsPublicVisible { get; set; } = true;
        [JsonPropertyName("invitationRequired")]
        public bool InvitationRequired { get; set; } = false;
        [JsonPropertyName("invitationLink")]
        public string InvitationLink { get; set; }
        public IList<StravaSegment> Segments { get; set; }
        public string GetUrlFriendlyTitle()
        {
            string urlFriendlyTitle = null;
            if (!String.IsNullOrEmpty(ChallengeTitle))
            {
                string titleLowerCase = ChallengeTitle.ToLowerInvariant();
                StringBuilder sb = new StringBuilder();
                int charCounter = 0;
                foreach (char c in titleLowerCase)
                {
                    if (++charCounter > 160)
                    {
                        // url not longer than 160 chars
                        break;
                    }
                    switch (c)
                    {
                        case '\u00F6':
                        case '\u00D6':
                            sb.Append("oe");
                            break;
                        case '\u00FC':
                        case '\u00DC':
                            sb.Append("ue");
                            break;
                        case '\u00E4':
                        case '\u00C4':
                            sb.Append("ae");
                            break;
                        case '\u00DF':
                            sb.Append("ss");
                            break;
                        case 'a':
                        case 'b':
                        case 'c':
                        case 'd':
                        case 'e':
                        case 'f':
                        case 'g':
                        case 'h':
                        case 'i':
                        case 'j':
                        case 'k':
                        case 'l':
                        case 'm':
                        case 'n':
                        case 'o':
                        case 'p':
                        case 'q':
                        case 'r':
                        case 's':
                        case 't':
                        case 'u':
                        case 'v':
                        case 'w':
                        case 'x':
                        case 'y':
                        case 'z':
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            sb.Append(c);
                            break;
                        default:
                            sb.Append('-');
                            break;
                    }
                }
                urlFriendlyTitle = sb.ToString().Trim('-');
            }
            return urlFriendlyTitle;
        }
    }
}
