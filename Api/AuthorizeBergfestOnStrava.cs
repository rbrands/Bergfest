using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BlazorApp.Api.Repositories;
using Microsoft.AspNetCore.Routing;
using BlazorApp.Shared;

namespace Bergfest.Api
{
    public class AuthorizeBergfestOnStrava
    {
        private readonly ILogger _logger;
        private StravaRepository _stravaRepository;
        public AuthorizeBergfestOnStrava(ILogger<AuthorizeBergfestOnStrava> logger,
                                         StravaRepository stravaRepository
                                         )
        {
            _logger = logger;
            _stravaRepository = stravaRepository;
        }

        [FunctionName("AuthorizeBergfestOnStrava")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AuthorizeBergfestOnStrava")] HttpRequest req
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var serializeOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };
                StravaAuthorization stravaAuthorization = JsonSerializer.Deserialize<StravaAuthorization>(requestBody, serializeOptions);

                _logger.LogInformation($"Authorize Bergfest on Strava for athlete {stravaAuthorization.Code}");
                StravaAccess stravaAccess = await _stravaRepository.Authorize(stravaAuthorization.Code);
                _logger.LogInformation($"Bergfest for athlete {stravaAccess.FirstName} {stravaAccess.LastName} authorized.");
                return new OkObjectResult(stravaAccess); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuthorizeBergFestOnStrava failed.");
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
