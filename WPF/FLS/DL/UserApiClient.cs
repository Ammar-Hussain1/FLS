using System.Net.Http;
using System.Threading.Tasks;
using FLS.Models;

namespace FLS.DL
{
    public class UserApiClient : BaseApiClient
    {
        public UserApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<UserResponse>> SignInAsync(SignInRequest request)
        {
            var content = CreateJsonContent(request);
            var response = await HttpClient.PostAsync($"{API_BASE_URL}/api/users/signin", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            var apiResponse = await DeserializeResponseAsync<ApiResponse<UserResponse>>(response);
            
            if (apiResponse != null)
            {
                return apiResponse;
            }

            var errorResponse = await DeserializeResponseAsync<ApiResponse<object>>(response);
            return new ApiResponse<UserResponse>
            {
                Success = false,
                Message = errorResponse?.Message ?? $"Sign in failed: {response.StatusCode}",
                ErrorCode = errorResponse?.ErrorCode
            };
        }

        public async Task<ApiResponse<UserResponse>> SignUpAsync(SignUpRequest request)
        {
            var content = CreateJsonContent(request);
            var response = await HttpClient.PostAsync($"{API_BASE_URL}/api/users/signup", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            var apiResponse = await DeserializeResponseAsync<ApiResponse<UserResponse>>(response);
            
            if (apiResponse != null)
            {
                return apiResponse;
            }

            var errorResponse = await DeserializeResponseAsync<ApiResponse<object>>(response);
            return new ApiResponse<UserResponse>
            {
                Success = false,
                Message = errorResponse?.Message ?? $"Sign up failed: {response.StatusCode}",
                ErrorCode = errorResponse?.ErrorCode
            };
        }
    }
}

