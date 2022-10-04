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
    public class AddParticipantToChallenge
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<StravaSegmentChallenge> _cosmosRepository;

        public AddParticipantToChallenge(ILogger<AddParticipantToChallenge> logger,
                         CosmosDBRepository<StravaSegmentChallenge> cosmosRepository
                         )
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName("AddParticipantToChallenge")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AddParticipantToChallenge/{challengeId}")] HttpRequest req, string challengeId
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                StravaSegmentChallenge.Participant participant = JsonSerializer.Deserialize<StravaSegmentChallenge.Participant>(requestBody);
                _logger.LogInformation($"AddParticipantToChallenge(Title = {participant.AthleteName})");
                StravaSegmentChallenge challenge = await _cosmosRepository.GetItem(challengeId);
                if (null == challenge)
                {
                    throw new Exception($"AddParticipantToChallenge: No challenge with id >{challengeId}< found");
                }
                if (null == challenge.Participants)
                {
                    challenge.Participants = new Dictionary<ulong, StravaSegmentChallenge.Participant>();
                }
                challenge.Participants.Add(participant.AthleteId, participant);
                // Patch operations are only allowed for value fields or arrays but not dictionaries. Therefore write the whole
                // list/dictionary of segments. But check if the timestamp has not changed to avoid the "race" condition that
                // the dictionary was updated from another one since challenge was read
                StravaSegmentChallenge updatedChallenge = await _cosmosRepository.PatchField(challengeId, "Participants", challenge.Participants, challenge.TimeStamp);

                return new OkObjectResult(updatedChallenge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddParticipantToChallenge failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
