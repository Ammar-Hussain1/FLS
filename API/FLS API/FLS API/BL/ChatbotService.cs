using FLS_API.DL;
using FLS_API.DL.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FLS_API.BL
{
    public class ChatbotService : IChatbotService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const int MAX_MEMORIES = 100;
        private const int TOP_MEMORIES_TO_KEEP = 50;

        public ChatbotService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> ProcessMessageAsync(string userIdStr, string message, string apiKey)
        {
            if (!int.TryParse(userIdStr, out var userId))
            {
                return "Invalid user ID";
            }

            // Create Gemini client with user's API key
            var geminiClient = new GeminiApiClient(apiKey);

            try
            {
                // 1. Get user memories (top 20 by importance)
                var memories = await GetTopMemoriesAsync(userId, limit: 20);
                
                // 2. RAG: Get relevant course materials (if applicable)
                var courseMaterials = await GetRelevantCourseMaterialsAsync(userMessage, userId);
                
                // 3. Build context-aware prompt (no chat history - it's local now)
                var prompt = BuildPrompt(userMessage, memories, courseMaterials);
                
                // 4. Call Gemini API
                var aiResponse = await geminiClient.GenerateContentAsync(prompt);
                
                // 5. Process memory updates
                await ProcessMemoryUpdatesAsync(userId, aiResponse);
                
                // 6. Prune old memories if needed
                await PruneMemoriesIfNeededAsync(userId, geminiClient);
                
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
            var memories = await _context.UserMemory
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.Importance)
                .ThenByDescending(m => m.CreatedAt)
                .Take(limit)
                .ToListAsync();

            // Update last accessed time
            foreach (var memory in memories)
            {
                memory.LastAccessedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

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

        private async Task ProcessMemoryUpdatesAsync(int userId, GeminiResponse response)
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
                
                _context.UserMemory.Add(memory);
            }
            
            // Delete forgotten memories
            if (response.MemoryDeletions.Any())
            {
                var memoriesToDelete = await _context.UserMemory
                    .Where(m => m.UserId == userId && response.MemoryDeletions.Contains(m.Id))
                    .ToListAsync();
                
                _context.UserMemory.RemoveRange(memoriesToDelete);
            }
            
            await _context.SaveChangesAsync();
        }



        private async Task PruneMemoriesIfNeededAsync(int userId, GeminiApiClient geminiClient)
        {
            var memoryCount = await _context.UserMemory.CountAsync(m => m.UserId == userId);
            
            if (memoryCount > MAX_MEMORIES)
            {
                // Get all memories sorted by importance
                var allMemories = await _context.UserMemory
                    .Where(m => m.UserId == userId)
                    .OrderByDescending(m => m.Importance)
                    .ThenByDescending(m => m.LastAccessedAt ?? m.CreatedAt)
                    .ToListAsync();
                
                // Keep top 50, summarize the rest
                var toKeep = allMemories.Take(TOP_MEMORIES_TO_KEEP).ToList();
                var toSummarize = allMemories.Skip(TOP_MEMORIES_TO_KEEP).ToList();
                
                if (toSummarize.Any())
                {
                    // Create summary using AI
                    var summaryPrompt = $@"Summarize these memories into a concise paragraph (max 200 words):
{string.Join("\n", toSummarize.Select(m => $"- {m.Content}"))}";
                    
                    var summaryResponse = await geminiClient.GenerateContentAsync(summaryPrompt, temperature: 0.3);
                    
                    // Add summary as a new memory
                    var summaryMemory = new UserMemory
                    {
                        UserId = userId,
                        Content = summaryResponse.CleanText,
                        Importance = 6,
                        Category = "summary",
                        IsSummary = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _context.UserMemory.Add(summaryMemory);
                    
                    // Delete old memories
                    _context.UserMemory.RemoveRange(toSummarize);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
