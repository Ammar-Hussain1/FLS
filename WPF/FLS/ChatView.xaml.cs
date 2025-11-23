using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FLS.Models;

namespace FLS
{
    public partial class ChatView : UserControl
    {
        private ObservableCollection<ChatMessage> _messages;
        private Random _random;
        private string[] _dummyResponses = new string[]
        {
            "That's a great question! Based on the course materials, I'd recommend reviewing the fundamentals first.",
            "I can help with that! Let me break it down for you step by step.",
            "Interesting topic! This relates to several concepts we've covered in your courses.",
            "I understand what you're asking. Here's what you need to know...",
            "Good thinking! That's an important aspect of the subject.",
            "Let me explain that concept in simpler terms for you.",
            "That's covered in your course materials. Would you like me to elaborate?",
            "Great question! This is a key concept that many students find challenging.",
            "I'm here to help! Let's explore this topic together.",
            "That's an excellent observation! You're on the right track."
        };

        public ChatView()
        {
            InitializeComponent();
            _messages = new ObservableCollection<ChatMessage>();
            _random = new Random();
            MessagesContainer.ItemsSource = _messages;

            AddAIMessage("Hello! I'm your AI Learning Assistant. How can I help you with your courses today?");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                SendMessage();
            }
        }

        private void SendMessage()
        {
            string message = MessageInput.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(message))
                return;

            _messages.Add(new ChatMessage(message, true));
            MessageInput.Text = string.Empty;
            
            ScrollToBottom();

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(800)
            };
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                GenerateAIResponse(message);
            };
            timer.Start();
        }

        private void GenerateAIResponse(string userMessage)
        {
            string response;

            string lowerMessage = userMessage.ToLower();
            
            if (lowerMessage.Contains("hello") || lowerMessage.Contains("hi") || lowerMessage.Contains("hey"))
            {
                response = "Hello! How can I assist you with your learning today?";
            }
            else if (lowerMessage.Contains("course") || lowerMessage.Contains("class"))
            {
                response = "I can help you with information about your courses. What would you like to know?";
            }
            else if (lowerMessage.Contains("help") || lowerMessage.Contains("assist"))
            {
                response = "I'm here to help! You can ask me about courses, assignments, study tips, or any learning-related questions.";
            }
            else if (lowerMessage.Contains("thank"))
            {
                response = "You're welcome! Feel free to ask if you have any more questions.";
            }
            else if (lowerMessage.Contains("assignment") || lowerMessage.Contains("homework"))
            {
                response = "For assignments, I recommend breaking them down into smaller tasks and tackling them one at a time. Need help with a specific assignment?";
            }
            else if (lowerMessage.Contains("study") || lowerMessage.Contains("learn"))
            {
                response = "Effective studying involves active recall, spaced repetition, and practice. What subject are you studying?";
            }
            else
            {
                response = _dummyResponses[_random.Next(_dummyResponses.Length)];
            }

            AddAIMessage(response);
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
