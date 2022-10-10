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
    public class WriteChallengeSegmentEffort
    {
        private readonly ILogger _logger;
        private ChallengeRepository _cosmosRepository;

        public WriteChallengeSegmentEffort(ILogger<WriteChallengeSegmentEffort> logger,
                         ChallengeRepository cosmosRepository
                         )
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName("WriteChallengeSegmentEffort")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "WriteChallengeSegmentEffort")] HttpRequest req
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                ChallengeSegmentEffort effort = JsonSerializer.Deserialize<ChallengeSegmentEffort>(requestBody);
                if (String.IsNullOrEmpty(effort.ChallengeId))
                {
                    throw new Exception($"WriteChallengeSegmentEffort called with missing ChallengeId");
                }
                _logger.LogInformation($"WriteChallengeSegmentEffort(SegmentTitle = {effort.SegmentTitle})");
                ChallengeSegmentEffort updatedEffort = await _cosmosRepository.UpsertSegmentEffort(effort);

                return new OkObjectResult(updatedEffort);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WriteChallengeSegmentEffort failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
