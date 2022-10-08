using Microsoft.Azure.Cosmos;
using BlazorApp.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Flurl;
using Flurl.Http.Configuration;
using Flurl.Http;
using Microsoft.VisualBasic;

namespace BackendLibrary
{
    /// <summary>
    /// Interface to Strava API. For Strava API see https://developers.strava.com/ 
    /// </summary>
    public class StravaRepository : CosmosDBRepository<StravaAccess>
    {
        private const string STRAVA_API_ENDPOINT = "https://www.strava.com/api/v3";
        public const int STRAVA_TTL_ACCESS = 3 * 30 * 24 * 3600; // 3 month TTL for Strava Access
        public const int STRAVA_TTL_SEGMENT_EFFORT = 7 * 24 * 3600; // 7 days TTL for segment efforts
        public const string STRAVA_WEBHOOK_VERIFY_TOKEN = "Bergfest-Webhook";
        public const int STRAVA_MESSAGE_VISIBILITY_TIMEOUT = 9;
        private readonly IFlurlClient _flurlClient;
        private readonly ILogger _logger;
        private IConfiguration _config;
        private ulong _adminAthleteId = 0;

        public StravaRepository(ILogger<StravaRepository> logger, IConfiguration config, CosmosClient cosmosClient, IFlurlClientFactory flurlClientFactory) : base(config, cosmosClient)
        {
            _logger = logger;
            _config = config;
            _flurlClient = flurlClientFactory.Get(STRAVA_API_ENDPOINT);
            // Get ID of athlete that is used to read segments.
            _adminAthleteId = Convert.ToUInt32(_config["STRAVA_ADMIN_ATHLETE_ID"]);
        }

        /// <summary>
        /// Get access token for calling the Strava API. The token is stored in the database. If expired get a new one with the stored refresh token
        /// See https://developers.strava.com/docs/authentication/ for details.
        /// </summary>
        /// <param name="athleteId"></param>
        /// <returns></returns>
        public async Task<StravaAccess> GetAccessToken(ulong athleteId)
        {
            try
            {
                StravaAccess? stravaAccess = await this.GetItemByKey(athleteId.ToString());
                if (null == stravaAccess)
                {
                    throw new Exception($"No access token found for athlete {athleteId}.");
                }
                if (stravaAccess.ExpirationAt < DateTime.UtcNow)
                {
                    dynamic response = await _flurlClient.Request("oauth/token")
                                                            .SetQueryParams(new
                                                            {
                                                                client_id = _config["STRAVA_CLIENT_ID"],
                                                                client_secret = _config["STRAVA_CLIENT_SECRET"],
                                                                grant_type = "refresh_token",
                                                                refresh_token = stravaAccess.RefreshToken
                                                            })
                                                            .PostJsonAsync(null)
                                                            .ReceiveJson();
                    stravaAccess.AccessToken = response.access_token;
                    stravaAccess.RefreshToken = response.refresh_token;
                    stravaAccess.ExpirationAt = DateTime.UtcNow.AddSeconds(response.expires_in);
                    await this.UpsertItem(stravaAccess);
                }
                return stravaAccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAccessToken failed.");
                throw;
            }
        }
        public async Task<string> GetAccessTokenForAdmin()
        {
            if (0 == _adminAthleteId)
            {
                throw new Exception("STRAVA_ADMIN_ATHLETE_ID not configured.");
            }
            StravaAccess stravaAccess = await GetAccessToken(_adminAthleteId);
            return stravaAccess.AccessToken;
        }
        /// <summary>
        /// Gets the StravaAccess item for the given athlete id. If not found null is returned.
        /// </summary>
        /// <param name="athleteId"></param>
        /// <returns></returns>
        public async Task<StravaAccess?> GetStravaAccess(ulong athleteId)
        {
            try
            {
                StravaAccess? stravaAccess = await this.GetItemByKey(athleteId.ToString());
                return stravaAccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetStravaAccess failed.");
                throw;
            }

        }

        /// <summary>
        /// Authorize application "Bergfest" on Strava.
        /// See https://developers.strava.com/docs/authentication/ for details.
        /// </summary>
        /// <param name="athleteId">Id of athlete received as query param after redirecting from Strava</param>
        /// <param name="code">Received as query param after redirecting from Strava</param>
        /// <returns>Access token</returns>
        public async Task<StravaAccess> Authorize(string code)
        {
            try
            {
                dynamic response = await _flurlClient.Request("oauth/token")
                                                     .SetQueryParams(new
                                                     {
                                                         client_id = _config["STRAVA_CLIENT_ID"],
                                                         client_secret = _config["STRAVA_CLIENT_SECRET"],
                                                         code = code,
                                                         grant_type = "authorization_code"
                                                     })
                                                      .PostJsonAsync(null)
                                                      .ReceiveJson();
                _logger.LogInformation($"Authorize returned {response.ToString()}");
                StravaAccess stravaAccess = new StravaAccess();
                stravaAccess.AccessToken = response.access_token;
                stravaAccess.RefreshToken = response.refresh_token;
                stravaAccess.ExpirationAt = DateTime.UtcNow.AddSeconds(response.expires_in);
                stravaAccess.AthleteId = response.athlete.id;
                stravaAccess.LogicalKey = stravaAccess.AthleteId.ToString();
                stravaAccess.FirstName = response.athlete.firstname;
                stravaAccess.LastName = response.athlete.lastname;
                stravaAccess.Sex = response.athlete.sex;
                stravaAccess.ProfileImageLink = response.athlete.profile;
                stravaAccess.ProfileSmallImageLink = response.athlete.profile_medium;
                stravaAccess.TimeToLive = STRAVA_TTL_ACCESS;
                _logger.LogInformation($"Athlete: LogicalKey {stravaAccess.LogicalKey} Name {stravaAccess.FirstName} {stravaAccess.LastName} ");

                await this.UpsertItem(stravaAccess);
                return stravaAccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authorize failed.");
                throw;
            }
        }
        /// <summary>
        /// User deauthorized application "Bergfest" on Strava ==> delete athlete
        /// See https://developers.strava.com/docs/authentication/ for details.
        /// </summary>
        /// <param name="athleteId">Id of athlete</param>
        public async Task DeAuthorize(ulong athleteId)
        {
            await this.DeleteItemByKeyAsync(athleteId.ToString());
        }
        public async Task<StravaAccess> UpdateUser(StravaAccess user)
        {
            try
            {
                _logger.LogInformation($"UpdateUser({user.GetFullName()})");
                List<PatchOperation> patchOperations = new()
                {
                    PatchOperation.Add("/FirstName", user.FirstName),
                    PatchOperation.Add("/LastName", user.LastName),
                    PatchOperation.Add("/Sex", user.Sex)
                };

                StravaAccess updatedUser = await this.PatchItem(user.Id, patchOperations);
                return updatedUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateChallenge failed.");
                throw;
            }
        }


    }
}
