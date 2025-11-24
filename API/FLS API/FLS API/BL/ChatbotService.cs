using FLS_API.DL;
using FLS_API.DL.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace FLS_API.BL
{
    public class ChatbotService : IChatbotService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ChatbotService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<string> ProcessMessageAsync(string userIdStr, string message)
        {
            return "Test";
        }
    }
}
