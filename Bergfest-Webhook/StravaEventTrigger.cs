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
        public StravaEventTrigger(ILogger<StravaWebhook> logger,
                             StravaRepository stravaRepository
                            )
        {
            _logger = logger;
            _stravaRepository = stravaRepository;
        }

        [FunctionName("StravaEventTrigger")]
        public async Task Run([QueueTrigger("stravaeventqueue", Connection = "AzureWebJobsStorage")]StravaEvent myQueueItem)
        {
            try
            { 
                _logger.LogInformation($"StravaEventTrigger: {myQueueItem.EventType} - {myQueueItem.Aspect} for athlete {myQueueItem.AthleteId} with object {myQueueItem.ObjectId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StravaEventTrigger failed.");
                throw;
            }

        }
    }
}
