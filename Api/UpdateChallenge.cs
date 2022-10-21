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
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using BackendLibrary;


namespace BlazorApp.Api
{
    public class UpdateChallenge
    {
        private readonly ILogger _logger;
        private ChallengeRepository _cosmosRepository;

        public UpdateChallenge(ILogger<UpdateChallenge> logger,
                         ChallengeRepository cosmosRepository
                         )
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName("UpdateChallenge")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "UpdateChallenge")] HttpRequest req
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation(requestBody);
                StravaSegmentChallenge challenge = JsonSerializer.Deserialize<StravaSegmentChallenge>(requestBody);

                StravaSegmentChallenge updatedChallenge = await _cosmosRepository.UpdateChallenge(challenge);

                return new OkObjectResult(updatedChallenge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateChallenge failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
