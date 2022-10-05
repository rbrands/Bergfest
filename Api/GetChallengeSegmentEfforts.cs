using System;
using System.Threading.Tasks;
using System.Linq;
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
    public class GetChallengeSegmentEfforts
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private ChallengeRepository _cosmosRepository;
        public GetChallengeSegmentEfforts(ILogger<GetChallengeSegmentEfforts> logger,
                        IConfiguration config,
                        ChallengeRepository cosmosRepository
        )
        {
            _logger = logger;
            _config = config;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName(nameof(GetChallengeSegmentEfforts))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetChallengeSegmentEfforts/{challengeId}")] HttpRequest req, string challengeId)
        {
            try
            {
                _logger.LogInformation($"GetChallengeSegmentEfforts({challengeId})");
                IList<ChallengeSegmentEffort> segmentEfforts = await _cosmosRepository.GetSegmentEfforts(challengeId);
                List<ChallengeSegmentEffort> orderedList = new List<ChallengeSegmentEffort>(segmentEfforts);
                return new OkObjectResult(orderedList.OrderBy(e => e.SegmentId).ThenByDescending(e => e.ElapsedTime));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetChallengeSegmentEfforts failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
