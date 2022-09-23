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
    public class GetSegmentsEfforts
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private CosmosDBRepository<StravaSegmentEffort> _cosmosRepository;
        public GetSegmentsEfforts(ILogger<GetSegmentsEfforts> logger,
                        IConfiguration config,
                        CosmosDBRepository<StravaSegmentEffort> cosmosRepository
        )
        {
            _logger = logger;
            _config = config;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName(nameof(GetSegmentsEfforts))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetSegmentsEfforts")] HttpRequest req)
        {
            try
            {
                _logger.LogInformation("GetSegmentsEfforts()");
                IEnumerable<StravaSegmentEffort> segmentEfforts = await _cosmosRepository.GetItems();
                List<StravaSegmentEffort> orderedList = new List<StravaSegmentEffort>(segmentEfforts);
                return new OkObjectResult(orderedList.OrderByDescending(s => s.StartDateLocal));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSegmentsEfforts failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
