using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Queues; // Namespace for Queue storage types
using Azure.Storage.Queues.Models; // Namespace for PeekedMessage
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Bergfest_Webhook.Utils;
using Microsoft.VisualBasic;
using System.Text.Json;
using Constants = Bergfest_Webhook.Utils.Constants;
using BlazorApp.Shared;
using Microsoft.Extensions.Logging;

namespace Bergfest_Webhook.Repositories
{
    public class QueueStorageRepository
    {
        private IConfiguration _config;
        private QueueClient _queueClient;
        private ILogger _logger;
        /// <summary>
        /// Create repository, typically as singleton. Create CosmosClient before.
        /// </summary>
        /// <param name="cosmosClient"></param>
        /// <param name="config"></param>
        public QueueStorageRepository(IConfiguration config, ILogger logger)
        {
            _config = config;
            _logger = logger;
            string connectionString = _config["AzureWebJobsStorage"];
            if (String.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Config value AzureWebJobsStorage not found.");
            }
            _queueClient = new QueueClient(connectionString, Constants.QUEUE_NAME);
            // Create the queue
            _queueClient.CreateIfNotExists();
        }

        public async Task InsertMessage(StravaEvent stravaEvent)
        {
            string messageSerialized = JsonSerializer.Serialize<StravaEvent>(stravaEvent);
            TimeSpan visibilityTimeout = new TimeSpan(0, 0, Constants.STRAVA_MESSAGE_VISIBILITY_TIMEOUT);
            _logger.LogInformation($"QueueStorageRepository.InsertMessage({messageSerialized})");
            await _queueClient.SendMessageAsync(messageSerialized, visibilityTimeout);
        }
    }
}
