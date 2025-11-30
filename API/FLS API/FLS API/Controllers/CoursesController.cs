using FLS_API.BL;
using FLS_API.DL.Models;
using FLS_API.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FLS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly SupabaseService _supabaseService;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(SupabaseService supabaseService, ILogger<CoursesController> logger)
        {
            _supabaseService = supabaseService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<CourseDTO>>> GetCourses(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            try
            {
                if (page < 1)
                {
                    return BadRequest("Page number must be greater than 0");
                }
                
                if (pageSize < 1 || pageSize > 100)
                {
                    return BadRequest("Page size must be between 1 and 100");
                }

                var allCoursesResponse = await _supabaseService.Client.From<Course>().Get();
                
                var filteredCourses = allCoursesResponse.Models.AsEnumerable();
                
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.Trim().ToLowerInvariant();
                    filteredCourses = filteredCourses.Where(c => 
                        (!string.IsNullOrEmpty(c.Code) && c.Code.ToLowerInvariant().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(c.Name) && c.Name.ToLowerInvariant().Contains(searchLower))
                    );
                }

                var filteredList = filteredCourses.ToList();
                var totalCount = filteredList.Count;

                var from = (page - 1) * pageSize;
                var paginatedCourses = filteredList
                    .Skip(from)
                    .Take(pageSize)
                    .Select(c => new CourseDTO
                    {
                        Id = c.Id,
                        Code = c.Code,
                        Name = c.Name,
                        Description = c.Description
                    })
                    .ToList();

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var paginatedResponse = new PaginatedResponse<CourseDTO>
                {
                    Data = paginatedCourses,
                    Pagination = new PaginationMetadata
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasPreviousPage = page > 1,
                        HasNextPage = page < totalPages
                    }
                };

                return Ok(paginatedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses");
                return StatusCode(500, "Error retrieving courses");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDTO>> GetCourse(string id)
        {
            try
            {
                var response = await _supabaseService.Client.From<Course>()
                    .Where(c => c.Id == id)
                    .Get();
                
                var course = response.Models.FirstOrDefault();
                if (course == null)
                {
                    return NotFound();
                }
                
                var courseDto = new CourseDTO
                {
                    Id = course.Id,
                    Code = course.Code,
                    Name = course.Name,
                    Description = course.Description
                };
                
                return Ok(courseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course");
                return StatusCode(500, "Error retrieving course");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CourseDTO>> CreateCourse([FromBody] CourseDTO courseDto)
        {
            if (courseDto == null)
            {
                return BadRequest("Course data is required");
            }

            try
            {
                var course = new Course
                {
                    Id = Guid.NewGuid().ToString(),
                    Code = courseDto.Code,
                    Name = courseDto.Name,
                    Description = courseDto.Description
                };
                
                await _supabaseService.Client.From<Course>().Insert(course);
                
                var createdCourseDto = new CourseDTO
                {
                    Id = course.Id,
                    Code = course.Code,
                    Name = course.Name,
                    Description = course.Description
                };
                
                return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, createdCourseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                return StatusCode(500, "Error creating course");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCourse(string id)
        {
            try
            {
                await _supabaseService.Client.From<Course>()
                    .Where(c => c.Id == id)
                    .Delete();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course");
                return StatusCode(500, "Error deleting course");
            }
        }
    }
}

