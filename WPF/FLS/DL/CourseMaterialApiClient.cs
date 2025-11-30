using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FLS.Models;

namespace FLS.DL
{
    public class CourseMaterialApiClient : BaseApiClient
    {
        public CourseMaterialApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<CourseWithMaterialsDTO>> GetCourseMaterialsAsync(string courseId, string? category = null)
        {
            var url = $"{API_BASE_URL}/api/CourseMaterials/course/{System.Uri.EscapeDataString(courseId)}";
            if (!string.IsNullOrWhiteSpace(category))
            {
                url += $"?category={System.Uri.EscapeDataString(category)}";
            }

            var response = await HttpClient.GetAsync(url);
            var responseJson = await response.Content.ReadAsStringAsync();

            var apiResponse = await DeserializeResponseAsync<ApiResponse<CourseWithMaterialsDTO>>(response);
            return apiResponse ?? new ApiResponse<CourseWithMaterialsDTO>
            {
                Success = false,
                Message = "Failed to deserialize response"
            };
        }

        public async Task<ApiResponse<MaterialResponseDTO>> GetMaterialAsync(string materialId)
        {
            var response = await HttpClient.GetAsync($"{API_BASE_URL}/api/CourseMaterials/{System.Uri.EscapeDataString(materialId)}");
            var responseJson = await response.Content.ReadAsStringAsync();

            var apiResponse = await DeserializeResponseAsync<ApiResponse<MaterialResponseDTO>>(response);
            return apiResponse ?? new ApiResponse<MaterialResponseDTO>
            {
                Success = false,
                Message = "Failed to deserialize response"
            };
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

                var response = await HttpClient.PostAsync($"{API_BASE_URL}/api/CourseMaterials/upload?userId={System.Uri.EscapeDataString(userId)}", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                var apiResponse = await DeserializeResponseAsync<ApiResponse<MaterialResponseDTO>>(response);
                return apiResponse ?? new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "Failed to deserialize response"
                };
            }
            catch (System.Exception ex)
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

            var content = CreateJsonContent(request);
            var response = await HttpClient.PostAsync($"{API_BASE_URL}/api/CourseMaterials/request?userId={System.Uri.EscapeDataString(userId)}", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            var apiResponse = await DeserializeResponseAsync<ApiResponse<MaterialResponseDTO>>(response);
            return apiResponse ?? new ApiResponse<MaterialResponseDTO>
            {
                Success = false,
                Message = "Failed to deserialize response"
            };
        }

        public async Task<ApiResponse<List<MaterialRequest>>> GetMyRequestsAsync(string userId)
        {
            var response = await HttpClient.GetAsync($"{API_BASE_URL}/api/CourseMaterials/my-requests?userId={System.Uri.EscapeDataString(userId)}");
            var responseJson = await response.Content.ReadAsStringAsync();

            var apiResponse = await DeserializeResponseAsync<ApiResponse<List<MaterialResponseDTO>>>(response);
            
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
                UploadedAt = dto.UploadedAt,
                UploadedBy = dto.UploadedBy,
                UploadedByName = dto.UploadedByName
            }).ToList();

            return new ApiResponse<List<MaterialRequest>>
            {
                Success = true,
                Data = materialRequests,
                Message = apiResponse.Message
            };
        }

        public async Task<ApiResponse<List<MaterialRequest>>> GetPendingRequestsAsync()
        {
            var response = await HttpClient.GetAsync($"{API_BASE_URL}/api/CourseMaterials/requests/pending");
            var responseJson = await response.Content.ReadAsStringAsync();

            var apiResponse = await DeserializeResponseAsync<ApiResponse<List<MaterialResponseDTO>>>(response);
            
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
                UploadedAt = dto.UploadedAt,
                UploadedBy = dto.UploadedBy,
                UploadedByName = dto.UploadedByName
            }).ToList();

            return new ApiResponse<List<MaterialRequest>>
            {
                Success = true,
                Data = materialRequests,
                Message = apiResponse.Message
            };
        }

        public async Task<ApiResponse<MaterialResponseDTO>> ApproveRequestAsync(string materialId)
        {
            var response = await HttpClient.PutAsync($"{API_BASE_URL}/api/CourseMaterials/requests/{System.Uri.EscapeDataString(materialId)}/approve", null);
            var responseJson = await response.Content.ReadAsStringAsync();

            var apiResponse = await DeserializeResponseAsync<ApiResponse<MaterialResponseDTO>>(response);
            return apiResponse ?? new ApiResponse<MaterialResponseDTO>
            {
                Success = false,
                Message = "Failed to deserialize response"
            };
        }

        public async Task<ApiResponse<MaterialResponseDTO>> RejectRequestAsync(string materialId)
        {
            var response = await HttpClient.PutAsync($"{API_BASE_URL}/api/CourseMaterials/requests/{System.Uri.EscapeDataString(materialId)}/reject", null);
            var responseJson = await response.Content.ReadAsStringAsync();

            var apiResponse = await DeserializeResponseAsync<ApiResponse<MaterialResponseDTO>>(response);
            return apiResponse ?? new ApiResponse<MaterialResponseDTO>
            {
                Success = false,
                Message = "Failed to deserialize response"
            };
        }
    }
}

