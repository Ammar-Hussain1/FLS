using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FLS.DL;
using FLS.Models;

namespace FLS.BL
{
    public class TimetableService
    {
        private readonly TimetableApiClient _timetableApiClient;

        public TimetableService(TimetableApiClient timetableApiClient)
        {
            _timetableApiClient = timetableApiClient ?? throw new ArgumentNullException(nameof(timetableApiClient));
        }

        public async Task UploadTimetableAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path is required", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Timetable file not found", filePath);
            }

            await _timetableApiClient.UploadTimetableAsync(filePath);
        }

        public async Task<List<TimetableDTO>> GetTimetableAsync()
        {
            return await _timetableApiClient.GetTimetableAsync();
        }

        public async Task<List<TimetableDTO>> GetMyTimetableAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID is required", nameof(userId));
            }

            return await _timetableApiClient.GetMyTimetableAsync(userId);
        }
    }
}

