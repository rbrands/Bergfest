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
    public class GetSegmentsWithEffortsForScope
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private CosmosDBRepository<StravaSegment> _cosmosRepository;
        private CosmosDBRepository<StravaSegmentEffort> _cosmosEffortRepository;
        private ChallengeRepository _challengeRepository;
        public GetSegmentsWithEffortsForScope(ILogger<GetSegmentsWithEffortsForScope> logger,
                        IConfiguration config,
                        CosmosDBRepository<StravaSegmentEffort> cosmosEffortRepository,
                        CosmosDBRepository<StravaSegment> cosmosRepository,
                        ChallengeRepository challengeRepository
        )
        {
            _logger = logger;
            _config = config;
            _cosmosEffortRepository = cosmosEffortRepository;
            _cosmosRepository = cosmosRepository;
            _challengeRepository = challengeRepository;
        }

        [FunctionName(nameof(GetSegmentsWithEffortsForScope))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetSegmentsWithEffortsForScope/{scope}")] HttpRequest req, string scope)
        {
            try
            {
                _logger.LogInformation($"GetSegmentsWithEffortsForScope({scope})");
                StravaSegmentChallenge
                 challenge = await _challengeRepository.GetChallengeByTitle(scope);
                if (null == challenge)
                {
                    throw new Exception($"No challenge with >{scope}< found");
                }
                IEnumerable<StravaSegment> segments = await _cosmosRepository.GetItems();
                List<StravaSegmentWithEfforts> segmentsWithEfforts = new List<StravaSegmentWithEfforts>();
                foreach (var s in challenge.Segments)
                {
                    StravaSegment stravaSegment = new StravaSegment()
                    {
                        SegmentId = s.Value.SegmentId,
                        IsEnabled = true,
                        DisplayEnabled = true,
                        SegmentName = s.Value.SegmentName,
                        Distance = s.Value.Distance,
                        AverageGrade = s.Value.AverageGrade,
                        MaximumGrade = s.Value.MaximumGrade,
                        Elevation = s.Value.Elevation,
                        ClimbCategory = s.Value.ClimbCategory,
                        City = s.Value.City,
                        ImageLink = s.Value.ImageLink,
                        Description = s.Value.Description,
                        RouteRecommendation = s.Value.RouteRecommendation,
                        Tags = s.Value.Tags,
                        Labels = s.Value.Labels,
                        Scope = s.Value.Scope
                    };
                    // Get all efforts corresponding to the segment and order them date, elapsed time (desc)
                    StravaSegmentWithEfforts segmentWithEfforts = new StravaSegmentWithEfforts(stravaSegment);
                    List<StravaSegmentEffort> efforts = new List<StravaSegmentEffort>(await _cosmosEffortRepository.GetItems(e => e.SegmentId == s.Value.SegmentId && e.StartDateLocal > DateTime.UtcNow.AddDays(-Constants.DAYS_IN_PAST_FOR_EFFORTS)));
                    segmentWithEfforts.Efforts = efforts.OrderByDescending(e => e.StartDateLocal.DayOfYear).ThenBy(e => e.ElapsedTime);

                    // As indication that there was a group of riders on the segment: Get days with more than 2 items
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
                _logger.LogError(ex, "GetSegmentsWithEffortsForScope failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
