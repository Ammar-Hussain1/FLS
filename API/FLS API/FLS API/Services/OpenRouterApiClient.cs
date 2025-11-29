using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FLS_API.BL
{
    public class OpenRouterApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string GROQ_API_URL = "https://api.groq.com/openai/v1/chat/completions";
        // Using Groq's free Llama model - fast and reliable
        private const string DEFAULT_MODEL = "llama-3.3-70b-versatile";

        public OpenRouterApiClient(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
        }

        public async Task<OpenRouterResponse> GenerateContentAsync(string prompt, double temperature = 0.7)
        {
            var requestBody = new
            {
                model = DEFAULT_MODEL,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = temperature,
                max_tokens = 2048
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.PostAsync(GROQ_API_URL, content);
            
            // Better error handling - capture the actual error message
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Groq API error ({response.StatusCode}): {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var groqResponse = JsonSerializer.Deserialize<OpenRouterApiResponse>(responseJson);

            if (groqResponse?.Choices == null || groqResponse.Choices.Length == 0)
            {
                throw new Exception("No response from Groq API");
            }

            var responseText = groqResponse.Choices[0].Message.Content;
            
            return new OpenRouterResponse
            {
                Text = responseText,
                MemoryUpdates = ParseMemoryUpdates(responseText),
                MemoryDeletions = ParseMemoryDeletions(responseText),
                CleanText = CleanResponseText(responseText)
            };
        }

        private List<MemoryUpdate> ParseMemoryUpdates(string text)
        {
            var updates = new List<MemoryUpdate>();
            var lines = text.Split('\n');

            foreach (var line in lines)
            {
                if (line.Contains("[REMEMBER:"))
                {
                    try
                    {
                        var content = ExtractBetween(line, "[REMEMBER:", "|");
                        var importanceStr = ExtractBetween(line, "IMPORTANCE:", "|");
                        var category = ExtractBetween(line, "CATEGORY:", "]");

                        if (!string.IsNullOrEmpty(content))
                        {
                            updates.Add(new MemoryUpdate
                            {
                                Content = content.Trim(),
                                Importance = int.TryParse(importanceStr?.Trim(), out var imp) ? imp : 5,
                                Category = category?.Trim() ?? "general"
                            });
                        }
                    }
                    catch
                    {
                        // Skip malformed memory updates
                    }
                }
            }

            return updates;
        }

        private List<Guid> ParseMemoryDeletions(string text)
        {
            var deletions = new List<Guid>();
            var lines = text.Split('\n');

            foreach (var line in lines)
            {
                if (line.Contains("[FORGET:"))
                {
                    var idStr = ExtractBetween(line, "[FORGET:", "]");
                    if (Guid.TryParse(idStr?.Trim(), out var id))
                    {
                        deletions.Add(id);
                    }
                }
            }

            return deletions;
        }

        private string CleanResponseText(string text)
        {
            // Remove memory management commands from user-facing response
            var lines = text.Split('\n')
                .Where(line => !line.Contains("[REMEMBER:") && !line.Contains("[FORGET:"))
                .ToArray();

            return string.Join('\n', lines).Trim();
        }

        private string? ExtractBetween(string text, string start, string end)
        {
            var startIndex = text.IndexOf(start);
            if (startIndex == -1) return null;

            startIndex += start.Length;
            var endIndex = text.IndexOf(end, startIndex);
            if (endIndex == -1) return null;

            return text.Substring(startIndex, endIndex - startIndex);
        }
    }

    // Response models
    public class OpenRouterResponse
    {
        public string Text { get; set; } = string.Empty;
        public string CleanText { get; set; } = string.Empty;
        public List<MemoryUpdate> MemoryUpdates { get; set; } = new();
        public List<Guid> MemoryDeletions { get; set; } = new();
    }

    public class MemoryUpdate
    {
        public string Content { get; set; } = string.Empty;
        public int Importance { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    // OpenAI-compatible API response models (works for Groq too)
    internal class OpenRouterApiResponse
    {
        [JsonPropertyName("choices")]
        public Choice[]? Choices { get; set; }
    }

    internal class Choice
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; } = new();
    }

    internal class Message
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}
