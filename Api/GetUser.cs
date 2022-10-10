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
    public class GetUser
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private StravaRepository _cosmosRepository;

        public GetUser(ILogger<GetUser> logger,
                        IConfiguration config,
                        StravaRepository cosmosRepository
        )
        {
            _logger = logger;
            _config = config;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName(nameof(GetUser))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetUser/{id}")] HttpRequest req, string id)
        {
            try
            {
                StravaAccess userInfos = await _cosmosRepository.GetItem(id);

                return new OkObjectResult(userInfos);
            }
            catch (Exception ex)
            {
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
