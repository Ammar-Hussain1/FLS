# WPF Architecture - GUI/BL/DL Structure

## Overview

The WPF project now follows a clean **3-layer architecture**:

```
WPF/FLS/
├── GUI Layer (Views)
│   ├── ChatView.xaml.cs
│   ├── TimetableView.xaml.cs
│   ├── MainWindow.xaml.cs
│   └── ... (other XAML views)
│
├── BL/ (Business Logic)
│   └── ChatService.cs
│
├── DL/ (Data Layer)
│   ├── ApiClient.cs
│   └── ChatHistoryRepository.cs
│
├── Models/
│   └── ChatMessage.cs
│
└── Helpers/
    └── AppSettings.cs
```

---

## Layer Responsibilities

### 🎨 GUI Layer (Views/*.xaml.cs)
**What it does:**
- Handles UI events (button clicks, key presses)
- Updates UI elements (add messages, scroll, show/hide)
- Manages ObservableCollections for data binding
- Shows dialogs and user prompts

**What it DOESN'T do:**
- ❌ HTTP requests
- ❌ File I/O
- ❌ Business logic
- ❌ Data validation

**Example: ChatView.xaml.cs**
```csharp
private async void SendMessage()
{
    // UI logic only
    _messages.Add(new ChatMessage(message, true));
    ScrollToBottom();
    
    // Delegate to BL
    var response = await _chatService.SendMessageAsync(userId, message);
    
    // Update UI
    AddAIMessage(response);
}
```

---

### 💼 BL Layer (Business Logic)
**What it does:**
- Orchestrates business operations
- Validates input
- Processes data
- Coordinates between GUI and DL

**Files:**
- `ChatService.cs` - Handles chat operations

**Example: ChatService.cs**
```csharp
public async Task<string> SendMessageAsync(int userId, string message)
{
    // Validation
    if (string.IsNullOrWhiteSpace(message))
        throw new ArgumentException("Message cannot be empty");
    
    // Coordinate with DL
    var request = new ChatRequest { UserId = userId, Message = message };
    var response = await _apiClient.SendChatMessageAsync(request);
    
    return response.Response;
}
```

---

### 💾 DL Layer (Data Access)
**What it does:**
- HTTP requests to API
- Local file I/O
- Database operations (future)
- Serialization/deserialization

**Files:**
- `ApiClient.cs` - HTTP communication with backend
- `ChatHistoryRepository.cs` - Local chat history storage

**Example: ApiClient.cs**
```csharp
public async Task<ChatResponse> SendChatMessageAsync(ChatRequest request)
{
    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync($"{API_BASE_URL}/api/Chat/send", content);
    
    // Handle response
    return JsonSerializer.Deserialize<ChatResponse>(responseJson);
}
```

**Example: ChatHistoryRepository.cs**
```csharp
public void SaveHistory(IEnumerable<ChatMessage> messages)
{
    var messagesToSave = messages.TakeLast(50).ToList();
    var json = JsonSerializer.Serialize(messagesToSave);
    File.WriteAllText(_historyPath, json);
}
```

---

## Data Flow

### Sending a Chat Message

```
User types message
    ↓
[GUI] ChatView.SendMessage()
    ↓
[BL] ChatService.SendMessageAsync()
    ↓
[DL] ApiClient.SendChatMessageAsync()
    ↓
HTTP POST → Backend API
    ↓
[DL] ApiClient returns ChatResponse
    ↓
[BL] ChatService returns response string
    ↓
[GUI] ChatView displays response
    ↓
[DL] ChatHistoryRepository.SaveHistory()
```

---

## Benefits

✅ **Separation of Concerns** - Each layer has a single responsibility  
✅ **Testability** - Can mock BL/DL for unit tests  
✅ **Maintainability** - Easy to find and fix bugs  
✅ **Reusability** - BL/DL can be used by multiple views  
✅ **Scalability** - Easy to add new features  

---

## Next Steps for Timetable

When implementing timetable parsing, follow the same pattern:

```
WPF/FLS/
├── BL/
│   ├── ChatService.cs
│   └── TimetableService.cs ← NEW
│
└── DL/
    ├── ApiClient.cs
    ├── ChatHistoryRepository.cs
    └── TimetableRepository.cs ← NEW
```

**TimetableService.cs** (BL):
- Parse timetable data
- Filter by day/time
- Handle schedule conflicts

**TimetableRepository.cs** (DL):
- Fetch timetable from API
- Cache locally
- Sync with server

---

## Summary

The WPF is now properly structured with clear separation between:
- **GUI** - User interface only
- **BL** - Business logic and orchestration
- **DL** - Data access and persistence

This makes the codebase **clean, maintainable, and scalable**! 🚀
