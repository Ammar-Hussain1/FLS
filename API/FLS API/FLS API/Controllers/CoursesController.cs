using FLS_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace FLS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        // Dummy data store (in-memory)
        private static List<Course> _courses = new List<Course>
        {
            new Course
            {
                Id = 1,
                Name = "Introduction to Programming",
                Code = "CS101",
                Description = "Fundamentals of programming and problem-solving",
                Credits = 3,
                Credits = 3,
                CreatedDate = DateTime.Now.AddDays(-30)
            },
            new Course
            {
                Id = 2,
                Name = "Data Structures and Algorithms",
                Code = "CS201",
                Description = "Advanced data structures and algorithm design",
                Credits = 4,
                Credits = 4,
                CreatedDate = DateTime.Now.AddDays(-20)
            },
            new Course
            {
                Id = 3,
                Name = "Database Systems",
                Code = "CS301",
                Description = "Design and implementation of database systems",
                Credits = 3,
                Credits = 3,
                CreatedDate = DateTime.Now.AddDays(-10)
            }
        };

        private readonly ILogger<CoursesController> _logger;

        public CoursesController(ILogger<CoursesController> logger)
        {
            _logger = logger;
        }

        // GET: api/courses
        [HttpGet]
        public ActionResult<IEnumerable<Course>> GetCourses()
        {
            return Ok(_courses);
        }

        // GET: api/courses/{id}
        [HttpGet("{id}")]
        public ActionResult<Course> GetCourse(int id)
        {
            var course = _courses.FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound();
            }
            return Ok(course);
        }

        // POST: api/courses
        [HttpPost]
        public ActionResult<Course> CreateCourse([FromBody] Course course)
        {
            if (course == null)
            {
                return BadRequest("Course data is required");
            }

            // Generate new ID
            var newId = _courses.Any() ? _courses.Max(c => c.Id) + 1 : 1;
            course.Id = newId;
            course.CreatedDate = DateTime.Now;

            _courses.Add(course);
            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
        }

        // DELETE: api/courses/{id}
        [HttpDelete("{id}")]
        public ActionResult DeleteCourse(int id)
        {
            var course = _courses.FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            _courses.Remove(course);
            return NoContent();
        }
    }
}

