using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FLS.Models;

namespace FLS.DL
{
    /// <summary>
    /// Data access layer for local chat history storage
    /// Manages reading/writing chat messages to local JSON file
    /// </summary>
    public class ChatHistoryRepository
    {
        private const int MAX_MESSAGES = 50;
        private readonly string _historyPath;

        public ChatHistoryRepository()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "FLS"
            );

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _historyPath = Path.Combine(appDataPath, "chat_history.json");
        }

        public List<ChatMessage> LoadHistory()
        {
            try
            {
                if (!File.Exists(_historyPath))
                {
                    return new List<ChatMessage>();
                }

                var json = File.ReadAllText(_historyPath);
                var history = JsonSerializer.Deserialize<List<ChatMessage>>(json);
                return history ?? new List<ChatMessage>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load chat history: {ex.Message}");
                return new List<ChatMessage>();
            }
        }

        public void SaveHistory(IEnumerable<ChatMessage> messages)
        {
            try
            {
                // Keep only last N messages
                var messagesToSave = messages.TakeLast(MAX_MESSAGES).ToList();
                var json = JsonSerializer.Serialize(messagesToSave, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_historyPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save chat history: {ex.Message}");
            }
        }

        public void ClearHistory()
        {
            try
            {
                if (File.Exists(_historyPath))
                {
                    File.Delete(_historyPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to clear chat history: {ex.Message}");
            }
        }
    }
}
