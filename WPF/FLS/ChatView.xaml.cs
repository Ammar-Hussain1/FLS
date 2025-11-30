using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FLS.Models;
using FLS.Helpers;
using FLS.BL;
using FLS.DL;
using FLS.Services;

namespace FLS
{
    public partial class ChatView : UserControl
    {
        private ObservableCollection<ChatMessage> _messages;
        private ChatService _chatService;
        private ChatHistoryRepository _historyRepository;
        private readonly HttpClient _httpClient;
        private bool _isLoading = false;
        private string _userApiKey = string.Empty;

        public ChatView()
        {
            InitializeComponent();
            _messages = new ObservableCollection<ChatMessage>();
            _httpClient = new HttpClient();

            // Use per-user chat history file so different users don't share conversations
            string? initialUserId = null;
            try
            {
                if (SessionManager.Instance.IsLoggedIn())
                {
                    initialUserId = SessionManager.Instance.GetCurrentUserId();
                }
            }
            catch
            {
                initialUserId = null;
            }

            _historyRepository = new ChatHistoryRepository(initialUserId);
            MessagesContainer.ItemsSource = _messages;

            // Check if API key is configured
            CheckAndPromptForApiKey();

            // Initialize chat service with API key
            if (!string.IsNullOrWhiteSpace(_userApiKey))
            {
                var chatApiClient = new ChatApiClient(_httpClient);
                _chatService = new ChatService(chatApiClient, _userApiKey);
            }

            // Load chat history from local storage
            LoadChatHistory();

            // Add greeting if no history
            if (_messages.Count == 0)
            {
                AddAIMessage("Hello! I'm your AI Learning Assistant. How can I help you with your courses today?");
            }
        }

        private void LoadChatHistory()
        {
            var history = _historyRepository.LoadHistory();
            foreach (var message in history)
            {
                _messages.Add(message);
            }

            if (history.Count > 0)
            {
                ScrollToBottom();
            }
        }

        private void SaveChatHistory()
        {
            _historyRepository.SaveHistory(_messages);
        }

        private void CheckAndPromptForApiKey()
        {
            _userApiKey = AppSettings.GetApiKey();

            if (string.IsNullOrWhiteSpace(_userApiKey))
            {
                var dialog = new ApiKeyDialog();
                dialog.Owner = Window.GetWindow(this);
                var result = dialog.ShowDialog();

                if (result == true)
                {
                    _userApiKey = AppSettings.GetApiKey();
                    var chatApiClient = new ChatApiClient(_httpClient);
                    _chatService = new ChatService(chatApiClient, _userApiKey);
                }
                else
                {
                    AddAIMessage("⚠️ Please configure your Groq API key to use the chatbot. You can get one from: https://console.groq.com/keys");
                }
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                await SendMessage();
            }
        }



        private async Task SendMessage()
        {
            string message = MessageInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(message) || _isLoading)
                return;

            if (_chatService == null || string.IsNullOrWhiteSpace(_userApiKey))
            {
                AddAIMessage("⚠️ Please configure your Groq API key first.");
                CheckAndPromptForApiKey();
                return;
            }

            // Add user message to UI
            var userMessageObj = new ChatMessage(message, true);
            _messages.Add(userMessageObj);
            MessageInput.Text = string.Empty;
            ScrollToBottom();

            // Show loading indicator
            _isLoading = true;
            var loadingMessage = new ChatMessage("Thinking...", false);
            _messages.Add(loadingMessage);
            ScrollToBottom();

            try
            {
                // Check if user is logged in
                if (!SessionManager.Instance.IsLoggedIn())
                {
                    _messages.Remove(loadingMessage);
                    AddAIMessage("⚠️ Please log in to use the chatbot.");
                    return;
                }

                // Get current user ID from session
                var userId = SessionManager.Instance.GetCurrentUserId();

                // Build recent conversation history (excluding current message and loading)
                var historyTurns = new System.Collections.Generic.List<ChatTurnDTO>();
                const int maxMessagesForHistory = 16;
                foreach (var m in _messages)
                {
                    if (m == loadingMessage || m == userMessageObj)
                        continue;

                    // Take last N messages
                    // We'll collect first then trim
                    historyTurns.Add(new ChatTurnDTO
                    {
                        Role = m.IsUserMessage ? "user" : "assistant",
                        Message = m.Message
                    });
                }
                if (historyTurns.Count > maxMessagesForHistory)
                {
                    historyTurns = historyTurns.GetRange(historyTurns.Count - maxMessagesForHistory, maxMessagesForHistory);
                }

                // Call business logic layer
                var response = await _chatService.SendMessageAsync(userId, message, historyTurns);

                // Remove loading and add AI response
                _messages.Remove(loadingMessage);
                AddAIMessage(response);

                // Save chat history to local storage
                SaveChatHistory();
            }
            catch (Exception ex)
            {
                _messages.Remove(loadingMessage);
                AddAIMessage($"Sorry, I encountered an error: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void AddAIMessage(string message)
        {
            _messages.Add(new ChatMessage(message, false));
            ScrollToBottom();
        }

        private void ThresholdButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ApiKeyDialog();
            dialog.Owner = Window.GetWindow(this);
            var result = dialog.ShowDialog();

            if (result == true)
            {
                _userApiKey = AppSettings.GetApiKey();
                var chatApiClient = new ChatApiClient(_httpClient);
                _chatService = new ChatService(chatApiClient, _userApiKey);
                AddAIMessage("✅ API key updated successfully! You can now continue chatting.");
            }
        }

        private void ScrollToBottom()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ChatScrollViewer.ScrollToBottom();
            }), DispatcherPriority.ContextIdle);
        }
    }
}
