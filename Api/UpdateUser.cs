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
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using BackendLibrary;


namespace BlazorApp.Api
{
    public class UpdateUser
    {
        private readonly ILogger _logger;
        private StravaRepository _cosmosRepository;

        public UpdateUser(ILogger<UpdateUser> logger,
                         StravaRepository cosmosRepository
                         )
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName("UpdateUser")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "UpdateUser")] HttpRequest req
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                StravaAccess user = JsonSerializer.Deserialize<StravaAccess>(requestBody);

                StravaAccess updatedUser = await _cosmosRepository.UpdateUser(user);

                return new OkObjectResult(updatedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateUser failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
