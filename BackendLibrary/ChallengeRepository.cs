using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorApp.Shared;
using Flurl.Http.Configuration;
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
        public ChallengeRepository(ILogger<ChallengeRepository> logger, IConfiguration config, CosmosClient cosmosClient, CosmosDBRepository<StravaSegmentChallenge> cosmosRepository) : base(config, cosmosClient)
        {
            _logger = logger;
            _config = config;
            _cosmosRepository = cosmosRepository;   
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
            catch(Exception ex)
            {
                _logger.LogError(ex, "GetChallenge failed.");
                throw;
            }
        }

        public async Task<IList<ChallengeSegmentEffort>> GetSegmentEfforts(string challengeId)
        {
            try
            {
                IList<ChallengeSegmentEffort> efforts = await this.GetItems(se => se.ChallengeId == challengeId);
                _logger.LogInformation($"GetSegmentEfforts: challengeId = >challengeId<, efforts count = >{efforts.Count}<");
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
                _logger.LogInformation($"UpdateSegmentEffortImprovement({segmentEffort.SegmentTitle} for {segmentEffort.AthleteName} with time {segmentEffort.ElapsedTime})");
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
