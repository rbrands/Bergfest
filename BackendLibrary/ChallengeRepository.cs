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
        public async Task<StravaSegmentChallenge?> GetChallengeByTitle(string challengeTitle)
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

        public async Task<ChallengeWithEfforts> CalculateRanking(string challengeId)
        {
            try
            {
                ChallengeWithEfforts challengeWithEfforts = new ChallengeWithEfforts();
                challengeWithEfforts.Efforts = await this.GetItems(se => se.ChallengeId == challengeId);
                _logger.LogInformation($"CalculateRanking: challengeId = >{challengeId}<, efforts count = >{challengeWithEfforts.Efforts.Count}<");
                if (challengeWithEfforts.Efforts.Count > 0)
                {
                    challengeWithEfforts.Efforts = challengeWithEfforts.Efforts.OrderBy(s => s.SegmentId).ThenBy(s => s.ElapsedTime).ToList();
                }
                // Calculate ranking/points
                StravaSegmentChallenge? challenge = await _cosmosRepository.GetItem(challengeId);
                if (null == challenge)
                {
                    throw new Exception($"CalculateRanking: No challenge with >{challengeId}< found.");
                }
                challengeWithEfforts.Challenge = challenge;
                int ranking = 0;
                int counter = 0;
                long elapsedTimePred = 0;
                ulong segmentIdPred = 0;
                // Calculate ranking/points for all efforts
                foreach (var e in challengeWithEfforts.Efforts)
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
                    _logger.LogDebug($"CalculateRanking: {e.SegmentTitle} for {e.AthleteName} with rank {e.Rank}");
                    e.Rank = ranking;
                    e.RankingPoints = challenge.MapRankingToPoints(e.Rank);
                }
                // Calculate ranking/points for women
                ranking = 0;
                counter = 0;
                elapsedTimePred = 0;
                segmentIdPred = 0;
                foreach (var e in challengeWithEfforts.Efforts.Where(se => se.AthleteSex == "F"))
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
                    _logger.LogDebug($"CalculateRanking: {e.SegmentTitle} for {e.AthleteName} with rank {e.RankFemale}");
                    e.RankFemale = ranking;
                    e.RankingFemalePoints = challenge.MapRankingToPoints(e.RankFemale);
                }
                // Calculate totals
                if (challenge.Participants == null)
                {
                    challenge.Participants = new Dictionary<ulong, StravaSegmentChallenge.Participant>();
                }
                if (challenge.ParticipantsFemale == null)
                {
                    challenge.ParticipantsFemale = new Dictionary<ulong, StravaSegmentChallenge.Participant>();
                }
                foreach (var p in challenge.Participants)
                {
                    p.Value.TotalPoints = challengeWithEfforts.Efforts.Where(e => e.AthleteId == p.Value.AthleteId).Sum(e => e.RankingPoints);
                    p.Value.SegmentCounter = challengeWithEfforts.Efforts.Where(e => e.AthleteId == p.Value.AthleteId).GroupBy(s => s.SegmentId).Count();
                }
                foreach (var p in challenge.ParticipantsFemale)
                {
                    p.Value.TotalPoints = challengeWithEfforts.Efforts.Where(e => e.AthleteId == p.Value.AthleteId).Sum(e => e.RankingFemalePoints);
                    p.Value.SegmentCounter = challengeWithEfforts.Efforts.Where(e => e.AthleteId == p.Value.AthleteId).GroupBy(s => s.SegmentId).Count();
                }
                var ordered = challenge.Participants.OrderByDescending(e => e.Value.TotalPoints);
                // Calculate total ranking
                ranking = 0;
                counter = 0;
                double pointsPred = 0.0;
                foreach (var p in ordered)
                {
                    ++counter;
                    if (p.Value.TotalPoints != pointsPred)
                    {
                        ranking = counter;
                    }
                    pointsPred = p.Value.TotalPoints;
                    p.Value.Rank = ranking;
                }
                Dictionary<ulong, StravaSegmentChallenge.Participant> orderedDictionary = new Dictionary<ulong, StravaSegmentChallenge.Participant>(ordered);
                var orderedFemale = challenge.ParticipantsFemale.OrderByDescending(e => e.Value.TotalPoints);
                // Calculate total ranking for women
                ranking = 0;
                counter = 0;
                pointsPred = 0.0;
                foreach (var p in orderedFemale)
                {
                    ++counter;
                    if (p.Value.TotalPoints != pointsPred)
                    {
                        ranking = counter;
                    }
                    pointsPred = p.Value.TotalPoints;
                    p.Value.Rank = ranking;
                }
                Dictionary<ulong, StravaSegmentChallenge.Participant> orderedDictionaryFemale = new Dictionary<ulong, StravaSegmentChallenge.Participant>(orderedFemale);
                challenge.Participants = orderedDictionary;
                challenge.ParticipantsFemale = orderedDictionaryFemale;

                IReadOnlyList<PatchOperation> patchOperations = new List<PatchOperation>()
                {
                    PatchOperation.Add("/Participants", orderedDictionary),
                    PatchOperation.Add("/ParticipantsFemale", orderedDictionaryFemale)
                };
                challengeWithEfforts.Challenge = await _cosmosRepository.PatchItem(challengeId, patchOperations, challenge.TimeStamp);
                return challengeWithEfforts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CalculateRanking failed.");
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
                    // Recalculate all rankings
                    await this.CalculateRanking(segmentEffort.ChallengeId);
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
                await this.DeleteSegmentEffort(segmentEffort.ChallengeId, segmentEffort.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertSegmentEffort failed.");
                throw;
            }
        }
        public async Task DeleteSegmentEffort(string challengeId, string segmentEffortId)
        {
            try
            {
                _logger.LogInformation($"DeleteSegmentEffort({segmentEffortId})");
                await this.DeleteItemAsync(segmentEffortId);
                await this.CalculateRanking(challengeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertSegmentEffort failed.");
                throw;
            }
        }
    }
}
