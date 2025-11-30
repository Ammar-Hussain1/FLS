using FLS_API.DL.Models;

namespace FLS_API.BL
{
    public class PlaylistService : IPlaylistService
    {
        private readonly SupabaseService _supabase;

        public PlaylistService(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        public async Task<PlaylistRequest> SubmitRequestAsync(PlaylistRequest request)
        {
            request.Status = "Pending";
            request.SubmittedDate = DateTime.UtcNow;
            
            var response = await _supabase.Client.From<PlaylistRequest>().Insert(request);
            return response.Models.FirstOrDefault() ?? request;
        }

        public async Task<List<PlaylistRequest>> GetAllRequestsAsync()
        {
            var response = await _supabase.Client.From<PlaylistRequest>()
                .Order(pr => pr.SubmittedDate, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();
            
            return response.Models;
        }

        public async Task<PlaylistRequest> ApproveRequestAsync(Guid requestId, Guid adminId)
        {
            // Get the request
            var requestResponse = await _supabase.Client.From<PlaylistRequest>()
                .Where(r => r.Id == requestId)
                .Single();
            
            var request = requestResponse;
            if (request == null)
            {
                throw new Exception("Playlist request not found");
            }

            // Update request status
            request.Status = "Approved";
            request.ReviewedDate = DateTime.UtcNow;
            request.ReviewedByAdminId = adminId;
            
            await _supabase.Client.From<PlaylistRequest>().Update(request);

            // Create community playlist
            var communityPlaylist = new CommunityPlaylist
            {
                Name = request.PlaylistName,
                Url = request.Url,
                CourseId = request.CourseId,
                Likes = 0,
                SubmittedByUserId = request.UserId,
                ApprovedByAdminId = adminId,
                CreatedAt = request.SubmittedDate,
                ApprovedAt = DateTime.UtcNow
            };

            await _supabase.Client.From<CommunityPlaylist>().Insert(communityPlaylist);

            return request;
        }

        public async Task<PlaylistRequest> RejectRequestAsync(Guid requestId, Guid adminId, string reason)
        {
            var requestResponse = await _supabase.Client.From<PlaylistRequest>()
                .Where(r => r.Id == requestId)
                .Single();
            
            var request = requestResponse;
            if (request == null)
            {
                throw new Exception("Playlist request not found");
            }

            request.Status = "Rejected";
            request.ReviewedDate = DateTime.UtcNow;
            request.ReviewedByAdminId = adminId;
            request.RejectionReason = reason;
            
            await _supabase.Client.From<PlaylistRequest>().Update(request);

            return request;
        }

        public async Task<List<CommunityPlaylist>> GetPlaylistsForUserCoursesAsync(Guid userId)
        {
            // Get user's enrolled course IDs
            var userCoursesResponse = await _supabase.Client.From<UserCourse>()
                .Where(uc => uc.UserId == userId)
                .Get();
            
            var enrolledCourseIds = userCoursesResponse.Models.Select(uc => uc.CourseId).ToList();

            if (!enrolledCourseIds.Any())
            {
                return new List<CommunityPlaylist>();
            }

            // Get community playlists for those courses
            var playlistsResponse = await _supabase.Client.From<CommunityPlaylist>()
                .Order(cp => cp.Likes, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();
            
            // Filter by enrolled course IDs (Supabase client doesn't support IN clause directly)
            var filteredPlaylists = playlistsResponse.Models
                .Where(p => enrolledCourseIds.Contains(p.CourseId))
                .ToList();

            return filteredPlaylists;
        }

        public async Task<CommunityPlaylist> LikePlaylistAsync(Guid playlistId)
        {
            var playlistResponse = await _supabase.Client.From<CommunityPlaylist>()
                .Where(p => p.Id == playlistId)
                .Single();
            
            var playlist = playlistResponse;
            if (playlist == null)
            {
                throw new Exception("Playlist not found");
            }

            playlist.Likes++;
            await _supabase.Client.From<CommunityPlaylist>().Update(playlist);

            return playlist;
        }
    }
}
