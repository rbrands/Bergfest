using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Bergfest_Webhook.Repositories;
using Microsoft.AspNetCore.Routing;
using BlazorApp.Shared;
using Bergfest_Webhook.Utils;

namespace Bergfest_Webhook
{
    public class StravaWebhook
    {
        private readonly ILogger _logger;
        private StravaRepository _stravaRepository;

        public StravaWebhook(ILogger<StravaWebhook> logger,
                             StravaRepository stravaRepository
                            )
        {
            _logger = logger;
            _stravaRepository = stravaRepository;
        }   
        [FunctionName("ValidateStravaWebhook")]
        public async Task<IActionResult> RunValidation(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ValidateStravaWebhook")] HttpRequest req
            )
        {
            try
            {
                string verifyToken = req.Query["hub.verify_token"];
                string hubChallenge = req.Query["hub.challenge"];
                _logger.LogInformation($"Strava Webhook validation request verifyToken={verifyToken} hubChallenge={hubChallenge}");
                if (String.Compare(verifyToken, Constants.STRAVA_WEBHOOK_VERIFY_TOKEN) != 0)
                {
                    throw new Exception("Verify token doesn't match.");
                }
 
                return new OkObjectResult("hallo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuthorizeBergFestOnStrava failed.");
                return new BadRequestObjectResult(ex.Message);
            }
           
        }
         
    }
}
