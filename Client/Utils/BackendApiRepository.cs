using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using BlazorApp.Shared;
using Microsoft.AspNetCore.Routing;

namespace BlazorApp.Client.Utils
{
    public class BackendApiRepository
    {
        HttpClient _http;

        private void PrepareHttpClient()
        {
        }
        public BackendApiRepository(HttpClient http)
        {
            _http = http;
        }
 
        public async Task<StravaAccess?> AuthorizeBergfestOnStrava(string code)
        {
            this.PrepareHttpClient();
            StravaAuthorization stravaAuthorization = new StravaAuthorization();
            stravaAuthorization.Code = code;
            HttpResponseMessage response = await _http.PostAsJsonAsync<StravaAuthorization>($"/api/AuthorizeBergfestOnStrava", stravaAuthorization);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<StravaAccess>();
        }

        public async Task<string> GetFunctionsVersion()
        {
            this.PrepareHttpClient();
            return await _http.GetStringAsync($"/api/GetVersion");
        }
        public async Task<Article?> WriteArticle(Article article)
        {
            this.PrepareHttpClient();
            HttpResponseMessage response = await _http.PostAsJsonAsync<Article>($"/api/WriteArticle", article);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Article>();
        }
        public async Task<Article?> GetArticle(string key)
        {
            this.PrepareHttpClient();
            return await _http.GetFromJsonAsync<Article>($"/api/GetArticle/{key}");
        }
        public async Task<StravaSegment?> GetSegmentFromStrava(ulong segmentId)
        {
            this.PrepareHttpClient();
            var response = await _http.GetAsync($"/api/GetSegmentFromStrava/{segmentId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<StravaSegment>();
            }
            else
            {
                ErrorMessage error = new ErrorMessage() {
                    Message = $"Http Fehlercode - {response.StatusCode.ToString()}"
                };
                try
                {
                    ErrorMessage? errorFromResponse = await response.Content.ReadFromJsonAsync<ErrorMessage>();
                    if (null != errorFromResponse && !String.IsNullOrEmpty(errorFromResponse.Message))
                    {
                        error.Message = errorFromResponse.Message;
                    }
                }
                catch(Exception)
                {
                    // No exception in exception handler ...
                }
                throw new Exception(error?.Message);
            }
        }
        public async Task<StravaSegment?> WriteSegment(StravaSegment segment)
        {
            this.PrepareHttpClient();
            HttpResponseMessage response = await _http.PostAsJsonAsync<StravaSegment>($"/api/WriteSegment", segment);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<StravaSegment>();
        }
        public async Task DeleteSegment(StravaSegment segment)
        {
            this.PrepareHttpClient();
            HttpResponseMessage response = await _http.PostAsJsonAsync<StravaSegment>($"/api/DeleteSegment", segment);
            response.EnsureSuccessStatusCode();
        }
        public async Task DeleteSegmentEffort(StravaSegmentEffort segmentEffort)
        {
            this.PrepareHttpClient();
            HttpResponseMessage response = await _http.PostAsJsonAsync<StravaSegmentEffort>($"/api/DeleteSegmentEffort", segmentEffort);
            response.EnsureSuccessStatusCode();
        }
        public async Task DeleteUser(StravaAccess user)
        {
            this.PrepareHttpClient();
            HttpResponseMessage response = await _http.PostAsJsonAsync<StravaAccess>($"/api/DeleteUser", user);
            response.EnsureSuccessStatusCode();
        }
        public async Task PostStravaEvent(StravaEvent stravaEvent)
        {
            this.PrepareHttpClient();
            HttpResponseMessage response = await _http.PostAsJsonAsync<StravaEvent>($"/api/PostStravaEvent", stravaEvent);
            response.EnsureSuccessStatusCode();
            return ;
        }
        public async Task<StravaSegment?> GetSegment(ulong segmentId)
        {
            this.PrepareHttpClient();
            var response = await _http.GetAsync($"/api/GetSegment/{segmentId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<StravaSegment>();
            }
            else
            {
                ErrorMessage error = new ErrorMessage()
                {
                    Message = $"Http Fehlercode - {response.StatusCode.ToString()}"
                };
                try
                {
                    ErrorMessage? errorFromResponse = await response.Content.ReadFromJsonAsync<ErrorMessage>();
                    if (null != errorFromResponse && !String.IsNullOrEmpty(errorFromResponse.Message))
                    {
                        error.Message = errorFromResponse.Message;
                    }
                }
                catch (Exception)
                {
                    // No exception in exception handler ...
                }
                throw new Exception(error?.Message);
            }
        }
        public async Task<IEnumerable<StravaAccess>?> GetUsers()
        {
            this.PrepareHttpClient();
            return await _http.GetFromJsonAsync<IEnumerable<StravaAccess>?>($"/api/GetUsers");
        }
        public async Task<IEnumerable<StravaSegmentEffort>?> GetSegmentsEfforts()
        {
            this.PrepareHttpClient();
            return await _http.GetFromJsonAsync<IEnumerable<StravaSegmentEffort>?>($"/api/GetSegmentsEfforts");
        }
        public async Task<IEnumerable<StravaSegmentWithEfforts>> GetSegmentsWithEfforts()
        {
            this.PrepareHttpClient();
            var response = await _http.GetAsync($"/api/GetSegmentsWithEfforts");
            if (response.IsSuccessStatusCode)
            {
                IEnumerable<StravaSegmentWithEfforts> returnList = new List<StravaSegmentWithEfforts>();
                IEnumerable<StravaSegmentWithEfforts>? segmentsWithEfforts = await response.Content.ReadFromJsonAsync<IEnumerable<StravaSegmentWithEfforts>>();
                if (null != segmentsWithEfforts)
                {
                    returnList = segmentsWithEfforts;
                }
                return returnList;
            }
            else
            {
                ErrorMessage error = new ErrorMessage()
                {
                    Message = $"Http Fehlercode - {response.StatusCode.ToString()}"
                };
                try
                {
                    ErrorMessage? errorFromResponse = await response.Content.ReadFromJsonAsync<ErrorMessage>();
                    if (null != errorFromResponse && !String.IsNullOrEmpty(errorFromResponse.Message))
                    {
                        error.Message = errorFromResponse.Message;
                    }
                }
                catch (Exception)
                {
                    // No exception in exception handler ...
                }
                throw new Exception(error?.Message);
            }
        }
        public async Task<IEnumerable<StravaSegmentEffort>> GetSegmentsEffortsForUser(ulong athleteId)
        {
            this.PrepareHttpClient();
            var response = await _http.GetAsync($"/api/GetSegmentsEffortsForUser/{athleteId}");
            if (response.IsSuccessStatusCode)
            {
                IEnumerable<StravaSegmentEffort> returnList = new List<StravaSegmentEffort>();
                IEnumerable<StravaSegmentEffort>? segmentsWithEfforts = await response.Content.ReadFromJsonAsync<IEnumerable<StravaSegmentEffort>>();
                if (null != segmentsWithEfforts)
                {
                    returnList = segmentsWithEfforts;
                }
                return returnList;
            }
            else
            {
                ErrorMessage error = new ErrorMessage()
                {
                    Message = $"Http Fehlercode - {response.StatusCode.ToString()}"
                };
                try
                {
                    ErrorMessage? errorFromResponse = await response.Content.ReadFromJsonAsync<ErrorMessage>();
                    if (null != errorFromResponse && !String.IsNullOrEmpty(errorFromResponse.Message))
                    {
                        error.Message = errorFromResponse.Message;
                    }
                }
                catch (Exception)
                {
                    // No exception in exception handler ...
                }
                throw new Exception(error?.Message);
            }
        }

    }

    public class ErrorMessage
    {
        public string Message { get; set; } = String.Empty;
    }
}