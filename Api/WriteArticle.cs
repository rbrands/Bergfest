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
    public class WriteArticle
    {
        private readonly ILogger _logger;
        private CosmosDBRepository<Article> _cosmosRepository;

        public WriteArticle(ILogger<WriteArticle> logger,
                         CosmosDBRepository<Article> cosmosRepository)
        {
            _logger = logger;
            _cosmosRepository = cosmosRepository;
        }

        /// <summary>
        /// Writes user contact details to the database. 
        /// </summary>
        [FunctionName("WriteArticle")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "WriteArticle")] HttpRequest req
            )
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                Article article = JsonSerializer.Deserialize<Article>(requestBody);
                if (String.IsNullOrEmpty(article.ArticleKey))
                {
                    throw new Exception("Article without key.");
                }
                article.LogicalKey = article.ArticleKey;
                _logger.LogInformation($"WriteArticle({article.ArticleKey})");
                Article updatedArticle = await _cosmosRepository.UpsertItem(article);

                return new OkObjectResult(article);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WriteArticle failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
