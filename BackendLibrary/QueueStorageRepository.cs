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
using System.Text.Json;
using BlazorApp.Shared;
using Microsoft.Extensions.Logging;

namespace BackendLibrary
{
    public class QueueStorageRepository
    {
        public const int STRAVA_MESSAGE_VISIBILITY_TIMEOUT = 9;
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
            string connectionString = _config["STRAVA_EVENT_QUEUE_ACCOUNT"];
            string _queueName = _config["STRAVA_EVENT_QUEUE"];
            if (String.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Config value STRAVA_EVENT_QUEUE_ACCOUNT not found.");
            }
            if (String.IsNullOrEmpty(_queueName))
            {
                throw new Exception("Config value STRAVA_EVENT_QUEUE not found.");
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
                lock (this)
                {
                    _messageCounter = 0;
                }
            }
            TimeSpan visibilityTimeout = new TimeSpan(0, 0, _messageCounter * STRAVA_MESSAGE_VISIBILITY_TIMEOUT);
            _logger.LogDebug($"QueueStorageRepository.InsertMessage({messageSerialized})");
            await _queueClient.SendMessageAsync(messageSerialized, visibilityTimeout);
        }
    }
}
