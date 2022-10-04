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
    public class WriteChallenge
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<StravaSegmentChallenge> _cosmosRepository;

        public WriteChallenge(ILogger<WriteChallenge> logger,
                         CosmosDBRepository<StravaSegmentChallenge> cosmosRepository
                         )
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName("WriteChallenge")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "WriteChallenge")] HttpRequest req
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                StravaSegmentChallenge challenge = JsonSerializer.Deserialize<StravaSegmentChallenge>(requestBody);
                _logger.LogInformation($"WriteChallenge(Title = {challenge.ChallengeTitle})");
                if (String.IsNullOrEmpty(challenge.UrlTitle))
                {
                    challenge.UrlTitle = challenge.GetUrlFriendlyTitle();
                }
                StravaSegmentChallenge updatedChallenge = await _cosmosRepository.UpsertItem(challenge);

                return new OkObjectResult(updatedChallenge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WriteChallenge failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
