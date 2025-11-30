using FLS_API.DL.Models;

namespace FLS_API.BL
{
    public interface IPlaylistService
    {
        Task<PlaylistRequest> SubmitRequestAsync(PlaylistRequest request);
        Task<List<PlaylistRequest>> GetAllRequestsAsync();
        Task<PlaylistRequest> ApproveRequestAsync(Guid requestId, Guid adminId);
        Task<PlaylistRequest> RejectRequestAsync(Guid requestId, Guid adminId, string reason);
        Task<List<CommunityPlaylist>> GetPlaylistsForUserCoursesAsync(Guid userId);
        Task<CommunityPlaylist> LikePlaylistAsync(Guid playlistId);
    }
}
