using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FLS.DL
{
    public abstract class BaseApiClient
    {
        protected readonly HttpClient HttpClient;
        protected const string API_BASE_URL = "http://localhost:5232";

        protected BaseApiClient(HttpClient httpClient)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        protected async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<T>(responseJson, options);
        }

        protected StringContent CreateJsonContent<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        protected async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API returned {response.StatusCode}: {errorContent}");
            }
        }
    }
}

