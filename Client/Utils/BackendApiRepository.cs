using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using BlazorApp.Shared;

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
            return await _http.GetFromJsonAsync<StravaSegment>($"/api/GetSegmentFromStrava/{segmentId}");
        }


    }
}