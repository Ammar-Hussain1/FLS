using FLS_API.DL.Models;
using FLS_API.DL.DTOs;
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

        public async Task<string> ProcessMessageAsync(string userIdStr, string message, string apiKey, List<ChatTurnDTO>? history = null)
        {
            if (!Guid.TryParse(userIdStr, out var userId))
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
                
                // 3. Build context-aware prompt using recent chat history
                var prompt = BuildPrompt(message, memories, courseMaterials, history);
                
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

        private async Task<List<UserMemory>> GetTopMemoriesAsync(Guid userId, int limit)
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

        private async Task<string> GetRelevantCourseMaterialsAsync(string query, Guid userId)
        {
            // TODO: Implement RAG when course materials are available
            // For now, return empty string
            return string.Empty;
        }

        private string BuildPrompt(string userMessage, List<UserMemory> memories, string courseMaterials, List<ChatTurnDTO>? history)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("You are a helpful, friendly study assistant for university students at FAST.");
            sb.AppendLine("Your job is to explain concepts clearly, help with assignments and exam prep,");
            sb.AppendLine("and keep answers focused, structured, and easy to understand.");
            sb.AppendLine("Use the conversation history and notes about the student to stay consistent,");
            sb.AppendLine("but do NOT mention any 'memory system', 'REMEMBER/forget' tags, or how you store information.");
            sb.AppendLine();
            
            // Add memories as silent student profile (not to be mentioned explicitly)
            if (memories.Any())
            {
                sb.AppendLine("Student profile (for your reference only, DO NOT mention these notes explicitly):");
                foreach (var memory in memories)
                {
                    sb.AppendLine($"- {memory.Content} (category: {memory.Category})");
                }
                sb.AppendLine();
            }
            
            // Add course materials if available
            if (!string.IsNullOrEmpty(courseMaterials))
            {
                sb.AppendLine("Relevant course materials (for your reference only, summarise or reference as needed):");
                sb.AppendLine(courseMaterials);
                sb.AppendLine();
            }

            // Add recent conversation history so the model can handle multi‑turn context
            if (history != null && history.Count > 0)
            {
                sb.AppendLine("Recent conversation (most recent last):");
                foreach (var turn in history)
                {
                    var speaker = string.Equals(turn.Role, "user", StringComparison.OrdinalIgnoreCase)
                        ? "Student"
                        : "Assistant";
                    sb.AppendLine($"{speaker}: {turn.Message}");
                }
                sb.AppendLine();
            }
            
            sb.AppendLine("Instructions:");
            sb.AppendLine("1. Answer as a supportive tutor: use clear steps, short paragraphs, and concrete examples.");
            sb.AppendLine("2. Use the recent conversation to understand follow‑up questions and avoid repeating yourself.");
            sb.AppendLine("3. If you need to remember something important for the future, you may append a hidden line");
            sb.AppendLine("   using [REMEMBER: ... | IMPORTANCE: 1-10 | CATEGORY: ...], but do NOT explain this to the student.");
            sb.AppendLine("4. If earlier information is clearly wrong or outdated, you may append [FORGET: <memory-id>] as a");
            sb.AppendLine("   hidden line, without talking about 'forgetting a memory' in your visible answer.");
            sb.AppendLine();
            sb.AppendLine($"Student: {userMessage}");
            sb.AppendLine();
            sb.AppendLine("Assistant:");
            return sb.ToString();
        }

        private async Task ProcessMemoryUpdatesAsync(Guid userId, OpenRouterResponse response)
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
