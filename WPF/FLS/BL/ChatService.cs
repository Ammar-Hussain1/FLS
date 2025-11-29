using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FLS.DL;

namespace FLS.BL
{
    /// <summary>
    /// Business logic for chatbot interactions
    /// Handles API communication and response processing
    /// </summary>
    public class ChatService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiClient _apiClient;
        private readonly string _apiKey;

        public ChatService(string apiKey)
        {
            _httpClient = new HttpClient();
            _apiClient = new ApiClient();
            _apiKey = apiKey;
        }

        public async Task<string> SendMessageAsync(int userId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message cannot be empty", nameof(message));
            }

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("API key is not configured");
            }

            var request = new ChatRequest
            {
                UserId = userId.ToString(),
                Message = message,
                ApiKey = _apiKey
            };

            var response = await _apiClient.SendChatMessageAsync(request);
            return response.Response;
        }

        public async Task UploadTimetableAsync(string filePath)
        {
            await _apiClient.UploadTimetableAsync(filePath);
        }
    }

    // DTOs for API communication
    public class ChatRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }

    public class ChatResponse
    {
        public string Response { get; set; } = string.Empty;
    }
}
