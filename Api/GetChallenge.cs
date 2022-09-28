using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BlazorApp.Shared;
using System.Web.Http;
using Flurl;
using Flurl.Http.Configuration;
using Flurl.Http;
using Flurl.Http.Content;
using System.Collections.Generic;
using BlazorApp.Api.Repositories;
using BlazorApp.Api.Utils;

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
                StravaSegmentChallenge challenge = await _cosmosRepository.GetItem(challengeId.ToString());
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
