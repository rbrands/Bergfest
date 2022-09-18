using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BlazorApp.Shared;
using System.Web.Http;
using Flurl;
using Flurl.Http.Configuration;
using Flurl.Http;
using Flurl.Http.Content;
using System.Collections.Generic;
using BlazorApp.Api.Repositories;
using BlazorApp.Api.Utils;

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
            catch (FlurlHttpException ex)
            {
                dynamic error = await ex.GetResponseJsonAsync();
                string errorMessage = error.message;
                _logger.LogError(ex, $"GetSegmentFromStrava(segmentId = {segmentId}) failed: {errorMessage}");
                return new BadRequestErrorMessageResult(errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetSegmentFromStrava(segmentId = {segmentId}) failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
