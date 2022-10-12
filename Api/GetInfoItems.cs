using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BlazorApp.Shared;
using System.Web.Http;
using System.Collections.Generic;
using BackendLibrary;


namespace BlazorApp.Api
{
    public class GetInfoItems
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private CosmosDBRepository<InfoItem> _cosmosRepository;
        public GetInfoItems(ILogger<GetSegments> logger,
                        IConfiguration config,
                        CosmosDBRepository<InfoItem> cosmosRepository
        )
        {
            _logger = logger;
            _config = config;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName(nameof(GetInfoItems))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetInfoItems")] HttpRequest req)
        {
            try
            {
                _logger.LogInformation($"GetInfoItems()");
                IEnumerable<InfoItem> infoItems = await _cosmosRepository.GetItems();
                return new OkObjectResult(infoItems.OrderBy(i => i.OrderId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetInfoItems failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
