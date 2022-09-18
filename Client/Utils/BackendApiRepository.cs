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
            // _http.DefaultRequestHeaders.Remove("x-meetup-tenant");
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

    }

    public class ErrorMessage
    {
        public string Message { get; set; } = String.Empty;
    }
}