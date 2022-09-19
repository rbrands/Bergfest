using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Shared
{
    public class StravaSegment : CosmosDBEntity
    {
        [JsonPropertyName("segmentId")]
        public ulong SegmentId { get; set; }
        // If enabled segment efforts are recorded for this segment
        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; } = true;
        // If false the results for this segment are not shown
        [JsonPropertyName("displayEnabled")]
        public bool DisplayEnabled { get; set; } = true;
        [JsonPropertyName("segmentName")]
        public string SegmentName { get; set; }
        [JsonPropertyName("distance")]
        public double Distance { get; set; }
        [JsonPropertyName("averageGrade")]
        public double AverageGrade { get; set; }
        [JsonPropertyName("maximumGrade")]
        public double MaximumGrade { get; set; }
        [JsonPropertyName("elevation")]
        public double Elevation { get; set; }
        [JsonPropertyName("climbCategory")]
        public long ClimbCategory { get; set; }
        [JsonPropertyName("city")]
        public string City { get; set; }
        [JsonPropertyName("effortCount")]
        public long EffortCount { get; set; }
        [JsonPropertyName("athleteCount")]
        public long AthleteCount { get; set; }
        // Optional link to be used as header
        [JsonPropertyName("imageLink")]
        public string ImageLink { get; set; }
        // Optional description for segment
        [JsonPropertyName("description")]
        public string Description {get; set;}
        // Optional link to a recommendated route that includes the segment
        [JsonPropertyName("routeRecommendation")]
        public string RouteRecommendation { get; set; }

        // Comma-separated list of tags to filter segments for presentation. E.g. "scuderia,dsd"
        [JsonPropertyName("tags")]
        public string Tags { get; set; }
        // Comma-separated list of labels to be used for display when presenting the results. E.g. "Sprint,Cat1" 
        [JsonPropertyName("labels")]
        public string Labels { get; set; }
        [JsonPropertyName("scope")]
        public string Scope { get; set; }
        public string GetSegmentLink()
        {
            string segmentLink = $"https://www.strava.com/segments/{SegmentId}";
            return segmentLink;
        }
        public string GetClimbCategoryLabel()
        {
            string[] climbCategories = { "-", "4", "3", "2", "1", "HC" };
            return climbCategories[ClimbCategory];
        }
        public string GetDistanceAsText()
        {
            string distanceFormat = String.Empty;
            if (Distance > 1000)
            {
                double distanceKm = Distance / 1000.0;
                distanceFormat = String.Format("{0:N2}km", distanceKm);
            }
            else
            {
                distanceFormat = String.Format("{0:N0}m", Distance);
            }
            return distanceFormat;
        }
        public string GetUrlFriendlyTitle()
        {
            string urlFriendlyTitle = null;
            if (!String.IsNullOrEmpty(SegmentName))
            {
                string titleLowerCase = SegmentName.ToLowerInvariant();
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
        public string[] GetTags()
        {
            string[] tagsAsArray = { };
            if (!String.IsNullOrEmpty(Tags))
            {
                tagsAsArray = this.Tags.Split(',');
            }
            return tagsAsArray;
        }
        public string[] GetLabels()
        {
            string[] labelsAsArray = { };
            if (!String.IsNullOrEmpty(Labels))
            {
                labelsAsArray = this.Labels.Split(',');
            }
            return labelsAsArray;
        }
    }
}