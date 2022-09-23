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
using BlazorApp.Api.Repositories;
using BlazorApp.Api.Utils;

namespace BlazorApp.Api
{
    public class PostStravaEvent
    {
        private readonly ILogger _logger;
        private QueueStorageRepository _queueRepository;

        public PostStravaEvent(ILogger<PostStravaEvent> logger,
                         QueueStorageRepository queueRepository)
        {
            _logger = logger;
            _queueRepository = queueRepository;
        }

        /// <summary>
        /// Writes user contact details to the database. 
        /// </summary>
        [FunctionName("PostStravaEvent")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "PostStravaEvent")] HttpRequest req
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                StravaEvent stravaEvent = JsonSerializer.Deserialize<StravaEvent>(requestBody);
                await _queueRepository.InsertMessage(stravaEvent);

                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostStravaEvent failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
