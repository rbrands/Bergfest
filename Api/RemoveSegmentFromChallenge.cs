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
    public class RemoveSegmentFromChallenge
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<StravaSegmentChallenge> _cosmosRepository;
        private CosmosDBRepository<StravaSegment> _cosmosSegmentRepository;

        public RemoveSegmentFromChallenge(ILogger<RemoveSegmentFromChallenge> logger,
                         CosmosDBRepository<StravaSegmentChallenge> cosmosRepository,
                         CosmosDBRepository<StravaSegment> cosmosSegmentRepository
                         )
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
            _cosmosSegmentRepository = cosmosSegmentRepository;
        }

        [FunctionName("RemoveSegmentFromChallenge")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "RemoveSegmentFromChallenge/{challengeId}")] HttpRequest req, string challengeId
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                StravaSegmentChallenge.Segment segment = JsonSerializer.Deserialize<StravaSegmentChallenge.Segment>(requestBody);
                _logger.LogInformation($"RemoveSegmentFromChallenge(Title = {segment.SegmentName})");
                StravaSegmentChallenge challenge = await _cosmosRepository.GetItem(challengeId);
                if (null == challenge)
                {
                    throw new Exception($"RemoveSegmentFromChallenge: No challenge with id >{challengeId}< found");
                }
                if (null == challenge.Segments)
                {
                    challenge.Segments = new Dictionary<ulong, StravaSegmentChallenge.Segment>();
                }
                challenge.Segments.Remove(segment.SegmentId);
                // Patch operations are only allowed for value fields or arrays but not dictionaries. Therefore write the whole
                // list/dictionary of segments. But check if the timestamp has not changed to avoid the "race" condition that
                // the dictionary was updated from another one since challenge was read
                StravaSegmentChallenge updatedChallenge = await _cosmosRepository.PatchField(challengeId, "Segments", challenge.Segments, challenge.TimeStamp);

                // Add challenge to segment for faster scanning for segment efforts
                StravaSegment stravaSegment = await _cosmosSegmentRepository.GetItemByKey(segment.SegmentId.ToString());
                if (null == stravaSegment)
                {
                    throw new Exception($"AddSegmentToChallenge: Strava-Segment {segment.SegmentId} not found.");
                }
                if (null == stravaSegment.Challenges)
                {
                    stravaSegment.Challenges = new Dictionary<string, StravaSegment.Challenge>();
                }
                stravaSegment.Challenges.Remove(challengeId);
                await _cosmosSegmentRepository.PatchField(stravaSegment.Id, "Challenges", stravaSegment.Challenges, stravaSegment.TimeStamp);

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
