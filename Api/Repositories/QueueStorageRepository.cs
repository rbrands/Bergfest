using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Azure.Storage.Queues; // Namespace for Queue storage types
using Azure.Storage.Queues.Models; // Namespace for PeekedMessage
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using BlazorApp.Api.Utils;
using Microsoft.VisualBasic;
using System.Text.Json;
using Constants = BlazorApp.Api.Utils.Constants;
using BlazorApp.Shared;
using Microsoft.Extensions.Logging;

namespace BlazorApp.Api.Repositories
{
    public class QueueStorageRepository
    {
        private IConfiguration _config;
        private QueueClient _queueClient;
        private ILogger _logger;
        private int _messageCounter = 0;
        /// <summary>
        /// Create repository, typically as singleton. Create CosmosClient before.
        /// </summary>
        /// <param name="cosmosClient"></param>
        /// <param name="config"></param>
        public QueueStorageRepository(IConfiguration config, ILogger<QueueStorageRepository> logger)
        {
            _config = config;
            _logger = logger;
            string connectionString = _config["AzureWebJobsStorage"];
            string _queueName = _config["STRAVA_EVENT_QUEUE"];
            if (String.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Config value AzureWebJobsStorage not found.");
            }
            QueueClientOptions queueClientOptions = new QueueClientOptions()
            {
                MessageEncoding = QueueMessageEncoding.Base64,
                Retry = { MaxRetries = 2 }
            };
            _queueClient = new QueueClient(connectionString, _queueName, queueClientOptions);
            // Create the queue
            _queueClient.CreateIfNotExists();
        }
        public int DecrementMessageCounter() 
        {
            Interlocked.Decrement(ref _messageCounter);
            return _messageCounter;
        }
        public int IncrementMessageCounter()
        {
            Interlocked.Increment(ref _messageCounter);
            return _messageCounter;
        }

        public async Task InsertMessage(StravaEvent stravaEvent)
        {
            string messageSerialized = JsonSerializer.Serialize<StravaEvent>(stravaEvent);
            IncrementMessageCounter();
            if (_messageCounter < 0)
            {
                lock(this)
                { 
                    _messageCounter = 0;
                }
            }
            TimeSpan visibilityTimeout = new TimeSpan(0, 0, _messageCounter * Constants.STRAVA_MESSAGE_VISIBILITY_TIMEOUT);
            _logger.LogDebug($"QueueStorageRepository.InsertMessage({messageSerialized})");
            await _queueClient.SendMessageAsync(messageSerialized, visibilityTimeout);
        }
    }
}
