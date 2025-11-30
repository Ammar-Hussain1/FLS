using FLS_API.BL;
using FLS_API.DL.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FLS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
                return BadRequest("Message cannot be empty.");

            if (string.IsNullOrEmpty(request.ApiKey))
                return BadRequest("API key is required.");

            var response = await _chatbotService.ProcessMessageAsync(request.UserId, request.Message, request.ApiKey, request.History);
            return Ok(new ChatResponse { Response = response });
        }
    }
}
