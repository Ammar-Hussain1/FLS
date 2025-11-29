using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FLS.BL;

namespace FLS.DL
{
    /// <summary>
    /// Data access layer for API communication
    /// Handles HTTP requests to the backend server
    /// </summary>
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private const string API_BASE_URL = "http://localhost:5232";

        public ApiClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<ChatResponse> SendChatMessageAsync(ChatRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{API_BASE_URL}/api/Chat/send", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API returned {response.StatusCode}: {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var chatResponse = JsonSerializer.Deserialize<ChatResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return chatResponse ?? throw new Exception("Failed to deserialize API response");
        }
    }
}
