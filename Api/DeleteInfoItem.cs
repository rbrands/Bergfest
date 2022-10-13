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
using BackendLibrary;


namespace BlazorApp.Api
{
    public class DeleteInfoItem
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<InfoItem> _cosmosRepository;

        public DeleteInfoItem(ILogger<DeleteInfoItem> logger,
                         CosmosDBRepository<InfoItem> cosmosRepository)
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName("DeleteInfoItem")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeleteInfoItem")] HttpRequest req
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                InfoItem info = JsonSerializer.Deserialize<InfoItem>(requestBody);
                _logger.LogInformation($"DeleteInfoItem({requestBody})");
                if (String.IsNullOrEmpty(info.Id))
                {
                    return new BadRequestErrorMessageResult("Id of InfoItem to be deleted is missing.");
                }
                await _cosmosRepository.DeleteItemAsync(info.Id);
                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteInfoItem failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
