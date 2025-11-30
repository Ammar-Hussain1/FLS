using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FLS.BL;

namespace FLS.DL
{
    public class ChatApiClient : BaseApiClient
    {
        public ChatApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ChatResponse> SendChatMessageAsync(ChatRequest request)
        {
            var content = CreateJsonContent(request);
            var response = await HttpClient.PostAsync($"{API_BASE_URL}/api/Chat/send", content);
            
            await EnsureSuccessStatusCodeAsync(response);
            
            var chatResponse = await DeserializeResponseAsync<ChatResponse>(response);
            return chatResponse ?? throw new System.Exception("Failed to deserialize API response");
        }
    }
}

