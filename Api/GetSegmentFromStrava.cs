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
using Flurl;
using Flurl.Http.Configuration;
using Flurl.Http;
using System.Collections.Generic;
using BlazorApp.Api.Repositories;
using BlazorApp.Api.Utils;

namespace BlazorApp.Api
{
    public class GetSegmentFromStrava
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private CosmosDBRepository<StravaSegment> _cosmosRepository;
        private const string STRAVA_API_ENDPOINT = "https://www.strava.com/api/v3";
        private readonly IFlurlClient _flurlClient;
        private readonly StravaRepository _stravaRepository;

        public GetSegmentFromStrava(ILogger<GetSegmentFromStrava> logger,
                        IConfiguration config,
                        CosmosDBRepository<StravaSegment> cosmosRepository,
                        IFlurlClientFactory flurlClientFactory,
                        StravaRepository stravaRepository
        )
        {
            _logger = logger;
            _config = config;
            _cosmosRepository = cosmosRepository;
            _flurlClient = flurlClientFactory.Get(STRAVA_API_ENDPOINT);
            _stravaRepository = stravaRepository;
        }

        [FunctionName(nameof(GetSegmentFromStrava))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetSegmentFromStrava/{segmentId}")] HttpRequest req, ulong segmentId)
        {
            try
            {
                if (0 == segmentId)
                {
                    throw new Exception("Missing segmentId for call GetSegmentFromStrava()");
                }
                _logger.LogInformation("GetSegmentFromStrava({segmentId})", segmentId);
                string accessToken = await _stravaRepository.GetAccessTokenForAdmin();
                dynamic response = await _flurlClient.Request("segments", segmentId)
                                                .WithOAuthBearerToken(accessToken)
                                                .GetJsonAsync();
                StravaSegment stravaSegment = new StravaSegment()
                { 
                    SegmentId = segmentId,
                    SegmentName = response.name,
                    Distance = response.distance,
                    AverageGrade = response.average_grade,
                    Elevation = response.elevation_high - response.elevation_low,
                    ClimbCategory = response.climb_category,
                    City = response.city
                };
                stravaSegment.LogicalKey = stravaSegment.SegmentId.ToString();
                
                return new OkObjectResult(stravaSegment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetArticle failed.");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}
