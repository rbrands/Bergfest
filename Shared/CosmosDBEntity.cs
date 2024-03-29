﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class CosmosDBEntity
    {
        [JsonProperty(PropertyName = "id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "type")]
        [JsonPropertyName("type")]
        public string Type
        {
            get
            {
                return this.GetType().Name;
            }
            set
            {

            }
        }
        [JsonProperty(PropertyName = "partitionKey")]
        [JsonPropertyName("partitionKey")]
        public string PartitionKey
        {
            get
            {
                return this.Type;
            }
            set
            {

            }
        }
        /// <summary>
        /// Logical key to be used for this entity. If this member is set the id of the document
        /// is created as according to the pattern type-key and should be unique.
        /// </summary>
        [JsonProperty(PropertyName = "key", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("key")]
        public string LogicalKey { get; set; }
        // used to set Time-to-live for expiration policy
        [JsonProperty(PropertyName = "ttl", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("ttl")]
        public int? TimeToLive { get; set; }
        [JsonProperty(PropertyName = "tenant", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("tenant")]
        public string Tenant { get; set; }
        [JsonProperty(PropertyName = "_ts")]
        [JsonPropertyName("_ts")]
        public string TimeStamp { get; set; }
        public void SetUniqueKey()
        {
            if (null != LogicalKey)
            {
                Id = Type + "-" + LogicalKey;
            }
            else
            {
                Id = Guid.NewGuid().ToString();
            }
        }
    }
}
