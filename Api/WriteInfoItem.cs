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
    public class WriteInfoItem
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<InfoItem> _cosmosRepository;

        public WriteInfoItem(ILogger<WriteInfoItem> logger,
                         CosmosDBRepository<InfoItem> cosmosRepository)
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
        }

        /// <summary>
        /// Writes user contact details to the database. 
        /// </summary>
        [FunctionName("WriteInfoItem")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "WriteInfoItem")] HttpRequest req
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                InfoItem infoItem = JsonSerializer.Deserialize<InfoItem>(requestBody);
                _logger.LogInformation($"WriteInfoItem({infoItem.Title})");
                if (infoItem.InfoLifeTimeInDays > 0)
                {
                    infoItem.TimeToLive = infoItem.InfoLifeTimeInDays * 24 * 3600;
                }

                InfoItem updatedInfoItem = await _cosmosRepository.UpsertItem(infoItem);

                return new OkObjectResult(updatedInfoItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WriteInfoItem failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
