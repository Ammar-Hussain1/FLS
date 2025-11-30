using FLS_API.BL;
using FLS_API.DL.Models;
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
        public async Task<ActionResult<List<TimeTable>>> GetTimetable()
        {
            var timetable = await _timetableService.GetTimetableAsync();
            return Ok(timetable);
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
