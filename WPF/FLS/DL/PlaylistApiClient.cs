using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FLS.Models;

namespace FLS.DL
{
    public class PlaylistApiClient : BaseApiClient
    {
        public PlaylistApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<List<CommunityPlaylist>> GetCommunityPlaylistsAsync(string userId)
        {
            var response = await HttpClient.GetAsync($"{API_BASE_URL}/api/Playlist/community?userId={System.Uri.EscapeDataString(userId)}");
            await EnsureSuccessStatusCodeAsync(response);

            var playlists = await DeserializeResponseAsync<List<CommunityPlaylist>>(response);
            return playlists ?? new List<CommunityPlaylist>();
        }
    }
}

