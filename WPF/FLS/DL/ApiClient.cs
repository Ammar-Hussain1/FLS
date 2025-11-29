using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FLS.BL;
using FLS.Models;

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

        public async Task UploadTimetableAsync(string filePath)
        {
            using var content = new MultipartFormDataContent();
            using var fileStream = System.IO.File.OpenRead(filePath);
            using var fileContent = new StreamContent(fileStream);
            
            content.Add(fileContent, "file", System.IO.Path.GetFileName(filePath));
            
            var response = await _httpClient.PostAsync($"{API_BASE_URL}/api/Timetable/upload", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API returned {response.StatusCode}: {errorContent}");
            }
        }

        public async Task<ApiResponse<UserResponse>> SignInAsync(SignInRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{API_BASE_URL}/api/users/signin", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                // The API returns an ApiResponse wrapper
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserResponse>>(responseJson, options);
                
                if (apiResponse != null)
                {
                    return apiResponse;
                }
            }
            catch
            {
                // If deserialization fails, try to parse as error
            }

            // Fallback: try to deserialize as error response
            var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseJson, options);
            return new ApiResponse<UserResponse>
            {
                Success = false,
                Message = errorResponse?.Message ?? $"Sign in failed: {response.StatusCode}",
                ErrorCode = errorResponse?.ErrorCode
            };
        }

        public async Task<ApiResponse<UserResponse>> SignUpAsync(SignUpRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{API_BASE_URL}/api/users/signup", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                // The API returns an ApiResponse wrapper
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserResponse>>(responseJson, options);
                
                if (apiResponse != null)
                {
                    return apiResponse;
                }
            }
            catch
            {
                // If deserialization fails, try to parse as error
            }

            // Fallback: try to deserialize as error response
            var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseJson, options);
            return new ApiResponse<UserResponse>
            {
                Success = false,
                Message = errorResponse?.Message ?? $"Sign up failed: {response.StatusCode}",
                ErrorCode = errorResponse?.ErrorCode
            };
        }
    }
}
