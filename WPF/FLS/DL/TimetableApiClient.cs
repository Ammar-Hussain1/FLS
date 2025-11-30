using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FLS.Models;

namespace FLS.DL
{
    public class TimetableApiClient : BaseApiClient
    {
        public TimetableApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task UploadTimetableAsync(string filePath)
        {
            using var content = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(filePath);
            using var fileContent = new StreamContent(fileStream);
            
            content.Add(fileContent, "file", Path.GetFileName(filePath));
            
            var response = await HttpClient.PostAsync($"{API_BASE_URL}/api/Timetable/upload", content);
            await EnsureSuccessStatusCodeAsync(response);
        }

        public async Task<List<TimetableDTO>> GetTimetableAsync()
        {
            var response = await HttpClient.GetAsync($"{API_BASE_URL}/api/Timetable");
            await EnsureSuccessStatusCodeAsync(response);

            var timetable = await DeserializeResponseAsync<List<TimetableDTO>>(response);
            return timetable ?? new List<TimetableDTO>();
        }

        public async Task<List<TimetableDTO>> GetMyTimetableAsync(string userId)
        {
            var response = await HttpClient.GetAsync($"{API_BASE_URL}/api/Timetable/my?userId={System.Uri.EscapeDataString(userId)}");
            await EnsureSuccessStatusCodeAsync(response);

            var timetable = await DeserializeResponseAsync<List<TimetableDTO>>(response);
            return timetable ?? new List<TimetableDTO>();
        }
    }
}

