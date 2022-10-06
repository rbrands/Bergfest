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
using BackendLibrary;


namespace BlazorApp.Api
{
    public class DeleteChallenge
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<StravaSegmentChallenge> _cosmosRepository;
        private ChallengeRepository _challengeRepository;
        private CosmosDBRepository<StravaSegment> _cosmosSegmentRepository;

        public DeleteChallenge(ILogger<DeleteChallenge> logger,
                         CosmosDBRepository<StravaSegmentChallenge> cosmosRepository,
                         ChallengeRepository challengeRepository,
                         CosmosDBRepository<StravaSegment> cosmosSegmentRepository)
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
            _challengeRepository = challengeRepository;
            _cosmosSegmentRepository = cosmosSegmentRepository;
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
                // Delete all challenge segment efforts
                IEnumerable<ChallengeSegmentEffort> efforts = await _challengeRepository.GetItems(e => e.ChallengeId == challenge.Id);
                foreach (var e in efforts)
                {
                    await _challengeRepository.DeleteItemAsync(e.Id);
                }
                // Delete challenge from all segments
                IEnumerable<StravaSegment> _segments = await _cosmosSegmentRepository.GetItems();
                foreach (var segment in _segments)
                {
                    if (null != segment.Challenges && segment.Challenges.ContainsKey(challenge.Id))
                    {
                        segment.Challenges.Remove(challenge.Id);
                        await _cosmosSegmentRepository.PatchField(segment.Id, "Challenges", segment.Challenges, segment.TimeStamp);
                    }
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
