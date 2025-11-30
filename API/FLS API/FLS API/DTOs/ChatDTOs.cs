namespace FLS_API.DL.DTOs
{
    public class ChatRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public List<ChatTurnDTO>? History { get; set; }
    }

    public class ChatResponse
    {
        public string Response { get; set; } = string.Empty;
    }

    public class ChatTurnDTO
    {
        public string Role { get; set; } = string.Empty; // "user" or "assistant"
        public string Message { get; set; } = string.Empty;
    }
}
