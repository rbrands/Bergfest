using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.Routing;

namespace BlazorApp.Api
{
    public class DeleteUser
    {
        private readonly ILogger _logger;
        private StravaRepository _cosmosRepository;

        public DeleteUser(ILogger<DeleteUser> logger,
                         StravaRepository cosmosRepository)
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName("DeleteUser")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeleteUser")] HttpRequest req
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                StravaAccess user = JsonSerializer.Deserialize<StravaAccess>(requestBody);
                _logger.LogInformation($"DeleteUser({requestBody})");
                if (String.IsNullOrEmpty(user.Id))
                {
                    return new BadRequestErrorMessageResult("Id of StravaAccess to be deleted is missing.");
                }
                await _cosmosRepository.DeleteItemAsync(user.Id);
                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteUser failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
