# API Key Storage - How It Works

## Current Implementation: Client-Side Storage ✅

### Where is the API key stored?
- **Location**: `%AppData%/FLS/settings.txt` (Windows)
- **Managed by**: `AppSettings.cs` helper class in WPF
- **Scope**: Per-user, per-machine

### How it works:

1. **First Time Setup**:
   - User opens chatbot
   - `ApiKeyDialog` prompts for Gemini API key
   - Key is saved to `%AppData%/FLS/settings.txt`

2. **Each Chat Message**:
   ```
   WPF Client → Reads API key from settings.txt
        ↓
   Sends to API: { userId, message, apiKey }
        ↓
   Backend creates GeminiApiClient(apiKey)
        ↓
   Calls Gemini API with user's key
   ```

3. **Security**:
   - API key stored locally on user's machine
   - Sent over HTTPS to backend
   - NOT stored in database
   - Each user uses their own key

---

## Why This Approach?

### ✅ Advantages:
1. **Cost Distribution**: Each user pays for their own API usage
2. **No Centralized Key**: No single API key to protect/manage
3. **User Control**: Users can change/revoke their key anytime
4. **Privacy**: Backend doesn't store sensitive API keys
5. **Scalability**: No rate limit concerns for the app

### ❌ Disadvantages:
1. **User Friction**: Users must get their own Gemini API key
2. **Support Burden**: Users might need help getting keys
3. **No Usage Analytics**: Can't track total API usage across users

---

## Alternative: Database Storage (Optional)

If you prefer to store API keys in the database:

### Implementation:

1. **Add to User model**:
```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string EncryptedApiKey { get; set; } // Encrypted!
    // ... other fields
}
```

2. **Encrypt before storing**:
```csharp
// Use Data Protection API or similar
var encryptedKey = EncryptionHelper.Encrypt(apiKey);
user.EncryptedApiKey = encryptedKey;
```

3. **Retrieve and decrypt**:
```csharp
var apiKey = EncryptionHelper.Decrypt(user.EncryptedApiKey);
var geminiClient = new GeminiApiClient(apiKey);
```

### When to use this:
- Users configure API key once in profile settings
- Key is encrypted in database
- Backend retrieves it automatically for each request
- Better UX (users don't need to configure per-machine)

---

## Recommendation

**Stick with current approach** (client-side storage) because:
- ✅ Simpler implementation
- ✅ Better security (no DB encryption needed)
- ✅ Each user controls their own costs
- ✅ No liability for storing API keys

**Switch to DB storage** if:
- You want to provide API keys to users (you pay)
- You need centralized usage tracking
- You want seamless multi-device experience

---

## Current Files

- **WPF**: `ChatView.xaml.cs` - Reads key from `AppSettings`
- **WPF**: `AppSettings.cs` - Manages local storage
- **WPF**: `ApiKeyDialog.xaml` - Prompts user for key
- **API**: `ChatRequest.cs` - Includes `ApiKey` field
- **API**: `ChatController.cs` - Validates and passes key
- **API**: `ChatbotService.cs` - Creates client with user's key

---

## User Experience

1. User opens chatbot for first time
2. Dialog appears: "Enter your Gemini API key"
3. User gets key from https://aistudio.google.com/app/apikey
4. User pastes key, clicks Save
5. Key is saved locally
6. Chatbot works! 🎉
7. On subsequent uses, no prompt (key already saved)

**Simple and secure!** ✅
