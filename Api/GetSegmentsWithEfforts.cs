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
using BlazorApp.Api.Repositories;
using BlazorApp.Api.Utils;
using Microsoft.AspNetCore.Routing;

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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetSegmentsWithEfforts")] HttpRequest req)
        {
            try
            {
                _logger.LogInformation("GetSegmentsWithEfforts()");
                IEnumerable<StravaSegment> segments = await _cosmosRepository.GetItems();
                List<StravaSegmentWithEfforts> segmentsWithEfforts = new List<StravaSegmentWithEfforts>();
                foreach (StravaSegment s in segments)
                {
                    // Get all efforts corresponding to the segment and order them date, elapsed time (desc)
                    StravaSegmentWithEfforts segmentWithEfforts = new StravaSegmentWithEfforts(s);
                    List<StravaSegmentEffort> efforts = new List<StravaSegmentEffort>(await _cosmosEffortRepository.GetItems(e => e.SegmentId == s.SegmentId && e.StartDateLocal > DateTime.UtcNow.AddDays(-Constants.DAYS_IN_PAST_FOR_EFFORTS)));
                    segmentWithEfforts.Efforts = efforts.OrderByDescending(e => e.StartDateLocal.DayOfYear).ThenBy(e => e.ElapsedTime);

                    StravaSegmentEffort mostRecentEffort = segmentWithEfforts.Efforts.FirstOrDefault();
                    if (null != mostRecentEffort)
                    {
                        segmentWithEfforts.MostRecentEffort = mostRecentEffort.StartDateLocal;
                    }
                    segmentsWithEfforts.Add(segmentWithEfforts);
                }
                // Order segments to get segment with most recent efforts on top 
                return new OkObjectResult(segmentsWithEfforts.OrderByDescending(s => s.MostRecentEffort).ThenBy(s => s.StravaSegment.Distance));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSegmentsWithEfforts failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
