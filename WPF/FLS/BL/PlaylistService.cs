using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FLS.DL;
using FLS.Models;

namespace FLS.BL
{
    public class PlaylistService
    {
        private readonly PlaylistApiClient _playlistApiClient;

        public PlaylistService(PlaylistApiClient playlistApiClient)
        {
            _playlistApiClient = playlistApiClient ?? throw new ArgumentNullException(nameof(playlistApiClient));
        }

        public async Task<List<CommunityPlaylist>> GetCommunityPlaylistsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID is required", nameof(userId));
            }

            return await _playlistApiClient.GetCommunityPlaylistsAsync(userId);
        }
    }
}

