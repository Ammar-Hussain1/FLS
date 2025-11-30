using System;
using System.Threading.Tasks;
using FLS.DL;
using FLS.Models;

namespace FLS.BL
{
    public class UserCourseService
    {
        private readonly UserCourseApiClient _userCourseApiClient;

        public UserCourseService(UserCourseApiClient userCourseApiClient)
        {
            _userCourseApiClient = userCourseApiClient ?? throw new ArgumentNullException(nameof(userCourseApiClient));
        }

        public async Task<ApiResponse<object>> AddUserCourseAsync(string userId, string courseId, string? sectionName)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "User ID is required",
                    ErrorCode = "INVALID_USER_ID"
                };
            }

            if (string.IsNullOrWhiteSpace(courseId))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Course ID is required",
                    ErrorCode = "INVALID_COURSE_ID"
                };
            }

            return await _userCourseApiClient.AddUserCourseAsync(userId, courseId, sectionName);
        }

        public async Task<ApiResponse<object>> RemoveUserCourseAsync(string userId, string courseId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "User ID is required",
                    ErrorCode = "INVALID_USER_ID"
                };
            }

            if (string.IsNullOrWhiteSpace(courseId))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Course ID is required",
                    ErrorCode = "INVALID_COURSE_ID"
                };
            }

            return await _userCourseApiClient.RemoveUserCourseAsync(userId, courseId);
        }

        public async Task<ApiResponse<List<UserCourseDTO>>> GetMyCoursesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ApiResponse<List<UserCourseDTO>>
                {
                    Success = false,
                    Message = "User ID is required",
                    ErrorCode = "INVALID_USER_ID"
                };
            }

            return await _userCourseApiClient.GetMyCoursesAsync(userId);
        }
    }
}

