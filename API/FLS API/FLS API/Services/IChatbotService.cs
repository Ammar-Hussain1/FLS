using FLS_API.DL.DTOs;

namespace FLS_API.BL
{
    public interface IChatbotService
    {
        Task<string> ProcessMessageAsync(string userId, string message, string apiKey);
    }
}
