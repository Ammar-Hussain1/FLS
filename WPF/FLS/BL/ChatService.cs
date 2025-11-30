using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FLS.DL;

namespace FLS.BL
{
    public class ChatService
    {
        private readonly ChatApiClient _chatApiClient;
        private readonly string _apiKey;

        public ChatService(ChatApiClient chatApiClient, string apiKey)
        {
            _chatApiClient = chatApiClient ?? throw new ArgumentNullException(nameof(chatApiClient));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }

        public async Task<string> SendMessageAsync(string userId, string message, List<ChatTurnDTO> history)
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
                UserId = userId,
                Message = message,
                ApiKey = _apiKey,
                History = history
            };

            var response = await _chatApiClient.SendChatMessageAsync(request);
            return response.Response;
        }
    }

    // DTOs for API communication
    public class ChatRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public List<ChatTurnDTO>? History { get; set; }
    }

    public class ChatResponse
    {
        public string Response { get; set; } = string.Empty;
    }

    public class ChatTurnDTO
    {
        public string Role { get; set; } = string.Empty; // "user" or "assistant"
        public string Message { get; set; } = string.Empty;
    }
}
