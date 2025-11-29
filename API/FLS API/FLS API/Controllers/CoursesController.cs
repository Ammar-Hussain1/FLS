using FLS_API.BL;
using FLS_API.Models;
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
        public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
        {
            try
            {
                var response = await _supabaseService.Client.From<Course>().Get();
                return Ok(response.Models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses");
                return StatusCode(500, "Error retrieving courses");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(string id)
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
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course");
                return StatusCode(500, "Error retrieving course");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Course>> CreateCourse([FromBody] Course course)
        {
            if (course == null)
            {
                return BadRequest("Course data is required");
            }

            try
            {
                course.Id = Guid.NewGuid().ToString();
                await _supabaseService.Client.From<Course>().Insert(course);
                return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
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

