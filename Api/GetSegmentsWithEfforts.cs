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
                    StravaSegmentWithEfforts segmentWithEfforts = new StravaSegmentWithEfforts(s);
                    List<StravaSegmentEffort> efforts = new List<StravaSegmentEffort>(await _cosmosEffortRepository.GetItems(e => e.SegmentId == s.SegmentId));
                    segmentWithEfforts.Efforts = efforts.OrderBy(e => e.ElapsedTime);

                    segmentsWithEfforts.Add(segmentWithEfforts);
                }

                return new OkObjectResult(segmentsWithEfforts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSegmentsWithEfforts failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
