using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorApp.Shared;
using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackendLibrary
{
    public class ChallengeRepository : CosmosDBRepository<ChallengeSegmentEffort>
    {
        private readonly ILogger _logger;
        private IConfiguration _config;
        private CosmosDBRepository<StravaSegmentChallenge> _cosmosRepository;
        private CosmosDBRepository<StravaSegment> _cosmosSegmentRepository;
        public ChallengeRepository(ILogger<ChallengeRepository> logger, IConfiguration config, CosmosClient cosmosClient, 
                                   CosmosDBRepository<StravaSegmentChallenge> cosmosRepository,
                                   CosmosDBRepository<StravaSegment> cosmosSegmentRepository) 
                                   : base(config, cosmosClient)
        {
            _logger = logger;
            _config = config;
            _cosmosRepository = cosmosRepository;
            _cosmosSegmentRepository = cosmosSegmentRepository;
        }
        public async Task<StravaSegmentChallenge> GetChallenge(string challengeId)
        {
            try
            {
                StravaSegmentChallenge? challenge = await _cosmosRepository.GetItem(challengeId);
                if (null == challenge)
                {
                    throw new Exception($"GetChallenge: No challenge with id >{challengeId}< found.");
                }
                _logger.LogInformation($"GetChallenge(Title = {challenge.ChallengeTitle})");
                return challenge;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetChallenge failed.");
                throw;
            }
        }
        public async Task<StravaSegmentChallenge> GetChallengeByTitle(string challengeTitle)
        {
            try
            {
                if (String.IsNullOrEmpty(challengeTitle))
                {
                    throw new Exception("Missing challengeTitle for call GetChallengeByTitle()");
                }
                string challengeTitleLowerCase = challengeTitle.ToLowerInvariant();
                _logger.LogInformation($"GetChallengeByTitle(ChallengeTitle = {challengeTitleLowerCase}");
                StravaSegmentChallenge? challenge = await _cosmosRepository.GetFirstItemOrDefault(c => c.UrlTitle == challengeTitleLowerCase);
                if (null == challenge)
                {
                    throw new Exception($"Challenge {challengeTitleLowerCase} not found.");
                }
                return challenge;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetChallengeByTitle failed.");
                throw;
            }
        }
        public async Task<StravaSegmentChallenge> UpdateChallenge(StravaSegmentChallenge challenge)
        {
            try
            {
                _logger.LogInformation($"UpdateChallenge(Title = {challenge.ChallengeTitle})");
                if (String.IsNullOrEmpty(challenge.UrlTitle))
                {
                    challenge.UrlTitle = challenge.GetUrlFriendlyTitle();
                }
                List<PatchOperation> patchOperations = new()
                {
                    PatchOperation.Add("/ChallengeTitle", challenge.ChallengeTitle),
                    PatchOperation.Add("/Description", challenge.Description),
                    PatchOperation.Add("/ImageLink", challenge.ImageLink),
                    PatchOperation.Add("/UrlTitle", challenge.UrlTitle),
                    PatchOperation.Add("/StartDateUTC", challenge.StartDateUTC),
                    PatchOperation.Add("/EndDateUTC", challenge.EndDateUTC),
                    PatchOperation.Add("/IsPublicVisible", challenge.IsPublicVisible),
                    PatchOperation.Add("/InvitationRequired", challenge.InvitationRequired),
                    PatchOperation.Add("/RegistrationIsOpen", challenge.RegistrationIsOpen),
                    PatchOperation.Add("/InvitationLink", challenge.InvitationLink),
                    PatchOperation.Add("/PointLookup", challenge.PointLookup)
                };

                StravaSegmentChallenge updatedChallenge = await _cosmosRepository.PatchItem(challenge.Id, patchOperations);
                // Update all challenge references in the segments
                foreach (var s in challenge.Segments)
                {
                    await UpdateChallengeInSegment(updatedChallenge, s.Value.SegmentId);
                }
                return updatedChallenge;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateChallenge failed.");
                throw;
            }
        }
        public async Task UpdateChallengeInSegment(StravaSegmentChallenge challenge, ulong segmentId)
        {
            StravaSegment? stravaSegment = await _cosmosSegmentRepository.GetItemByKey(segmentId.ToString());
            if (null == stravaSegment)
            {
                throw new Exception($"UpdateChallengeInSegment: Strava-Segment {segmentId} not found.");
            }
            if (null == stravaSegment.Challenges)
            {
                stravaSegment.Challenges = new Dictionary<string, StravaSegment.Challenge>();
            }
            StravaSegment.Challenge challengeInSegment = new StravaSegment.Challenge()
            {
                Id = challenge.Id,
                ChallengeTitle = challenge.ChallengeTitle,
                StartDateUTC = challenge.StartDateUTC,
                EndDateUTC = challenge.EndDateUTC
            };
            stravaSegment.Challenges[challenge.Id] = challengeInSegment;
            await _cosmosSegmentRepository.PatchField(stravaSegment.Id, "Challenges", stravaSegment.Challenges, stravaSegment.TimeStamp);

        }

        public async Task<IList<ChallengeSegmentEffort>> GetSegmentEfforts(string challengeId)
        {
            try
            {
                IList<ChallengeSegmentEffort> efforts = await this.GetItems(se => se.ChallengeId == challengeId);
                _logger.LogInformation($"GetSegmentEfforts: challengeId = >challengeId<, efforts count = >{efforts.Count}<");
                efforts = efforts.OrderBy(s => s.SegmentId).ThenBy(s => s.ElapsedTime).ToList();
                // Calculate ranking/points
                int ranking = 0;
                int counter = 0;
                long elapsedTimePred = 0;
                ulong segmentIdPred = 0;
                foreach (var e in efforts)
                {
                    if (segmentIdPred != 0 && segmentIdPred != e.SegmentId)
                    {
                        ranking = 0;
                        counter = 0;
                        elapsedTimePred = 0;
                    }
                    segmentIdPred = e.SegmentId;
                    ++counter;
                    if (e.ElapsedTime > elapsedTimePred)
                    {
                        ranking = counter;
                    }
                    elapsedTimePred = e.ElapsedTime;
                    e.Rank = ranking;
                }
                // Calculate ranking/points for women
                ranking = 0;
                counter = 0;
                elapsedTimePred = 0;
                segmentIdPred = 0;
                foreach (var e in efforts.Where(se => se.AthleteSex == "F"))
                {
                    if (segmentIdPred != 0 && segmentIdPred != e.SegmentId)
                    {
                        ranking = 0;
                        counter = 0;
                        elapsedTimePred = 0;
                    }
                    segmentIdPred = e.SegmentId;
                    ++counter;
                    if (e.ElapsedTime > elapsedTimePred)
                    {
                        ranking = counter;
                    }
                    elapsedTimePred = e.ElapsedTime;
                    e.RankFemale = ranking;
                }

                return efforts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSegmentEfforts failed.");
                throw;
            }
        }
        public async Task<ChallengeSegmentEffort?> UpdateSegmentEffortImprovement(ChallengeSegmentEffort segmentEffort)
        {
            try
            {
                segmentEffort.LogicalKey = $"{segmentEffort.ChallengeId}-{segmentEffort.AthleteId}-{segmentEffort.SegmentId}";
                // Check if there is already a time for the segment stored and update this one if the time has been improved
                ChallengeSegmentEffort? effortInStock = await this.GetItemByKey(segmentEffort.LogicalKey);
                if (null == effortInStock || segmentEffort.ElapsedTime < effortInStock.ElapsedTime)
                {
                    _logger.LogInformation($"UpdateSegmentEffortImprovement({segmentEffort.SegmentTitle} for {segmentEffort.AthleteName} with time {segmentEffort.ElapsedTime})");
                    ChallengeSegmentEffort updatedEffort = await UpsertItem(segmentEffort);
                    return updatedEffort;
                }
                return segmentEffort;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSegmentEffort failed.");
                throw;
            }
        }
        public async Task<ChallengeSegmentEffort> UpsertSegmentEffort(ChallengeSegmentEffort segmentEffort)
        {
            try
            {
                _logger.LogInformation($"UpsertSegmentEffort({segmentEffort.SegmentTitle} for {segmentEffort.AthleteName} with time {segmentEffort.ElapsedTime})");
                segmentEffort.LogicalKey = $"{segmentEffort.ChallengeId}-{segmentEffort.AthleteId}-{segmentEffort.SegmentId}";
                return await this.UpsertItem(segmentEffort);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertSegmentEffort failed.");
                throw;
            }
        }
        public async Task DeleteSegmentEffort(ChallengeSegmentEffort segmentEffort)
        {
            try
            {
                _logger.LogInformation($"DeleteSegmentEffort({segmentEffort.SegmentTitle} for {segmentEffort.AthleteName} with time {segmentEffort.ElapsedTime})");
                segmentEffort.LogicalKey = $"{segmentEffort.ChallengeId}-{segmentEffort.AthleteId}-{segmentEffort.SegmentId}";
                await this.DeleteItemAsync(segmentEffort.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertSegmentEffort failed.");
                throw;
            }
        }
    }
}
