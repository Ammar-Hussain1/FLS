using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FLS.Models;

namespace FLS.DL
{
    public class CourseApiClient : BaseApiClient
    {
        public CourseApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<PaginatedResponse<CourseDTO>>> GetCoursesAsync(int page = 1, int pageSize = 10, string? search = null)
        {
            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                queryParams.Add($"search={System.Uri.EscapeDataString(search)}");
            }

            var url = $"{API_BASE_URL}/api/Courses?{string.Join("&", queryParams)}";
            var response = await HttpClient.GetAsync(url);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var paginatedResponse = await DeserializeResponseAsync<PaginatedResponse<CourseDTO>>(response);
                return new ApiResponse<PaginatedResponse<CourseDTO>>
                {
                    Success = true,
                    Data = paginatedResponse
                };
            }
            else
            {
                return new ApiResponse<PaginatedResponse<CourseDTO>>
                {
                    Success = false,
                    Message = $"API returned {response.StatusCode}: {responseJson}"
                };
            }
        }
    }
}

