using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BlazorApp.Shared;
using Bergfest_Webhook.Repositories;
using Bergfest_Webhook.Utils;
using Flurl;
using Flurl.Http.Configuration;
using Flurl.Http;
using System.Linq;

namespace Bergfest_Webhook
{
    public class StravaEventTrigger
    {
        private readonly ILogger _logger;
        private StravaRepository _stravaRepository;
        private QueueStorageRepository _queueStorageRepository;
        private CosmosDBRepository<StravaSegmentEffort> _segmentEffortsRepository;
        private CosmosDBRepository<StravaSegment> _segmentRepository;
        private const string STRAVA_API_ENDPOINT = "https://www.strava.com/api/v3";
        private readonly IFlurlClient _flurlClient;

        public StravaEventTrigger(ILogger<StravaWebhook> logger,
                             StravaRepository stravaRepository,
                             QueueStorageRepository queueRepository,
                             CosmosDBRepository<StravaSegmentEffort> segmentEffortsRepository,
                             CosmosDBRepository<StravaSegment> segmentRepository,
                             IFlurlClientFactory flurlClientFactory
                            )
        {
            _logger = logger;
            _stravaRepository = stravaRepository;
            _queueStorageRepository = queueRepository;
            _segmentEffortsRepository = segmentEffortsRepository;
            _segmentRepository = segmentRepository;
            _flurlClient = flurlClientFactory.Get(STRAVA_API_ENDPOINT);
        }

        [FunctionName("StravaEventTrigger")]
        public async Task Run([QueueTrigger("stravaeventqueue", Connection = "AzureWebJobsStorage")]string myQueueItem)
        {
            try
            {
                _queueStorageRepository.DecrementMessageCounter();
                StravaEvent stravaEvent = JsonSerializer.Deserialize<StravaEvent>(myQueueItem);
                _logger.LogInformation($"StravaEventTrigger: {stravaEvent.EventType} - {stravaEvent.Aspect} for athlete {stravaEvent.AthleteId} with object {stravaEvent.ObjectId}");
                switch (stravaEvent.EventType)
                {
                    case StravaEvent.ObjectType.Activity:
                        if (stravaEvent.Aspect == StravaEvent.AspectType.Deauthorize || stravaEvent.Aspect == StravaEvent.AspectType.Delete)
                        {
                            await DeleteSegmentEffortsForActivity(stravaEvent);
                        }
                        else
                        {
                            await ScanSegmentsInActivity(stravaEvent);
                        }
                        break;
                    case StravaEvent.ObjectType.Athlete:
                        if (stravaEvent.Aspect == StravaEvent.AspectType.Deauthorize)
                        {
                            await DeAuthorize(stravaEvent);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StravaEventTrigger failed.");
                throw;
            }
        }
        public async Task ScanSegmentsInActivity(StravaEvent stravaEvent)
        {
            _logger.LogInformation($"ScanSegmentsInActivity activityId {stravaEvent.ObjectId} athleteId {stravaEvent.AthleteId}");
            string accessToken = await _stravaRepository.GetAccessToken(stravaEvent.AthleteId);
            dynamic response = await _flurlClient.Request("activities", stravaEvent.ObjectId)
                                            .SetQueryParam("include_all_efforts", "true")
                                            .WithOAuthBearerToken(accessToken)
                                            .GetJsonAsync();
            _logger.LogInformation($"Activity name {response.name} {response.segment_efforts}");
            IList<Object> segmentEfforts = response.segment_efforts;
            IEnumerable<StravaSegment> segments = await _segmentRepository.GetItems();
            foreach (dynamic segmentEffort in segmentEfforts)
            {
                // TODO: Filter segments applied with list of segments of interest
                if (segmentEffort.segment.id == 3730649 || segmentEffort.segment.id == 20350376 || null != segments.FirstOrDefault(s => s.SegmentId == segmentEffort.segment_id))
                {
                    StravaSegmentEffort stravaSegmentEffort = new StravaSegmentEffort()
                    {
                        SegmentId = segmentEffort.segment.id,
                        SegmentName = segmentEffort.segment.name,
                        AthleteId = stravaEvent.AthleteId,
                        ActivityId = stravaEvent.ObjectId,
                        ElapsedTime = segmentEffort.elapsed_time,
                        StartDateLocal = segmentEffort.start_date_local,
                        TimeToLive = Constants.STRAVA_TTL_SEGMENT_EFFORT
                    };
                    await _segmentEffortsRepository.UpsertItem(stravaSegmentEffort);
                    _logger.LogInformation($"Segment {stravaSegmentEffort.SegmentId} - {stravaSegmentEffort.SegmentName} - {stravaSegmentEffort.StartDateLocal} - {stravaSegmentEffort.ElapsedTime}s");
                }
            }
        }

        public async Task DeAuthorize(StravaEvent stravaEvent)
        {
            _logger.LogInformation($"DeAuthorize athleteId {stravaEvent.AthleteId}");
            await _stravaRepository.DeAuthorize(stravaEvent.AthleteId);
            // Delete all segment efforts for athlete
            IEnumerable<StravaSegmentEffort> segmentEfforts = await _segmentEffortsRepository.GetItems(se => se.AthleteId == stravaEvent.AthleteId);
            foreach (StravaSegmentEffort se in segmentEfforts)
            {
                await _segmentEffortsRepository.DeleteItemAsync(se.Id);
            }
        }
        public async Task DeleteSegmentEffortsForActivity(StravaEvent stravaEvent)
        {
            _logger.LogInformation($"DeleteSegmentEffortsForActivity activityId {stravaEvent.ObjectId} athleteId {stravaEvent.AthleteId}");
            // Delete all segment efforts for given activity
            IEnumerable<StravaSegmentEffort> segmentEfforts = await _segmentEffortsRepository.GetItems(se => se.ActivityId == stravaEvent.ObjectId);
            foreach (StravaSegmentEffort se in segmentEfforts)
            {
                await _segmentEffortsRepository.DeleteItemAsync(se.Id);
            }
        }
    }
}
