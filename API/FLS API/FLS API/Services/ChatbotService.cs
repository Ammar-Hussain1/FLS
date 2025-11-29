using FLS_API.DL.Models;
using System.Text;

namespace FLS_API.BL
{
    public class ChatbotService : IChatbotService
    {
        private readonly SupabaseService _supabase;
        private readonly IConfiguration _configuration;
        private const int MAX_MEMORIES = 100;
        private const int TOP_MEMORIES_TO_KEEP = 50;

        public ChatbotService(SupabaseService supabase, IConfiguration configuration)
        {
            _supabase = supabase;
            _configuration = configuration;
        }

        public async Task<string> ProcessMessageAsync(string userIdStr, string message, string apiKey)
        {
            if (!int.TryParse(userIdStr, out var userId))
            {
                return "Invalid user ID";
            }

            // Create OpenRouter client with user's API key
            var openRouterClient = new OpenRouterApiClient(apiKey);

            try
            {
                // 1. Get user memories (top 20 by importance)
                var memories = await GetTopMemoriesAsync(userId, limit: 20);
                
                // 2. RAG: Get relevant course materials (if applicable)
                var courseMaterials = await GetRelevantCourseMaterialsAsync(message, userId);
                
                // 3. Build context-aware prompt (no chat history - it's local now)
                var prompt = BuildPrompt(message, memories, courseMaterials);
                
                // 4. Call OpenRouter API
                var aiResponse = await openRouterClient.GenerateContentAsync(prompt);
                
                // 5. Process memory updates
                await ProcessMemoryUpdatesAsync(userId, aiResponse);
                
                return aiResponse.CleanText;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in chatbot: {ex.Message}");
                return "I'm sorry, I encountered an error. Please try again.";
            }
        }

        private async Task<List<UserMemory>> GetTopMemoriesAsync(int userId, int limit)
        {
            var response = await _supabase.Client.From<UserMemory>()
                .Where(m => m.UserId == userId)
                .Order(m => m.Importance, Supabase.Postgrest.Constants.Ordering.Descending)
                .Order(m => m.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(limit)
                .Get();

            var memories = response.Models;

            // Update last accessed time (optional, might skip to reduce writes)
            // For now skipping to keep it simple and fast
            
            return memories;
        }

        private async Task<string> GetRelevantCourseMaterialsAsync(string query, int userId)
        {
            // TODO: Implement RAG when course materials are available
            // For now, return empty string
            return string.Empty;
        }

        private string BuildPrompt(string userMessage, List<UserMemory> memories, string courseMaterials)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("You are a friendly AI learning companion and personal assistant. You remember important things about the user and help them with their studies.");
            sb.AppendLine();
            
            // Add memories
            if (memories.Any())
            {
                sb.AppendLine("What you remember about the user:");
                foreach (var memory in memories)
                {
                    sb.AppendLine($"- [{memory.Id}] {memory.Content} (importance: {memory.Importance}, category: {memory.Category})");
                }
                sb.AppendLine();
            }
            
            // Add course materials if available
            if (!string.IsNullOrEmpty(courseMaterials))
            {
                sb.AppendLine("Relevant course materials:");
                sb.AppendLine(courseMaterials);
                sb.AppendLine();
            }
            
            sb.AppendLine("Instructions:");
            sb.AppendLine("1. Respond to the user in a friendly, supportive, and conversational way");
            sb.AppendLine("2. Reference memories when relevant to show you remember them");
            sb.AppendLine("3. If the user shares something worth remembering, output it in this format:");
            sb.AppendLine("   [REMEMBER: <what to remember> | IMPORTANCE: <1-10> | CATEGORY: <personal/academic/preferences/goals>]");
            sb.AppendLine("4. If a memory is no longer relevant or contradicted, output:");
            sb.AppendLine("   [FORGET: <memory id>]");
            sb.AppendLine("5. Keep memory updates on separate lines at the end of your response");
            sb.AppendLine();
            sb.AppendLine($"User: {userMessage}");
            sb.AppendLine();
            sb.AppendLine("AI:");
            
            return sb.ToString();
        }

        private async Task ProcessMemoryUpdatesAsync(int userId, OpenRouterResponse response)
        {
            // Add new memories
            foreach (var update in response.MemoryUpdates)
            {
                var memory = new UserMemory
                {
                    UserId = userId,
                    Content = update.Content,
                    Importance = update.Importance,
                    Category = update.Category,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _supabase.Client.From<UserMemory>().Insert(memory);
            }
            
            // Delete forgotten memories
            if (response.MemoryDeletions.Any())
            {
                foreach (var memoryId in response.MemoryDeletions)
                {
                    await _supabase.Client.From<UserMemory>()
                        .Where(m => m.UserId == userId && m.Id == memoryId)
                        .Delete();
                }
            }
        }


    }
}
