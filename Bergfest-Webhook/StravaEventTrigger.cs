using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BlazorApp.Shared;
using Bergfest_Webhook.Utils;
using Flurl;
using Flurl.Http.Configuration;
using Flurl.Http;
using BackendLibrary;

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
        public async Task Run([QueueTrigger("%STRAVA_EVENT_QUEUE%", Connection = "AzureWebJobsStorage")]string myQueueItem)
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
                            if (stravaEvent.Aspect == StravaEvent.AspectType.UpdateTitle)
                            {
                                await UpdateActivityTitle(stravaEvent);
                            }
                            else
                            { 
                                await ScanSegmentsInActivity(stravaEvent);
                            }
                        }
                        break;
                    case StravaEvent.ObjectType.Athlete:
                        if (stravaEvent.Aspect == StravaEvent.AspectType.Deauthorize)
                        {
                            await DeAuthorize(stravaEvent);
                        }
                        else if (stravaEvent.Aspect == StravaEvent.AspectType.Update)
                        {
                            await UpdateAthlete(stravaEvent);
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
            StravaAccess stravaAccess = await _stravaRepository.GetAccessToken(stravaEvent.AthleteId);
            try
            {
                dynamic response = await _flurlClient.Request("activities", stravaEvent.ObjectId)
                                                .SetQueryParam("include_all_efforts", "true")
                                                .WithOAuthBearerToken(stravaAccess.AccessToken)
                                                .GetJsonAsync();
                _logger.LogInformation($"ScanSegmentsInActivity >{response.name}< athleteId {stravaEvent.AthleteId} - {stravaAccess.GetFullName()}");
                IList<Object> segmentEfforts = response.segment_efforts;
                // Get all defined segments and load them into a dictionary for faster lookup
                // TODO: Store dictionary and refresh regularly e.g. every 30 minutes
                IEnumerable<StravaSegment> segments = await _segmentRepository.GetItems();
                Dictionary<ulong, StravaSegment> segmentLookup = new Dictionary<ulong, StravaSegment>();
                foreach (StravaSegment s in segments)
                {
                    segmentLookup.Add(s.SegmentId, s);
                }
                _logger.LogDebug($"{segmentLookup.Count} segments configured.");
                foreach (dynamic segmentEffort in segmentEfforts)
                {
                    // Filter segments applied with list of segments of interest
                    StravaSegment stravaSegment = null;
                    ulong segmentId = (ulong)segmentEffort.segment.id;
                    bool segmentFound = segmentLookup.TryGetValue(segmentId, out stravaSegment);
                    if (segmentFound && stravaSegment.IsEnabled)
                    {
                        StravaSegmentEffort stravaSegmentEffort = new StravaSegmentEffort()
                        {
                            SegmentEffortId = (ulong)segmentEffort.id,
                            SegmentId = segmentId,
                            SegmentName = segmentEffort.segment.name,
                            AthleteId = stravaEvent.AthleteId,
                            AthleteName = stravaAccess.GetFullName(),
                            ProfileImageLink = stravaAccess.ProfileImageLink,
                            AthleteSex = stravaAccess.Sex,
                            ActivityId = stravaEvent.ObjectId,
                            ActivityName = response.name,
                            ElapsedTime = segmentEffort.elapsed_time,
                            StartDateLocal = segmentEffort.start_date_local,
                            TimeToLive = Constants.STRAVA_TTL_SEGMENT_EFFORT
                        };
                        stravaSegmentEffort.LogicalKey = stravaSegmentEffort.SegmentEffortId.ToString();
                        await _segmentEffortsRepository.UpsertItem(stravaSegmentEffort);
                        _logger.LogInformation($"SegmentEffort {stravaSegmentEffort.Id} - {stravaSegmentEffort.SegmentName} - {stravaSegmentEffort.StartDateLocal} - {stravaSegmentEffort.ElapsedTime}s");
                    }
                }
            }
            catch (FlurlHttpException ex)
            {
                // Ignore status if an activity is not found on Strava - this might happen because events are fired with delay and
                // users may deleted/set the privacy of an activity in the meantime
                if (null != ex.StatusCode && ex.StatusCode == (int)System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"ScanSegmentsInActivity activity not found. AthleteId: {stravaEvent.AthleteId} ActivityId: {stravaEvent.ObjectId} Aspect: {stravaEvent.Aspect} - {ex.Message}");
                }
                else
                {
                    _logger.LogError($"ScanSegmentsInActivity failed in Http request. AthleteId: {stravaEvent.AthleteId} ActivityId: {stravaEvent.ObjectId} Aspect: {stravaEvent.Aspect} - {ex.Message}");
                    throw;
                }
            }
        }
        public async Task UpdateAthlete(StravaEvent stravaEvent)
        {
            StravaAccess stravaAccess = await _stravaRepository.GetAccessToken(stravaEvent.AthleteId);
            dynamic response = await _flurlClient.Request("athlete")
                                            .WithOAuthBearerToken(stravaAccess.AccessToken)
                                            .GetJsonAsync();
            _logger.LogInformation($"UpdateAthlete athleteId {stravaEvent.AthleteId} - {stravaAccess.GetFullName()}");
            stravaAccess.FirstName = response.firstname;
            stravaAccess.LastName = response.lastname;
            stravaAccess.ProfileImageLink = response.profile;
            stravaAccess.ProfileSmallImageLink = response.profile_medium;
            stravaAccess.Sex = response.sex;
            await _stravaRepository.UpsertItem(stravaAccess);
        }
        public async Task UpdateActivityTitle(StravaEvent stravaEvent)
        {
            _logger.LogInformation($"UpdateActivityTitle >{stravaEvent.ActivityName}< athleteId {stravaEvent.AthleteId}");
            IEnumerable<StravaSegmentEffort> segmentEfforts = await _segmentEffortsRepository.GetItems(s => s.ActivityId == stravaEvent.ObjectId);
            foreach (StravaSegmentEffort segmentEffort in segmentEfforts)
            {
                await _segmentEffortsRepository.PatchField(segmentEffort.Id, "ActivityName", stravaEvent.ActivityName);
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
