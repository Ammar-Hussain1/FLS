using System.Net.Http;
using System.Threading.Tasks;
using FLS.Models;

namespace FLS.DL
{
    public class UserCourseApiClient : BaseApiClient
    {
        public UserCourseApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<object>> AddUserCourseAsync(string userId, string courseId, string? sectionName)
        {
            var request = new AddUserCourseRequest
            {
                UserId = userId,
                CourseId = courseId,
                SectionName = sectionName
            };

            var content = CreateJsonContent(request);
            var response = await HttpClient.PostAsync($"{API_BASE_URL}/api/UserCourses", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            var apiResponse = await DeserializeResponseAsync<ApiResponse<object>>(response);
            if (apiResponse != null)
            {
                return apiResponse;
            }

            return new ApiResponse<object>
            {
                Success = response.IsSuccessStatusCode,
                Message = response.IsSuccessStatusCode
                    ? "User course saved successfully."
                    : $"API returned {response.StatusCode}: {responseJson}"
            };
        }

        public async Task<ApiResponse<object>> RemoveUserCourseAsync(string userId, string courseId)
        {
            var url = $"{API_BASE_URL}/api/UserCourses?userId={System.Uri.EscapeDataString(userId)}&courseId={System.Uri.EscapeDataString(courseId)}";
            var response = await HttpClient.DeleteAsync(url);
            var responseJson = await response.Content.ReadAsStringAsync();

            var apiResponse = await DeserializeResponseAsync<ApiResponse<object>>(response);
            if (apiResponse != null)
            {
                return apiResponse;
            }

            return new ApiResponse<object>
            {
                Success = response.IsSuccessStatusCode,
                Message = response.IsSuccessStatusCode
                    ? "User course removed successfully."
                    : $"API returned {response.StatusCode}: {responseJson}"
            };
        }

        public async Task<ApiResponse<List<UserCourseDTO>>> GetMyCoursesAsync(string userId)
        {
            var response = await HttpClient.GetAsync($"{API_BASE_URL}/api/CourseMaterials/my-courses?userId={System.Uri.EscapeDataString(userId)}");
            var responseJson = await response.Content.ReadAsStringAsync();

            var apiResponse = await DeserializeResponseAsync<ApiResponse<List<UserCourseDTO>>>(response);
            return apiResponse ?? new ApiResponse<List<UserCourseDTO>>
            {
                Success = false,
                Message = "Failed to deserialize response"
            };
        }
    }
}

