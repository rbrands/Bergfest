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
    public class GetChallengeByTitle
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private ChallengeRepository _cosmosRepository;

        public GetChallengeByTitle(ILogger<GetChallengeByTitle> logger,
                        IConfiguration config,
                        ChallengeRepository cosmosRepository
        )
        {
            _logger = logger;
            _config = config;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName(nameof(GetChallengeByTitle))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetChallengeByTitle/{challengeTitle}")] HttpRequest req, string challengeTitle)
        {
            try
            {
                if (String.IsNullOrEmpty(challengeTitle))
                {
                    throw new Exception("Missing challengeTitle for call GetChallenge()");
                }
                StravaSegmentChallenge challenge = await _cosmosRepository.GetChallengeByTitle(challengeTitle);
                return new OkObjectResult(challenge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetChallenge(challengeId = {challengeTitle}) failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
