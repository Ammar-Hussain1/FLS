using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FLS.BL;
using FLS.Models;

namespace FLS.DL
{
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

        public async Task<List<TimetableDTO>> GetTimetableAsync()
        {
            var response = await _httpClient.GetAsync($"{API_BASE_URL}/api/Timetable");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API returned {response.StatusCode}: {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var timetable = JsonSerializer.Deserialize<List<TimetableDTO>>(responseJson, options);
            return timetable ?? new List<TimetableDTO>();
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
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserResponse>>(responseJson, options);
                
                if (apiResponse != null)
                {
                    return apiResponse;
                }
            }
            catch
            {
            }

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
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserResponse>>(responseJson, options);
                
                if (apiResponse != null)
                {
                    return apiResponse;
                }
            }
            catch
            {
            }

            var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseJson, options);
            return new ApiResponse<UserResponse>
            {
                Success = false,
                Message = errorResponse?.Message ?? $"Sign up failed: {response.StatusCode}",
                ErrorCode = errorResponse?.ErrorCode
            };
        }

        public async Task<ApiResponse<List<UserCourseDTO>>> GetMyCoursesAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"{API_BASE_URL}/api/CourseMaterials/my-courses?userId={Uri.EscapeDataString(userId)}");
            var responseJson = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<UserCourseDTO>>>(responseJson, options);
                return apiResponse ?? new ApiResponse<List<UserCourseDTO>>
                {
                    Success = false,
                    Message = "Failed to deserialize response"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<UserCourseDTO>>
                {
                    Success = false,
                    Message = $"Failed to get courses: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<CourseWithMaterialsDTO>> GetCourseMaterialsAsync(string courseId, string? category = null)
        {
            var url = $"{API_BASE_URL}/api/CourseMaterials/course/{Uri.EscapeDataString(courseId)}";
            if (!string.IsNullOrWhiteSpace(category))
            {
                url += $"?category={Uri.EscapeDataString(category)}";
            }

            var response = await _httpClient.GetAsync(url);
            var responseJson = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<CourseWithMaterialsDTO>>(responseJson, options);
                return apiResponse ?? new ApiResponse<CourseWithMaterialsDTO>
                {
                    Success = false,
                    Message = "Failed to deserialize response"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<CourseWithMaterialsDTO>
                {
                    Success = false,
                    Message = $"Failed to get course materials: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<MaterialResponseDTO>> GetMaterialAsync(string materialId)
        {
            var response = await _httpClient.GetAsync($"{API_BASE_URL}/api/CourseMaterials/{Uri.EscapeDataString(materialId)}");
            var responseJson = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<MaterialResponseDTO>>(responseJson, options);
                return apiResponse ?? new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "Failed to deserialize response"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = $"Failed to get material: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<MaterialResponseDTO>> UploadMaterialAsync(string userId, string filePath, string courseName, string fileType, int? year = null)
        {
            if (!File.Exists(filePath))
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "File not found"
                };
            }

            try
            {
                using var content = new MultipartFormDataContent();
                using var fileStream = File.OpenRead(filePath);
                using var fileContent = new StreamContent(fileStream);
                
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                content.Add(fileContent, "File", Path.GetFileName(filePath));
                content.Add(new StringContent(courseName), "CourseName");
                content.Add(new StringContent(fileType), "FileType");
                if (year.HasValue)
                {
                    content.Add(new StringContent(year.Value.ToString()), "Year");
                }

                var response = await _httpClient.PostAsync($"{API_BASE_URL}/api/CourseMaterials/upload?userId={Uri.EscapeDataString(userId)}", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<MaterialResponseDTO>>(responseJson, options);
                return apiResponse ?? new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "Failed to deserialize response"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = $"Failed to upload material: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<MaterialResponseDTO>> RequestMaterialAsync(string userId, string fileLink, string courseName, string fileType, int? year = null)
        {
            var request = new
            {
                fileLink,
                courseName,
                fileType,
                year
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{API_BASE_URL}/api/CourseMaterials/request?userId={Uri.EscapeDataString(userId)}", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<MaterialResponseDTO>>(responseJson, options);
                return apiResponse ?? new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "Failed to deserialize response"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = $"Failed to request material: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<MaterialRequest>>> GetMyRequestsAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"{API_BASE_URL}/api/CourseMaterials/my-requests?userId={Uri.EscapeDataString(userId)}");
            var responseJson = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<MaterialResponseDTO>>>(responseJson, options);
                
                if (apiResponse == null)
                {
                    return new ApiResponse<List<MaterialRequest>>
                    {
                        Success = false,
                        Message = "Failed to deserialize response"
                    };
                }

                if (!apiResponse.Success || apiResponse.Data == null)
                {
                    return new ApiResponse<List<MaterialRequest>>
                    {
                        Success = apiResponse.Success,
                        Message = apiResponse.Message,
                        ErrorCode = apiResponse.ErrorCode,
                        Data = new List<MaterialRequest>()
                    };
                }

                // Convert MaterialResponseDTO to MaterialRequest
                var materialRequests = apiResponse.Data.Select(dto => new MaterialRequest
                {
                    Id = dto.Id,
                    CourseId = dto.CourseId,
                    CourseName = dto.CourseName,
                    Title = dto.Title,
                    Category = dto.Category,
                    FilePath = dto.FilePath,
                    Year = dto.Year,
                    Status = dto.Status,
                    UploadedAt = dto.UploadedAt
                }).ToList();

                return new ApiResponse<List<MaterialRequest>>
                {
                    Success = true,
                    Data = materialRequests,
                    Message = apiResponse.Message
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<MaterialRequest>>
                {
                    Success = false,
                    Message = $"Failed to get requests: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<MaterialRequest>>> GetPendingRequestsAsync()
        {
            var response = await _httpClient.GetAsync($"{API_BASE_URL}/api/CourseMaterials/requests/pending");
            var responseJson = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                // Deserialize as MaterialResponseDTO first (what API actually returns)
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<MaterialResponseDTO>>>(responseJson, options);
                
                if (apiResponse == null)
                {
                    return new ApiResponse<List<MaterialRequest>>
                    {
                        Success = false,
                        Message = "Failed to deserialize response"
                    };
                }

                if (!apiResponse.Success || apiResponse.Data == null)
                {
                    return new ApiResponse<List<MaterialRequest>>
                    {
                        Success = apiResponse.Success,
                        Message = apiResponse.Message,
                        ErrorCode = apiResponse.ErrorCode,
                        Data = new List<MaterialRequest>()
                    };
                }

                // Convert MaterialResponseDTO to MaterialRequest
                var materialRequests = apiResponse.Data.Select(dto => new MaterialRequest
                {
                    Id = dto.Id,
                    CourseId = dto.CourseId,
                    CourseName = dto.CourseName,
                    Title = dto.Title,
                    Category = dto.Category,
                    FilePath = dto.FilePath,
                    Year = dto.Year,
                    Status = dto.Status,
                    UploadedAt = dto.UploadedAt
                }).ToList();

                return new ApiResponse<List<MaterialRequest>>
                {
                    Success = true,
                    Data = materialRequests,
                    Message = apiResponse.Message
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<MaterialRequest>>
                {
                    Success = false,
                    Message = $"Failed to get pending requests: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<MaterialResponseDTO>> ApproveRequestAsync(string materialId)
        {
            var response = await _httpClient.PutAsync($"{API_BASE_URL}/api/CourseMaterials/requests/{Uri.EscapeDataString(materialId)}/approve", null);
            var responseJson = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<MaterialResponseDTO>>(responseJson, options);
                return apiResponse ?? new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "Failed to deserialize response"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = $"Failed to approve request: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<MaterialResponseDTO>> RejectRequestAsync(string materialId)
        {
            var response = await _httpClient.PutAsync($"{API_BASE_URL}/api/CourseMaterials/requests/{Uri.EscapeDataString(materialId)}/reject", null);
            var responseJson = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<MaterialResponseDTO>>(responseJson, options);
                return apiResponse ?? new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "Failed to deserialize response"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = $"Failed to reject request: {ex.Message}"
                };
            }
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
                queryParams.Add($"search={Uri.EscapeDataString(search)}");
            }

            var url = $"{API_BASE_URL}/api/Courses?{string.Join("&", queryParams)}";
            var response = await _httpClient.GetAsync(url);
            var responseJson = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var paginatedResponse = JsonSerializer.Deserialize<PaginatedResponse<CourseDTO>>(responseJson, options);
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
            catch (Exception ex)
            {
                return new ApiResponse<PaginatedResponse<CourseDTO>>
                {
                    Success = false,
                    Message = $"Failed to get courses: {ex.Message}"
                };
            }
        }
    }
}
