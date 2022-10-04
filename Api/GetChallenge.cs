using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BlazorApp.Shared;
using System.Web.Http;
using BackendLibrary;


namespace BlazorApp.Api
{
    public class GetChallenge
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private CosmosDBRepository<StravaSegmentChallenge> _cosmosRepository;

        public GetChallenge(ILogger<GetChallenge> logger,
                        IConfiguration config,
                        CosmosDBRepository<StravaSegmentChallenge> cosmosRepository
        )
        {
            _logger = logger;
            _config = config;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName(nameof(GetChallenge))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetChallenge/{challengeId}")] HttpRequest req, string challengeId)
        {
            try
            {
                if (String.IsNullOrEmpty(challengeId))
                {
                    throw new Exception("Missing challengeId for call GetChallenge()");
                }
                StravaSegmentChallenge challenge = await _cosmosRepository.GetItem(challengeId);
                if (null == challenge)
                {
                    throw new Exception($"GetChallenge(ChallengeId = >{challengeId}< not found.");
                }
                _logger.LogInformation($"GetChallenge(ChallengeId = {challengeId} Title = {challenge.ChallengeTitle}");
                return new OkObjectResult(challenge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetChallenge(challengeId = {challengeId}) failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
