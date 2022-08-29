using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using BlazorApp.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Flurl;
using Flurl.Http.Configuration;
using Flurl.Http;
using System.Web.Http;

namespace BlazorApp.Api.Repositories
{
    public class StravaRepository : CosmosDBRepository<StravaAccess>
    {
        private const string STRAVA_API_ENDPOINT = "https://www.strava.com/api/v3";
        private const string STRAVA_TOKEN_ENDPOINT = "https://www.strava.com/api/v3/oauth/token";
        private readonly IFlurlClient _flurlClient;
        private readonly ILogger _logger;
        private IConfiguration _config;

        public StravaRepository(ILogger<StravaRepository> logger, IConfiguration config, CosmosClient cosmosClient, IFlurlClientFactory flurlClientFactory) : base(config, cosmosClient)
        {
            _logger = logger;
            _config = config;
            _flurlClient = flurlClientFactory.Get(STRAVA_API_ENDPOINT);
        }

        public async Task<string> GetAccessToken(int athleteId)
        {
            try
            { 
            StravaAccess stravaAccess = await this.GetFirstItemOrDefault(a => a.AthleteId == athleteId);
            if (null == stravaAccess)
            {
                throw new Exception($"No access token found for athlete {athleteId}.");
            }
            if (stravaAccess.ExpirationAt < DateTime.UtcNow)
            {

                    dynamic response = await _flurlClient.Request("oauth/token")
                                                         .SetQueryParams(new {
                                                             client_id = _config["STRAVA_CLIENT_ID"],
                                                             client_secret = _config["STRAVA_CLIENT_SECRET"],
                                                             grant_type = "refresh_token",
                                                             refresh_token = stravaAccess.RefreshToken
                                                         })
                                                         .PostJsonAsync(null);
                    stravaAccess.AccessToken = response.access_token;
                    stravaAccess.RefreshToken = response.refresh_token;
                    stravaAccess.ExpirationAt = DateTime.UtcNow.AddSeconds(response.expires_in);
                    await this.UpsertItem(stravaAccess);
                    return stravaAccess.AccessToken;
            }
            return null;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "GetAccessToken failed.");
                throw;
            }
        }
    }
}
