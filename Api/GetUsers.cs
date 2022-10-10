using System;
using System.IO;
using System.Threading.Tasks;
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
    public class GetUsers
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private StravaRepository _cosmosRepository;

        public GetUsers(ILogger<GetUsers> logger,
                        IConfiguration config,
                        StravaRepository cosmosRepository
        )
        {
            _logger = logger;
            _config = config;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName(nameof(GetUsers))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetUsers")] HttpRequest req)
        {
            try
            {
                IEnumerable<StravaAccess> userInfos = await _cosmosRepository.GetItems();

                return new OkObjectResult(userInfos);
            }
            catch (Exception ex)
            {
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
