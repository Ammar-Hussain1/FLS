using FLS_API.BL;
using FLS_API.DL.DTOs;
using FLS_API.DL.Models;
using Microsoft.AspNetCore.Mvc;

namespace FLS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistController : ControllerBase
    {
        private readonly IPlaylistService _playlistService;

        public PlaylistController(IPlaylistService playlistService)
        {
            _playlistService = playlistService;
        }

        [HttpPost("request")]
        public async Task<IActionResult> SubmitRequest([FromBody] SubmitPlaylistRequestDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.Url))
                return BadRequest("Name and URL are required.");

            var request = new PlaylistRequest
            {
                Name = dto.Name,
                PlaylistName = dto.PlaylistName,
                Url = dto.Url,
                CourseId = dto.CourseId,
                UserId = dto.UserId
            };

            var result = await _playlistService.SubmitRequestAsync(request);
            return Ok(result);
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetAllRequests()
        {
            var requests = await _playlistService.GetAllRequestsAsync();
            return Ok(requests);
        }

        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveRequest(Guid id, [FromBody] ApprovePlaylistDTO dto)
        {
            try
            {
                var result = await _playlistService.ApproveRequestAsync(id, dto.AdminId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("reject/{id}")]
        public async Task<IActionResult> RejectRequest(Guid id, [FromBody] RejectPlaylistDTO dto)
        {
            try
            {
                var result = await _playlistService.RejectRequestAsync(id, dto.AdminId, dto.Reason);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("community")]
        public async Task<IActionResult> GetCommunityPlaylists([FromQuery] Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest("Valid user ID is required.");

            var playlists = await _playlistService.GetPlaylistsForUserCoursesAsync(userId);
            return Ok(playlists);
        }

        [HttpPost("like/{id}")]
        public async Task<IActionResult> LikePlaylist(Guid id)
        {
            try
            {
                var result = await _playlistService.LikePlaylistAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
