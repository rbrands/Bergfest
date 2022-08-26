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
            _http.DefaultRequestHeaders.Remove("x-meetup-tenant");
        }
        public BackendApiRepository(HttpClient http)
        {
            _http = http;
        }
 

        public async Task<string> GetFunctionsVersion()
        {
            this.PrepareHttpClient();
            return await _http.GetStringAsync($"/api/GetVersion");
        }


    }
}