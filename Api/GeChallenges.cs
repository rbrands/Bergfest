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
using System.Collections.Generic;
using BackendLibrary;


namespace BlazorApp.Api
{
    public class GetChallenges
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private CosmosDBRepository<StravaSegmentChallenge> _cosmosRepository;

        public GetChallenges(ILogger<GetChallenge> logger,
                        IConfiguration config,
                        CosmosDBRepository<StravaSegmentChallenge> cosmosRepository
        )
        {
            _logger = logger;
            _config = config;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName(nameof(GetChallenges))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetChallenges")] HttpRequest req)
        {
            try
            {
                IList<StravaSegmentChallenge> challenges = await _cosmosRepository.GetItems();
                _logger.LogInformation($"GetChallenges()");
                return new OkObjectResult(challenges);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetChallengea() failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
