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
using BlazorApp.Api.Repositories;
using BlazorApp.Api.Utils;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace BlazorApp.Api
{
    public class RemoveSegmentFromChallenge
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<StravaSegmentChallenge> _cosmosRepository;

        public RemoveSegmentFromChallenge(ILogger<RemoveSegmentFromChallenge> logger,
                         CosmosDBRepository<StravaSegmentChallenge> cosmosRepository
                         )
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
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
                List<PatchOperation> patchOperations = new()
                {
                    PatchOperation.Add($"/Segments", challenge.Segments)
                };

                StravaSegmentChallenge updatedChallenge = await _cosmosRepository.PatchItem(challengeId, patchOperations, challenge.TimeStamp);

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
