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
using BlazorApp.Api.Repositories;
using BlazorApp.Api.Utils;

namespace BlazorApp.Api
{
    public class GetArticle
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private CosmosDBRepository<Article> _cosmosRepository;
        public GetArticle(ILogger<GetArticle> logger,
                        IConfiguration config,
                        CosmosDBRepository<Article> cosmosRepository
        )
        {
            _logger = logger;
            _config = config;
            _cosmosRepository = cosmosRepository;
        }

        [FunctionName(nameof(GetArticle))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetArticle/{key}")] HttpRequest req, string key)
        {
            try
            {
                if (String.IsNullOrEmpty(key))
                {
                    throw new Exception("Missing key for call GetArticle()");
                }
                _logger.LogInformation("GetArticle({key})", key);
                string dbKey = key;
                Article article = await _cosmosRepository.GetItemByKey(dbKey);
                if (null == article)
                {
                    article = new Article();
                    article.ArticleKey = key;
                }
                return new OkObjectResult(article);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetArticle failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
