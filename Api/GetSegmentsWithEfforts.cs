using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using BlazorApp.Shared;
using System.Web.Http;
using System.Collections.Generic;
using BlazorApp.Api.Utils;
using BackendLibrary;
using System.Threading;

namespace BlazorApp.Api
{
    public class GetSegmentsWithEfforts
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private CosmosDBRepository<StravaSegment> _cosmosRepository;
        private CosmosDBRepository<StravaSegmentEffort> _cosmosEffortRepository;
        public GetSegmentsWithEfforts(ILogger<GetSegmentsWithEfforts> logger,
                        IConfiguration config,
                        CosmosDBRepository<StravaSegmentEffort> cosmosEffortRepository,
                        CosmosDBRepository<StravaSegment> cosmosRepository
        )
        {
            _logger = logger;
            _config = config;
            _cosmosEffortRepository = cosmosEffortRepository;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName(nameof(GetSegmentsWithEfforts))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetSegmentsWithEfforts/{tag}")] HttpRequest req, string tag)
        {
            try
            {
                _logger.LogInformation($"GetSegmentsWithEfforts({tag})");
                IEnumerable<StravaSegment> segments = await _cosmosRepository.GetItems();
                List<StravaSegmentWithEfforts> segmentsWithEfforts = new List<StravaSegmentWithEfforts>();
                foreach (StravaSegment s in segments)
                {
                    if (!String.IsNullOrEmpty(tag) && tag != "*")
                    {
                        string[] tags = s.GetTags();
                        tag = tag.ToLowerInvariant();
                        if (String.IsNullOrEmpty(s.Tags) || !s.Tags.Contains(tag) )
                        {
                            // If the given tag is not found in the list of tags skip this segment
                            continue;
                        }
                    }
                    // Get all efforts corresponding to the segment and order them date, elapsed time (desc)
                    StravaSegmentWithEfforts segmentWithEfforts = new StravaSegmentWithEfforts(s);
                    List<StravaSegmentEffort> efforts = new List<StravaSegmentEffort>(await _cosmosEffortRepository.GetItems(e => e.SegmentId == s.SegmentId && e.StartDateLocal > DateTime.UtcNow.AddDays(-Constants.DAYS_IN_PAST_FOR_EFFORTS)));
                    segmentWithEfforts.Efforts = efforts.OrderByDescending(e => e.StartDateLocal.DayOfYear).ThenBy(e => e.ElapsedTime);

                    // As indication that there was a group of riders on the segment: Get days with more than one item
                    var daySections = segmentWithEfforts.Efforts.GroupBy(
                        e => e.StartDateLocal.Date,
                        e => e,
                        (day, efforts) => new
                        {
                            Day = day,
                            Count = efforts.Count()
                        }
                    ).Where(d => d.Count >= StravaSegmentWithEfforts.MIN_ITEMS_TO_SHOW);
                    if (daySections.Any())
                    {
                        segmentWithEfforts.MostRecentEffort = daySections.First().Day;
                    }
                    
                    segmentsWithEfforts.Add(segmentWithEfforts);
                }
                // Order segments to get segment with most recent efforts on top 
                return new OkObjectResult(segmentsWithEfforts.OrderByDescending(s => s.MostRecentEffort).ThenBy(s => s.StravaSegment.ClimbCategory).ThenBy(s => s.StravaSegment.Distance));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSegmentsWithEfforts failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
