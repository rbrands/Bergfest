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
    public class DeleteChallengeSegmentEffort
    {
        private readonly ILogger _logger;
        private ChallengeRepository _challengeRepository;

        public DeleteChallengeSegmentEffort(ILogger<DeleteChallengeSegmentEffort> logger,
                         ChallengeRepository challengeRepository)
        {
            _logger = logger;
            _challengeRepository = challengeRepository;
        }

        [FunctionName("DeleteChallengeSegmentEffort")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeleteChallengeSegmentEffort")] HttpRequest req
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                ChallengeSegmentEffort segmentEffort = JsonSerializer.Deserialize<ChallengeSegmentEffort>(requestBody);
                _logger.LogInformation($"DeleteChallengeSegmentEffort({requestBody})");
                if (String.IsNullOrEmpty(segmentEffort.Id))
                {
                    return new BadRequestErrorMessageResult("Id of StravaSegmentEffort to be deleted is missing.");
                }
                await _challengeRepository.DeleteSegmentEffort(segmentEffort);
                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteChallengeSegmentEffort failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
