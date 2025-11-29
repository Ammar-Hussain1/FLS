using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FLS_API.BL;
using FLS_API.DTOs;
using FLS_API.Models;
using FLS_API.Utilities;
using System.IO;

namespace FLS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseMaterialsController : ControllerBase
    {
        private readonly SupabaseService _supabaseService;

        public CourseMaterialsController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ApiResponse.ErrorResponse("User ID is required.", "INVALID_USER_ID").ToActionResult();

            var result = await _supabaseService.GetUserCoursesAsync(userId);
            return result.ToActionResult();
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseMaterials(string courseId, [FromQuery] string? category = null)
        {
            if (string.IsNullOrWhiteSpace(courseId))
                return ApiResponse.ErrorResponse("Course ID is required.", "INVALID_COURSE_ID").ToActionResult();

            var result = await _supabaseService.GetCourseMaterialsAsync(courseId, category);
            return result.ToActionResult();
        }

        [HttpGet("{materialId}")]
        public async Task<IActionResult> GetMaterial(string materialId)
        {
            if (string.IsNullOrWhiteSpace(materialId))
                return ApiResponse.ErrorResponse("Material ID is required.", "INVALID_MATERIAL_ID").ToActionResult();

            var result = await _supabaseService.GetMaterialByIdAsync(materialId);
            return result.ToActionResult();
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestMaterial([FromBody] MaterialRequestDTO request, [FromQuery] string userId)
        {
            if (request == null)
                return ApiResponse.ErrorResponse("Request body is required.", "INVALID_REQUEST").ToActionResult();

            if (!ModelState.IsValid)
                return ApiResponse.ErrorResponse("Invalid request data.", "VALIDATION_ERROR").ToActionResult();

            if (string.IsNullOrWhiteSpace(userId))
                return ApiResponse.ErrorResponse("User ID is required.", "INVALID_USER_ID").ToActionResult();

            var result = await _supabaseService.RequestMaterialAsync(userId, request);
            return result.ToActionResult(StatusCodes.Status201Created);
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMaterial(
            [FromForm] FileUploadDTO uploadDto,
            [FromQuery] string userId)
        {
            if (uploadDto == null)
                return ApiResponse.ErrorResponse("Upload data is required.", "INVALID_REQUEST").ToActionResult();

            if (uploadDto.File == null || uploadDto.File.Length == 0)
                return ApiResponse.ErrorResponse("File is required.", "INVALID_FILE").ToActionResult();

            var fileExtension = Path.GetExtension(uploadDto.File.FileName).ToLowerInvariant();
            if (fileExtension != ".pdf")
            {
                return ApiResponse.ErrorResponse("Only PDF files are allowed.", "INVALID_FILE_FORMAT").ToActionResult();
            }

            if (uploadDto.File.ContentType != "application/pdf" && !uploadDto.File.ContentType.Contains("pdf"))
            {
                return ApiResponse.ErrorResponse("Only PDF files are allowed.", "INVALID_FILE_FORMAT").ToActionResult();
            }

            if (string.IsNullOrWhiteSpace(uploadDto.CourseName))
                return ApiResponse.ErrorResponse("Course name is required.", "INVALID_COURSE_NAME").ToActionResult();

            if (string.IsNullOrWhiteSpace(uploadDto.FileType))
                return ApiResponse.ErrorResponse("File type is required.", "INVALID_FILE_TYPE").ToActionResult();

            if (string.IsNullOrWhiteSpace(userId))
                return ApiResponse.ErrorResponse("User ID is required.", "INVALID_USER_ID").ToActionResult();

            var validCategories = new[] { "assignments", "books", "course outline", "quizzes", "Midterm 1", "Midterm 2", "Final" };
            if (!validCategories.Contains(uploadDto.FileType, StringComparer.OrdinalIgnoreCase))
            {
                return ApiResponse.ErrorResponse($"Invalid file type. Must be one of: {string.Join(", ", validCategories)}", "INVALID_FILE_TYPE").ToActionResult();
            }

            try
            {
                var courseResponse = await _supabaseService.Client.From<Course>()
                    .Where(c => c.Name == uploadDto.CourseName || c.Code == uploadDto.CourseName)
                    .Get();
                
                var course = courseResponse.Models.FirstOrDefault();
                if (course == null)
                {
                    return ApiResponse.ErrorResponse("Course not found.", "COURSE_NOT_FOUND").ToActionResult();
                }

                using var fileStream = uploadDto.File.OpenReadStream();
                var uploadResult = await _supabaseService.UploadFileToStorageAsync(
                    fileStream, 
                    uploadDto.File.FileName, 
                    "application/pdf",
                    course.Name,  
                    uploadDto.FileType
                );
                
                if (!uploadResult.IsSuccess || string.IsNullOrEmpty(uploadResult.Data))
                {
                    return ApiResponse.ErrorResponse(uploadResult.ErrorMessage ?? "Failed to upload file", uploadResult.ErrorCode ?? "UPLOAD_FAILED").ToActionResult();
                }

                var request = new MaterialRequestDTO
                {
                    FileLink = uploadResult.Data,
                    CourseName = uploadDto.CourseName,
                    FileType = uploadDto.FileType,
                    Year = uploadDto.Year
                };

                var result = await _supabaseService.RequestMaterialAsync(userId, request);
                return result.ToActionResult(StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return ApiResponse.ErrorResponse($"Failed to upload material: {ex.Message}", "UPLOAD_FAILED").ToActionResult();
            }
        }

        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRequests([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ApiResponse.ErrorResponse("User ID is required.", "INVALID_USER_ID").ToActionResult();

            var result = await _supabaseService.GetUserMaterialRequestsAsync(userId);
            return result.ToActionResult();
        }

        [HttpGet("requests/pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var result = await _supabaseService.GetPendingMaterialRequestsAsync();
            return result.ToActionResult();
        }

        [HttpPut("requests/{materialId}/approve")]
        public async Task<IActionResult> ApproveRequest(string materialId)
        {
            if (string.IsNullOrWhiteSpace(materialId))
                return ApiResponse.ErrorResponse("Material ID is required.", "INVALID_MATERIAL_ID").ToActionResult();

            var result = await _supabaseService.ApproveMaterialRequestAsync(materialId);
            return result.ToActionResult();
        }

        [HttpPut("requests/{materialId}/reject")]
        public async Task<IActionResult> RejectRequest(string materialId)
        {
            if (string.IsNullOrWhiteSpace(materialId))
                return ApiResponse.ErrorResponse("Material ID is required.", "INVALID_MATERIAL_ID").ToActionResult();

            var result = await _supabaseService.RejectMaterialRequestAsync(materialId);
            return result.ToActionResult();
        }
    }
}

