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
    public class DeleteSegmentEffort
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<StravaSegmentEffort> _cosmosRepository;
        private ChallengeRepository _challengeRepository;

        public DeleteSegmentEffort(ILogger<DeleteSegmentEffort> logger,
                         CosmosDBRepository<StravaSegmentEffort> cosmosRepository,
                         ChallengeRepository challengeRepository)
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
            _challengeRepository = challengeRepository;
        }

        [FunctionName("DeleteSegmentEffort")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeleteSegmentEffort")] HttpRequest req
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                StravaSegmentEffort segmentEffort = JsonSerializer.Deserialize<StravaSegmentEffort>(requestBody);
                _logger.LogInformation($"DeleteSegmentEffort({requestBody})");
                if (String.IsNullOrEmpty(segmentEffort.Id))
                {
                    return new BadRequestErrorMessageResult("Id of StravaSegmentEffort to be deleted is missing.");
                }
                await _cosmosRepository.DeleteItemAsync(segmentEffort.Id);
                var challengeEfforts = await _challengeRepository.GetItems(se => se.SegmentEffortId == segmentEffort.SegmentEffortId);
                foreach (var s in challengeEfforts)
                {
                    await _challengeRepository.DeleteSegmentEffort(s);
                }
                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteSegmentEffort failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
