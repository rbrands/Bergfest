using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BlazorApp.Shared;
using System.Web.Http;
using BlazorApp.Api.Repositories;
using BlazorApp.Api.Utils;
using Microsoft.AspNetCore.Routing;

namespace BlazorApp.Api
{
    public class DeleteSegment
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<StravaSegment> _cosmosRepository;
        private CosmosDBRepository<StravaSegmentEffort> _cosmosEffortRepository;

        public DeleteSegment(ILogger<DeleteSegment> logger,
                         CosmosDBRepository<StravaSegmentEffort> cosmosEffortRepository,
                         CosmosDBRepository<StravaSegment> cosmosRepository)
        {
            _logger = logger;
            _cosmosEffortRepository = cosmosEffortRepository;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName("DeleteSegment")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeleteSegment")] HttpRequest req
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                StravaSegment segment = JsonSerializer.Deserialize<StravaSegment>(requestBody);
                _logger.LogInformation($"DeleteSegment({requestBody})");
                if (String.IsNullOrEmpty(segment.Id))
                {
                    return new BadRequestErrorMessageResult("Id of StravaSegment to be deleted is missing.");
                }
                await _cosmosRepository.DeleteItemAsync(segment.Id);
                // Delete all efforts
                IEnumerable<StravaSegmentEffort> efforts = await _cosmosEffortRepository.GetItems(e => e.SegmentId == segment.SegmentId);
                foreach (StravaSegmentEffort e in efforts)
                {
                    await _cosmosEffortRepository.DeleteItemAsync(e.Id);
                }
                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteSegment failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
