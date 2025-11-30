using FLS_API.BL;
using FLS_API.DTOs;
using FLS_API.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace FLS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserCoursesController : ControllerBase
    {
        private readonly SupabaseService _supabaseService;

        public UserCoursesController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        [HttpPost]
        public async Task<IActionResult> AddUserCourse([FromBody] AddUserCourseRequestDTO request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.UserId) ||
                string.IsNullOrWhiteSpace(request.CourseId))
            {
                return ApiResponse.ErrorResponse("UserId and CourseId are required.", "INVALID_REQUEST")
                    .ToActionResult();
            }

            var result = await _supabaseService.AddUserCourseAsync(request.UserId, request.CourseId, request.SectionName);
            return result.ToActionResult(StatusCodes.Status201Created);
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveUserCourse([FromQuery] string userId, [FromQuery] string courseId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(courseId))
            {
                return ApiResponse.ErrorResponse("UserId and CourseId are required.", "INVALID_REQUEST")
                    .ToActionResult();
            }

            var result = await _supabaseService.RemoveUserCourseAsync(userId, courseId);
            return result.ToActionResult(StatusCodes.Status200OK);
        }
    }
}

