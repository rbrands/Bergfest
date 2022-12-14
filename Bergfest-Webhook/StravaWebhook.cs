using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BlazorApp.Shared;
using Bergfest_Webhook.Utils;
using System.Text.Json;
using System.Dynamic;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using BackendLibrary;

namespace Bergfest_Webhook
{
    public class StravaWebhook
    {
        private readonly ILogger _logger;
        private StravaRepository _stravaRepository;
        private QueueStorageRepository _queueRepository;
        private IConfiguration _config;

        public StravaWebhook(ILogger<StravaWebhook> logger,
                             IConfiguration config,
                             StravaRepository stravaRepository,
                             QueueStorageRepository queueRepository
                            )
        {
            _logger = logger;
            _config = config;
            _stravaRepository = stravaRepository;
            _queueRepository = queueRepository;
        }   
        [FunctionName("StravaWebhook")]
        public async Task<IActionResult> Run(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "StravaWebhook")] HttpRequest req
            )
        {
            try
            {
                if (req.Method == "GET")
                { 
                    string responseSerialized = ValidateSubscrption(req);
                    return new OkObjectResult(responseSerialized);
                }
                else if (req.Method == "POST")
                {
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    _logger.LogInformation($"StravaWebHook Post Request received: {requestBody}");
                    dynamic postRequest = JsonSerializer.Deserialize<ExpandoObject>(requestBody);
                    await HandleWebhookPost(postRequest);
                    return new OkResult();
                }
                else
                {
                    throw new Exception($"StravaWebhook called with invalid method {req.Method}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StravaWebhook failed.");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        internal async Task HandleWebhookPost(dynamic postRequest)
        {
            string subscriptionId = _config["STRAVA_SUBSCRIPTION_ID"];
            Dictionary<string, string> updates = new Dictionary<string, string>();
            if (String.IsNullOrEmpty(subscriptionId))
            {
                throw new Exception("STRAVA_SUBSCRIPTION_ID not configured.");
            }
            _logger.LogDebug($"Subscription-Id expected: {subscriptionId}");
            string subscriptionIdReceived = postRequest.subscription_id.ToString();
            if (subscriptionId != subscriptionIdReceived)
            {
                throw new Exception($"Wrong subscription id. Expected >{subscriptionId}< received >{subscriptionIdReceived}<");
            }
            _logger.LogDebug($"Webhook for subscription_id {postRequest.subscription_id} object_id {postRequest.object_id}");
            StravaEvent stravaEvent = new StravaEvent();
            stravaEvent.ObjectId = Convert.ToUInt64(postRequest.object_id.ToString());
            stravaEvent.AthleteId = Convert.ToUInt64(postRequest.owner_id.ToString());
            StravaAccess stravaAccess = await _stravaRepository.GetStravaAccess(stravaEvent.AthleteId);
            if (null == stravaAccess)
            {
                _logger.LogWarning($"No data for athlete id {stravaEvent.AthleteId}");
                return;
            }
            stravaEvent.EventType = (postRequest.object_type.ToString() == "activity") ? StravaEvent.ObjectType.Activity : StravaEvent.ObjectType.Athlete;
            switch (postRequest.aspect_type.ToString())
            {
                case "create":
                    stravaEvent.Aspect = StravaEvent.AspectType.Create;
                    break;
                case "update":
                    stravaEvent.Aspect = StravaEvent.AspectType.Update;
                    break;
                case "delete":
                    stravaEvent.Aspect = StravaEvent.AspectType.Delete;
                    break;
                default:
                    break;
            }
            updates = JsonSerializer.Deserialize<Dictionary<string,string>>(postRequest.updates);
            if (stravaEvent.EventType == StravaEvent.ObjectType.Activity && stravaEvent.Aspect == StravaEvent.AspectType.Update)
            {
                if (updates.ContainsKey("private"))
                {
                    if (updates["private"] == "true")
                    {
                        stravaEvent.Aspect = StravaEvent.AspectType.Deauthorize;
                    }
                }
                if (updates.ContainsKey("title"))
                {
                    stravaEvent.ActivityName = updates["title"];
                    stravaEvent.Aspect = StravaEvent.AspectType.UpdateTitle;
                }
            }
            if (stravaEvent.EventType == StravaEvent.ObjectType.Athlete && updates.ContainsKey("authorized"))
            {
                stravaEvent.Aspect = StravaEvent.AspectType.Deauthorize;
            }
            _logger.LogInformation($"Webhook event for >{stravaAccess.GetFullName()}< - {stravaEvent.EventType} - {stravaEvent.Aspect} ");
            await _queueRepository.InsertMessage(stravaEvent);
        }
  
        internal string ValidateSubscrption(HttpRequest req)
        {
            string verifyToken = req.Query["hub.verify_token"];
            string hubChallenge = req.Query["hub.challenge"];
            _logger.LogInformation($"Strava Webhook validation request verifyToken={verifyToken} hubChallenge={hubChallenge}");
            if (String.Compare(verifyToken, Constants.STRAVA_WEBHOOK_VERIFY_TOKEN) != 0)
            {
                throw new Exception("Verify token doesn't match.");
            }
            StravaValidationResponse response = new StravaValidationResponse();
            response.HubChallenge = hubChallenge;
            string responseSerialized = JsonSerializer.Serialize(response);
            _logger.LogInformation(responseSerialized);
            return responseSerialized;
        }
    }
}
