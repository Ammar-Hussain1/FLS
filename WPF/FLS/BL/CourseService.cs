using System;
using System.Threading.Tasks;
using FLS.DL;
using FLS.Models;

namespace FLS.BL
{
    public class CourseService
    {
        private readonly CourseApiClient _courseApiClient;

        public CourseService(CourseApiClient courseApiClient)
        {
            _courseApiClient = courseApiClient ?? throw new ArgumentNullException(nameof(courseApiClient));
        }

        public async Task<ApiResponse<PaginatedResponse<CourseDTO>>> GetCoursesAsync(int page = 1, int pageSize = 10, string? search = null)
        {
            if (page < 1)
            {
                return new ApiResponse<PaginatedResponse<CourseDTO>>
                {
                    Success = false,
                    Message = "Page number must be greater than 0",
                    ErrorCode = "INVALID_PAGE"
                };
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return new ApiResponse<PaginatedResponse<CourseDTO>>
                {
                    Success = false,
                    Message = "Page size must be between 1 and 100",
                    ErrorCode = "INVALID_PAGE_SIZE"
                };
            }

            return await _courseApiClient.GetCoursesAsync(page, pageSize, search);
        }
    }
}

