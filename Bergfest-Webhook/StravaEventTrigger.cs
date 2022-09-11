using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BlazorApp.Shared;
using Bergfest_Webhook.Repositories;
using Bergfest_Webhook.Utils;


namespace Bergfest_Webhook
{
    public class StravaEventTrigger
    {
        private readonly ILogger _logger;
        private StravaRepository _stravaRepository;
        private QueueStorageRepository _queueStorageRepository;
        public StravaEventTrigger(ILogger<StravaWebhook> logger,
                             StravaRepository stravaRepository,
                             QueueStorageRepository queueRepository
                            )
        {
            _logger = logger;
            _stravaRepository = stravaRepository;
            _queueStorageRepository = queueRepository;
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
                        await ScanSegmentsInActivity(stravaEvent);
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
            await _stravaRepository.ScanSegmentsInActivity(stravaEvent.AthleteId, stravaEvent.ObjectId);
        }
    }
}
