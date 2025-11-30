using FLS_API.DL.Models;
using FLS_API.Models;

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
            // For now, ignore userId and return all approved community playlists,
            // grouped client-side by course. The WPF app already restricts the
            // course list to the user's saved/enrolled courses.
            var playlistsResponse = await _supabase.Client.From<CommunityPlaylist>()
                .Order(cp => cp.Likes, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();

            return playlistsResponse.Models;
        }

        public async Task<CommunityPlaylist> LikePlaylistAsync(Guid playlistId, Guid userId)
        {
            // Ensure playlist exists
            var playlistResponse = await _supabase.Client.From<CommunityPlaylist>()
                .Where(p => p.Id == playlistId)
                .Single();

            var playlist = playlistResponse;
            if (playlist == null)
            {
                throw new Exception("Playlist not found");
            }

            // Prevent duplicate likes by the same user
            var userIdString = userId.ToString();
            var playlistIdString = playlistId.ToString();

            var existingLikeResponse = await _supabase.Client.From<PlaylistLike>()
                .Where(l => l.PlaylistId == playlistIdString && l.UserId == userIdString)
                .Get();

            if (existingLikeResponse.Models.Any())
            {
                // Already liked by this user – signal to caller
                throw new InvalidOperationException("Playlist already liked by this user.");
            }

            // Insert like row
            var like = new PlaylistLike
            {
                Id = Guid.NewGuid().ToString(),
                PlaylistId = playlistIdString,
                UserId = userIdString,
                LikedAt = DateTime.UtcNow
            };

            await _supabase.Client.From<PlaylistLike>().Insert(like);

            // Increment aggregate like counter on playlist
            playlist.Likes++;
            await _supabase.Client.From<CommunityPlaylist>().Update(playlist);

            return playlist;
        }

        public async Task<Dictionary<string, Course>> GetAllCoursesAsync()
        {
            var coursesResponse = await _supabase.Client.From<Course>().Get();
            return coursesResponse.Models.ToDictionary(c => c.Id, c => c);
        }

        public async Task<Course?> GetCourseByIdAsync(string courseId)
        {
            if (string.IsNullOrWhiteSpace(courseId))
                return null;

            try
            {
                var courseResponse = await _supabase.Client.From<Course>()
                    .Where(c => c.Id == courseId)
                    .Single();
                return courseResponse;
            }
            catch
            {
                return null;
            }
        }
    }
}
