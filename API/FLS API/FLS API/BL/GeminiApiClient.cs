using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FLS_API.BL
{
    public class GeminiApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

        public GeminiApiClient(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
        }

        public async Task<GeminiResponse> GenerateContentAsync(string prompt, double temperature = 0.7)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = temperature,
                    maxOutputTokens = 2048
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{GEMINI_API_URL}?key={_apiKey}", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseJson);

            if (geminiResponse?.Candidates == null || geminiResponse.Candidates.Length == 0)
            {
                throw new Exception("No response from Gemini API");
            }

            var responseText = geminiResponse.Candidates[0].Content.Parts[0].Text;
            
            return new GeminiResponse
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

        private List<int> ParseMemoryDeletions(string text)
        {
            var deletions = new List<int>();
            var lines = text.Split('\n');

            foreach (var line in lines)
            {
                if (line.Contains("[FORGET:"))
                {
                    var idStr = ExtractBetween(line, "[FORGET:", "]");
                    if (int.TryParse(idStr?.Trim(), out var id))
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
    public class GeminiResponse
    {
        public string Text { get; set; } = string.Empty;
        public string CleanText { get; set; } = string.Empty;
        public List<MemoryUpdate> MemoryUpdates { get; set; } = new();
        public List<int> MemoryDeletions { get; set; } = new();
    }

    public class MemoryUpdate
    {
        public string Content { get; set; } = string.Empty;
        public int Importance { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    // Gemini API response models
    internal class GeminiApiResponse
    {
        [JsonPropertyName("candidates")]
        public Candidate[]? Candidates { get; set; }
    }

    internal class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; } = new();
    }

    internal class Content
    {
        [JsonPropertyName("parts")]
        public Part[] Parts { get; set; } = Array.Empty<Part>();
    }

    internal class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}
