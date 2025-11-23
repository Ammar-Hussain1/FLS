using System;
using System.Windows;

namespace FLS.Models
{
    public class ChatMessage
    {
        public string Message { get; set; }
        public bool IsUserMessage { get; set; }
        public DateTime Timestamp { get; set; }

        // Visibility properties for XAML binding
        public Visibility UserMessageVisibility => IsUserMessage ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AIMessageVisibility => !IsUserMessage ? Visibility.Visible : Visibility.Collapsed;

        public ChatMessage(string message, bool isUserMessage)
        {
            Message = message;
            IsUserMessage = isUserMessage;
            Timestamp = DateTime.Now;
        }
    }
}

