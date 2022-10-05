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
using System.Collections.Generic;
using BackendLibrary;


namespace BlazorApp.Api
{
    public class AddSegmentToChallenge
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<StravaSegmentChallenge> _cosmosRepository;
        private CosmosDBRepository<StravaSegment> _cosmosSegmentRepository;
        private ChallengeRepository _challengeRepository;

        public AddSegmentToChallenge(ILogger<AddSegmentToChallenge> logger,
                         CosmosDBRepository<StravaSegmentChallenge> cosmosRepository,
                         CosmosDBRepository<StravaSegment> cosmosSegmentRepoitory,
                         ChallengeRepository challengeRepository
                         )
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
            _cosmosSegmentRepository = cosmosSegmentRepoitory;
            _challengeRepository = challengeRepository;
        }

        [FunctionName("AddSegmentToChallenge")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AddSegmentToChallenge/{challengeId}")] HttpRequest req, string challengeId
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                StravaSegmentChallenge.Segment segment = JsonSerializer.Deserialize<StravaSegmentChallenge.Segment>(requestBody);
                _logger.LogInformation($"AddSegmentToChallenge(Title = {segment.SegmentName})");
                StravaSegmentChallenge challenge = await _challengeRepository.GetChallenge(challengeId);;
                if (null == challenge)
                {
                    throw new Exception($"AddSegmentToChallenge: No challenge with id >{challengeId}< found");
                }
                if (null == challenge.Segments)
                {
                    challenge.Segments = new Dictionary<ulong, StravaSegmentChallenge.Segment>();
                }
                challenge.Segments[segment.SegmentId] = segment;
                // Patch operations are only allowed for value fields or arrays but not dictionaries. Therefore write the whole
                // list/dictionary of segments. But check if the timestamp has not changed to avoid the "race" condition that
                // the dictionary was updated from another one since challenge was read
                StravaSegmentChallenge updatedChallenge = await _cosmosRepository.PatchField(challengeId, "Segments", challenge.Segments, challenge.TimeStamp);

                // Add challenge to segment for faster scanning for segment efforts
                await _challengeRepository.UpdateChallengeInSegment(updatedChallenge, segment.SegmentId);

                return new OkObjectResult(updatedChallenge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddSegmentToChallenge failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
