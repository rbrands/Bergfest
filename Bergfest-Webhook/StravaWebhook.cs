using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Bergfest_Webhook.Repositories;
using Microsoft.AspNetCore.Routing;
using BlazorApp.Shared;
using Bergfest_Webhook.Utils;
using System.Text.Json;
using System.Dynamic;
using Microsoft.Extensions.Configuration;

namespace Bergfest_Webhook
{
    public class StravaWebhook
    {
        private readonly ILogger _logger;
        private StravaRepository _stravaRepository;
        private IConfiguration _config;

        public StravaWebhook(ILogger<StravaWebhook> logger,
                             IConfiguration config,
                             StravaRepository stravaRepository
                            )
        {
            _logger = logger;
            _config = config;
            _stravaRepository = stravaRepository;
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
                    _logger.LogInformation($"StravaWebhook {requestBody}");
                    dynamic postRequest = JsonSerializer.Deserialize<ExpandoObject>(requestBody);
                    HandleWebhookPost(postRequest);
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

        internal void HandleWebhookPost(dynamic postRequest)
        {
            string subscriptionId = _config["STRAVA_SUBSCRIPTION_ID"];
            if (String.IsNullOrEmpty(subscriptionId))
            {
                throw new Exception("STRAVA_SUBSCRIPTION_ID not configured.");
            }
            _logger.LogInformation($"Subscription-Id expected: {subscriptionId}");
            if (subscriptionId != postRequest.subscription_id.ToString())
            {
                throw new Exception("Wrong subscription id.");
            }
            _logger.LogInformation($"Webhook for subscription_id {postRequest.subscription_id} object_id {postRequest.object_id}");
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
