using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FLS.Models;
using FLS.Helpers;
using FLS.BL;
using FLS.DL;

namespace FLS
{
    /// <summary>
    /// GUI layer for chat interface
    /// Handles UI interactions and delegates business logic to BL layer
    /// </summary>
    public partial class ChatView : UserControl
    {
        private ObservableCollection<ChatMessage> _messages;
        private ChatService _chatService;
        private ChatHistoryRepository _historyRepository;
        private const int CURRENT_USER_ID = 1;
        private bool _isLoading = false;
        private string _userApiKey = string.Empty;

        public ChatView()
        {
            InitializeComponent();
            _messages = new ObservableCollection<ChatMessage>();
            _historyRepository = new ChatHistoryRepository();
            MessagesContainer.ItemsSource = _messages;

            // Check if API key is configured
            CheckAndPromptForApiKey();

            // Initialize chat service with API key
            if (!string.IsNullOrWhiteSpace(_userApiKey))
            {
                _chatService = new ChatService(_userApiKey);
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
                    _chatService = new ChatService(_userApiKey);
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
            _messages.Add(new ChatMessage(message, true));
            MessageInput.Text = string.Empty;
            ScrollToBottom();

            // Show loading indicator
            _isLoading = true;
            var loadingMessage = new ChatMessage("Thinking...", false);
            _messages.Add(loadingMessage);
            ScrollToBottom();

            try
            {
                // Call business logic layer
                var response = await _chatService.SendMessageAsync(CURRENT_USER_ID, message);

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

        private void ScrollToBottom()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ChatScrollViewer.ScrollToBottom();
            }), DispatcherPriority.ContextIdle);
        }
    }
}
