using FLS_API.BL;
using Microsoft.AspNetCore.Mvc;

namespace FLS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimetableController : ControllerBase
    {
        private readonly ITimetableService _timetableService;

        public TimetableController(ITimetableService timetableService)
        {
            _timetableService = timetableService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTimetable()
        {
            var timetable = await _timetableService.GetTimetableAsync();
            return Ok(timetable);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadTimetable(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                using var stream = file.OpenReadStream();
                await _timetableService.ParseAndSaveTimetableAsync(stream);
                return Ok("Timetable uploaded and parsed successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
