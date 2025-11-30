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

            var responseDto = new PlaylistRequestDTO
            {
                Id = result.Id,
                Name = result.Name,
                PlaylistName = result.PlaylistName,
                Url = result.Url,
                CourseId = result.CourseId,
                UserId = result.UserId,
                Status = result.Status,
                SubmittedDate = result.SubmittedDate
            };

            return Ok(responseDto);
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetAllRequests()
        {
            var requests = await _playlistService.GetAllRequestsAsync();
            var dtos = requests.Select(r => new PlaylistRequestDTO
            {
                Id = r.Id,
                Name = r.Name,
                PlaylistName = r.PlaylistName,
                Url = r.Url,
                CourseId = r.CourseId,
                UserId = r.UserId,
                Status = r.Status,
                SubmittedDate = r.SubmittedDate
            }).ToList();

            return Ok(dtos);
        }

        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveRequest(Guid id, [FromBody] ApprovePlaylistDTO dto)
        {
            try
            {
                await _playlistService.ApproveRequestAsync(id, dto.AdminId);
                return NoContent();
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
                await _playlistService.RejectRequestAsync(id, dto.AdminId, dto.Reason);
                return NoContent();
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

            var dtos = playlists.Select(p => new CommunityPlaylistDTO
            {
                Id = p.Id,
                Name = p.Name,
                Url = p.Url,
                CourseId = p.CourseId,
                Likes = p.Likes
            }).ToList();

            return Ok(dtos);
        }

        [HttpPost("like/{id}")]
        public async Task<IActionResult> LikePlaylist(Guid id, [FromQuery] Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    return BadRequest("Valid user ID is required.");
                }

                await _playlistService.LikePlaylistAsync(id, userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                // User has already liked this playlist
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
