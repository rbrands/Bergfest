using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BlazorApp.Shared;
using System.Web.Http;
using BackendLibrary;

namespace BlazorApp.Api
{
    public class GetSegment
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private CosmosDBRepository<StravaSegment> _cosmosRepository;

        public GetSegment(ILogger<GetSegment> logger,
                        IConfiguration config,
                        CosmosDBRepository<StravaSegment> cosmosRepository
        )
        {
            _logger = logger;
            _config = config;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName(nameof(GetSegment))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetSegment/{segmentId}")] HttpRequest req, ulong segmentId)
        {
            try
            {
                if (0 == segmentId)
                {
                    throw new Exception("Missing segmentId for call GetSegment()");
                }
                StravaSegment segment = await _cosmosRepository.GetItemByKey(segmentId.ToString());
                _logger.LogInformation($"GetSegment(SegmentId = {segmentId}, SegmentName = {segment.SegmentName})", segmentId);
                return new OkObjectResult(segment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetSegment(segmentId = {segmentId}) failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
