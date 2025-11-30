using FLS_API.DL.Models;

namespace FLS_API.BL
{
    public interface ITimetableService
    {
        Task ParseAndSaveTimetableAsync(Stream fileStream);
        Task<List<TimeTable>> GetTimetableAsync();
        Task<List<Section>> GetSectionsAsync();
    }
}
