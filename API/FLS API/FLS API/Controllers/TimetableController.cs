using FLS_API.BL;
using FLS_API.DL.Models;
using FLS_API.DTOs;
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
        public async Task<ActionResult<List<TimetableDTO>>> GetTimetable()
        {
            var timetable = await _timetableService.GetTimetableAsync();
            
            // Fetch all sections to get instructor names
            var sections = await _timetableService.GetSectionsAsync();
            var sectionsDict = sections.ToDictionary(s => s.Id, s => s);
            
            var timetableDtos = timetable.Select(t => 
            {
                var section = sectionsDict.GetValueOrDefault(t.SectionId);
                return new TimetableDTO
                {
                    Id = t.Id,
                    CourseId = t.CourseId,
                    SectionId = t.SectionId,
                    Day = t.Day,
                    Time = t.Time,
                    Subject = t.Subject,
                    Room = t.Room,
                    InstructorName = section?.InstructorName,
                    SectionName = section?.Name
                };
            }).ToList();
            
            return Ok(timetableDtos);
        }

        [HttpGet("my")]
        public async Task<ActionResult<List<TimetableDTO>>> GetMyTimetable([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("User ID is required.");
            }

            // Load all timetable entries and sections once
            var timetable = await _timetableService.GetTimetableAsync();
            if (timetable == null || !timetable.Any())
            {
                return Ok(new List<TimetableDTO>());
            }

            var sections = await _timetableService.GetSectionsAsync();
            var sectionsDict = sections.ToDictionary(s => s.Id, s => s);

            // Load usercourses for this user via SupabaseService resolved from DI
            var supabaseService = HttpContext.RequestServices.GetRequiredService<SupabaseService>();
            var client = supabaseService.Client;
            var userCoursesResponse = await client.From<FLS_API.Models.UserCourseModel>()
                .Where(uc => uc.UserId == userId)
                .Get();

            if (!userCoursesResponse.Models.Any())
            {
                return Ok(new List<TimetableDTO>());
            }

            var userCourses = userCoursesResponse.Models;
            var courseIds = userCourses.Select(uc => uc.CourseId).Distinct().ToHashSet();
            var sectionIds = userCourses.Select(uc => uc.SectionId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToHashSet();

            // Filter timetable entries to only those belonging to the user's courses/sections
            var filteredTimetable = timetable
                .Where(t => courseIds.Contains(t.CourseId) &&
                            (sectionIds.Count == 0 ||
                             string.IsNullOrEmpty(t.SectionId) ||
                             sectionIds.Contains(t.SectionId)))
                .ToList();

            var timetableDtos = filteredTimetable.Select(t =>
            {
                sectionsDict.TryGetValue(t.SectionId, out var section);
                return new TimetableDTO
                {
                    Id = t.Id,
                    CourseId = t.CourseId,
                    SectionId = t.SectionId,
                    Day = t.Day,
                    Time = t.Time,
                    Subject = t.Subject,
                    Room = t.Room,
                    InstructorName = section?.InstructorName,
                    SectionName = section?.Name
                };
            }).ToList();

            return Ok(timetableDtos);
        }

        [HttpPost("sync")]
        public async Task<IActionResult> SyncTimetable()
        {
            var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
            var filePath = configuration["TimetableSettings:FilePath"];

            if (string.IsNullOrEmpty(filePath))
                return BadRequest("Timetable file path is not configured.");

            Stream fileStream = null;
            string tempFilePath = null;

            try
            {
                if (filePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle URL (Google Drive or direct link)
                    using var httpClient = new HttpClient();
                    
                    // Convert Google Drive View URL to Download URL if needed
                    // Format: https://drive.google.com/file/d/FILE_ID/view... -> https://drive.google.com/uc?export=download&id=FILE_ID
                    if (filePath.Contains("drive.google.com") && filePath.Contains("/d/"))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(filePath, @"/d/([a-zA-Z0-9_-]+)");
                        if (match.Success)
                        {
                            var fileId = match.Groups[1].Value;
                            filePath = $"https://drive.google.com/uc?export=download&id={fileId}";
                        }
                    }
                    else if (filePath.Contains("docs.google.com/spreadsheets/d/"))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(filePath, @"/d/([a-zA-Z0-9_-]+)");
                        if (match.Success)
                        {
                            var fileId = match.Groups[1].Value;
                            filePath = $"https://docs.google.com/spreadsheets/d/{fileId}/export?format=xlsx";
                        }
                    }

                    var response = await httpClient.GetAsync(filePath);
                    if (!response.IsSuccessStatusCode)
                        return BadRequest($"Failed to download file from URL. Status: {response.StatusCode}");

                    fileStream = await response.Content.ReadAsStreamAsync();
                }
                else
                {
                    // Handle Local File
                    if (!System.IO.File.Exists(filePath))
                        return BadRequest($"File not found at path: {filePath}");

                    fileStream = System.IO.File.OpenRead(filePath);
                }

                await _timetableService.ParseAndSaveTimetableAsync(fileStream);
                return Ok("Timetable synced successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error syncing timetable: {ex}");
                return StatusCode(500, $"Error syncing timetable: {ex.Message}");
            }
            finally
            {
                fileStream?.Dispose();
                if (tempFilePath != null && System.IO.File.Exists(tempFilePath))
                    System.IO.File.Delete(tempFilePath);
            }
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
