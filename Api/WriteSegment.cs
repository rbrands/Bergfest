using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BlazorApp.Shared;
using System.Web.Http;
using BackendLibrary;


namespace BlazorApp.Api
{
    public class WriteSegment
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<StravaSegment> _cosmosRepository;

        public WriteSegment(ILogger<WriteSegment> logger,
                         CosmosDBRepository<StravaSegment> cosmosRepository
                         )
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName("WriteSegment")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "WriteSegment")] HttpRequest req
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                StravaSegment segment = JsonSerializer.Deserialize<StravaSegment>(requestBody);
                if (segment.SegmentId < 0)
                {
                    throw new Exception($"WriteSegment called with invalid SegmentId");
                }
                _logger.LogInformation($"WriteSegment(SegmentId = {segment.SegmentId} SegmentName = {segment.SegmentName})");
                if (String.IsNullOrEmpty(segment.Scope))
                {
                    segment.Scope = segment.GetUrlFriendlyTitle();
                }
                if (!String.IsNullOrEmpty(segment.Tags))
                {
                    // Tags should only by in lowercase
                    segment.Tags = segment.Tags.ToLowerInvariant();
                    char[] separatorChars = { ',', ';'};
                    string[] tags = segment.Tags.Split(separatorChars, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    segment.Tags = String.Join(',', tags);
                }
                segment.LogicalKey = segment.SegmentId.ToString();
                StravaSegment updatedSegment = await _cosmosRepository.UpsertItem(segment);

                return new OkObjectResult(updatedSegment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WriteRoute failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
