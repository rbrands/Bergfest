using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BlazorApp.Shared;
using System.Web.Http;
using System.Collections.Generic;
using BackendLibrary;


namespace BlazorApp.Api
{
    public class GetSegments
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private CosmosDBRepository<StravaSegment> _cosmosRepository;
        public GetSegments(ILogger<GetSegments> logger,
                        IConfiguration config,
                        CosmosDBRepository<StravaSegment> cosmosRepository
        )
        {
            _logger = logger;
            _config = config;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName(nameof(GetSegments))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetSegments")] HttpRequest req)
        {
            try
            {
                _logger.LogInformation($"GetSegments()");
                IEnumerable<StravaSegment> segments = await _cosmosRepository.GetItems();
                return new OkObjectResult(segments.OrderBy(s => s.Distance));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSegments failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
