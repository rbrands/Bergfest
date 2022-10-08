using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BlazorApp.Shared;
using System.Web.Http;
using BackendLibrary;


namespace BlazorApp.Api
{
    public class WriteUser
    {
        private readonly ILogger _logger;
        private StravaRepository _cosmosRepository;

        public WriteUser(ILogger<WriteUser> logger,
                         StravaRepository cosmosRepository
                         )
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName("WriteUser")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "WriteUser")] HttpRequest req
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                StravaAccess user = JsonSerializer.Deserialize<StravaAccess>(requestBody);
                _logger.LogInformation($"WriteUser({user.GetFullName()})");
                user.LogicalKey = user.AthleteId.ToString();
                user.TimeToLive = StravaRepository.STRAVA_TTL_ACCESS;
                StravaAccess updatedUser = await _cosmosRepository.UpsertItem(user);

                return new OkObjectResult(updatedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WriteUser failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
