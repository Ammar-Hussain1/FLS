using System;
using System.IO;
using System.Threading.Tasks;
using FLS.DL;
using FLS.Models;

namespace FLS.BL
{
    public class CourseMaterialService
    {
        private readonly CourseMaterialApiClient _courseMaterialApiClient;

        public CourseMaterialService(CourseMaterialApiClient courseMaterialApiClient)
        {
            _courseMaterialApiClient = courseMaterialApiClient ?? throw new ArgumentNullException(nameof(courseMaterialApiClient));
        }

        public async Task<ApiResponse<CourseWithMaterialsDTO>> GetCourseMaterialsAsync(string courseId, string? category = null)
        {
            if (string.IsNullOrWhiteSpace(courseId))
            {
                return new ApiResponse<CourseWithMaterialsDTO>
                {
                    Success = false,
                    Message = "Course ID is required",
                    ErrorCode = "INVALID_COURSE_ID"
                };
            }

            return await _courseMaterialApiClient.GetCourseMaterialsAsync(courseId, category);
        }

        public async Task<ApiResponse<MaterialResponseDTO>> GetMaterialAsync(string materialId)
        {
            if (string.IsNullOrWhiteSpace(materialId))
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "Material ID is required",
                    ErrorCode = "INVALID_MATERIAL_ID"
                };
            }

            return await _courseMaterialApiClient.GetMaterialAsync(materialId);
        }

        public async Task<ApiResponse<MaterialResponseDTO>> UploadMaterialAsync(string userId, string filePath, string courseName, string fileType, int? year = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "User ID is required",
                    ErrorCode = "INVALID_USER_ID"
                };
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "File path is required",
                    ErrorCode = "INVALID_FILE_PATH"
                };
            }

            if (!File.Exists(filePath))
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "File not found",
                    ErrorCode = "FILE_NOT_FOUND"
                };
            }

            if (string.IsNullOrWhiteSpace(courseName))
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "Course name is required",
                    ErrorCode = "INVALID_COURSE_NAME"
                };
            }

            if (string.IsNullOrWhiteSpace(fileType))
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "File type is required",
                    ErrorCode = "INVALID_FILE_TYPE"
                };
            }

            return await _courseMaterialApiClient.UploadMaterialAsync(userId, filePath, courseName, fileType, year);
        }

        public async Task<ApiResponse<MaterialResponseDTO>> RequestMaterialAsync(string userId, string fileLink, string courseName, string fileType, int? year = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "User ID is required",
                    ErrorCode = "INVALID_USER_ID"
                };
            }

            if (string.IsNullOrWhiteSpace(fileLink))
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "File link is required",
                    ErrorCode = "INVALID_FILE_LINK"
                };
            }

            if (string.IsNullOrWhiteSpace(courseName))
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "Course name is required",
                    ErrorCode = "INVALID_COURSE_NAME"
                };
            }

            if (string.IsNullOrWhiteSpace(fileType))
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "File type is required",
                    ErrorCode = "INVALID_FILE_TYPE"
                };
            }

            return await _courseMaterialApiClient.RequestMaterialAsync(userId, fileLink, courseName, fileType, year);
        }

        public async Task<ApiResponse<List<MaterialRequest>>> GetMyRequestsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ApiResponse<List<MaterialRequest>>
                {
                    Success = false,
                    Message = "User ID is required",
                    ErrorCode = "INVALID_USER_ID"
                };
            }

            return await _courseMaterialApiClient.GetMyRequestsAsync(userId);
        }

        public async Task<ApiResponse<List<MaterialRequest>>> GetPendingRequestsAsync()
        {
            return await _courseMaterialApiClient.GetPendingRequestsAsync();
        }

        public async Task<ApiResponse<MaterialResponseDTO>> ApproveRequestAsync(string materialId)
        {
            if (string.IsNullOrWhiteSpace(materialId))
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "Material ID is required",
                    ErrorCode = "INVALID_MATERIAL_ID"
                };
            }

            return await _courseMaterialApiClient.ApproveRequestAsync(materialId);
        }

        public async Task<ApiResponse<MaterialResponseDTO>> RejectRequestAsync(string materialId)
        {
            if (string.IsNullOrWhiteSpace(materialId))
            {
                return new ApiResponse<MaterialResponseDTO>
                {
                    Success = false,
                    Message = "Material ID is required",
                    ErrorCode = "INVALID_MATERIAL_ID"
                };
            }

            return await _courseMaterialApiClient.RejectRequestAsync(materialId);
        }
    }
}

