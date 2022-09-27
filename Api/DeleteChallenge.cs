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
    public class DeleteChallenge
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<StravaSegmentChallenge> _cosmosRepository;
        private CosmosDBRepository<ChallengeParticipant> _cosmosParticipantRepository;

        public DeleteChallenge(ILogger<DeleteChallenge> logger,
                         CosmosDBRepository<ChallengeParticipant> cosmosParticipantRepository,
                         CosmosDBRepository<StravaSegmentChallenge> cosmosRepository)
        {
            _logger = logger;
            _cosmosParticipantRepository = cosmosParticipantRepository;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName("DeleteChallenge")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeleteChallenge")] HttpRequest req
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                StravaSegmentChallenge challenge = JsonSerializer.Deserialize<StravaSegmentChallenge>(requestBody);
                _logger.LogInformation($"DeleteChallenge({requestBody})");
                if (String.IsNullOrEmpty(challenge.Id))
                {
                    return new BadRequestErrorMessageResult("Id of StravaSegmentChallenge to be deleted is missing.");
                }
                await _cosmosRepository.DeleteItemAsync(challenge.Id);
                // Delete all participants
                IEnumerable<ChallengeParticipant> participants = await _cosmosParticipantRepository.GetItems(p => p.ChallengeId == challenge.Id);
                foreach (ChallengeParticipant p in participants)
                {
                    await _cosmosParticipantRepository.DeleteItemAsync(p.Id);
                }
                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteChallenge failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}