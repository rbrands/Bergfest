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
        private CosmosDBRepository<StravaSegmentChallenge> _cosmosRepository;

        public UpdateChallenge(ILogger<UpdateChallenge> logger,
                         CosmosDBRepository<StravaSegmentChallenge> cosmosRepository
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
                StravaSegmentChallenge challenge = JsonSerializer.Deserialize<StravaSegmentChallenge>(requestBody);
                _logger.LogInformation($"UpdateChallenge(Title = {challenge.ChallengeTitle})");
                if (String.IsNullOrEmpty(challenge.UrlTitle))
                {
                    challenge.UrlTitle = challenge.GetUrlFriendlyTitle();
                }
                List<PatchOperation> patchOperations = new()
                {
                    PatchOperation.Add("/ChallengeTitle", challenge.ChallengeTitle),
                    PatchOperation.Add("/Description", challenge.Description),
                    PatchOperation.Add("/ImageLink", challenge.ImageLink),
                    PatchOperation.Add("/UrlTitle", challenge.UrlTitle),
                    PatchOperation.Add("/StartDateUTC", challenge.StartDateUTC),
                    PatchOperation.Add("/EndDateUTC", challenge.EndDateUTC),
                    PatchOperation.Add("/IsPublicVisible", challenge.IsPublicVisible),
                    PatchOperation.Add("/InvitationRequired", challenge.InvitationRequired),
                    PatchOperation.Add("/RegistrationIsOpen", challenge.RegistrationIsOpen),
                    PatchOperation.Add("/InvitationLink", challenge.InvitationLink),
                    PatchOperation.Add("/PointLookup", challenge.PointLookup)
                };

                StravaSegmentChallenge updatedChallenge = await _cosmosRepository.PatchItem(challenge.Id, patchOperations);

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
