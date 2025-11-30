# Session Management - Final Manual Updates Needed

## Files Successfully Updated
✅ `Services/SessionManager.cs` - Created
✅ `Models/AuthModels.cs` - UserResponse.Id changed to Guid
✅ `LoginPage.xaml.cs` - SessionManager integrated
✅ `ChatView.xaml.cs` - SessionManager integrated
✅ `Models/CommunityPlaylist.cs` - Id changed to Guid
✅ `Models/PlaylistRequest.cs` - Id changed to Guid, UserId added

## Files That Need Manual Updates

### 1. UserPlaylistView.xaml.cs

**Line 60** - Replace:
```csharp
var response = await _httpClient.GetAsync($"{_apiBaseUrl}/Playlist/community?userId={SessionManager.Instance.GetCurrentUserId()}");
```
With:
```csharp
var userId = SessionManager.Instance.GetCurrentUserId();
var response = await _httpClient.GetAsync($"{_apiBaseUrl}/Playlist/community?userId={userId}");
```

**Line 91** - Replace:
```csharp
allRequests.Where(r => r.UserId == _currentUserId));
```
With:
```csharp
var userId = SessionManager.Instance.GetCurrentUserId();
allRequests.Where(r => r.UserId == userId));
```

**Line 246** - Replace:
```csharp
UserId = _currentUserId
```
With:
```csharp
UserId = SessionManager.Instance.GetCurrentUserId()
```

**Top of file** - Add using statement:
```csharp
using FLS.Services;
```

**Remove field** (around line 22):
```csharp
private readonly int _currentUserId = 1; // TODO: Get from authentication
```

### 2. PlaylistRequestsAdminView.xaml.cs

**Top of file** - Add using statement:
```csharp
using FLS.Services;
```

**Remove field** (around line 19):
```csharp
private readonly int _currentAdminId = 1; // TODO: Get from authentication
```

**In ApproveRequest method** - Replace:
```csharp
var approveDto = new { AdminId = _currentAdminId };
```
With:
```csharp
var approveDto = new { AdminId = SessionManager.Instance.GetCurrentUserId() };
```

**In RejectRequest method** - Replace:
```csharp
var rejectDto = new 
{ 
    AdminId = _currentAdminId,
    Reason = "Does not meet quality standards"
};
```
With:
```csharp
var rejectDto = new 
{ 
    AdminId = SessionManager.Instance.GetCurrentUserId(),
    Reason = "Does not meet quality standards"
};
```

## Testing After Updates

1. **Login** with valid credentials
2. **Verify** SessionManager stores user info
3. **Test chatbot** - should use logged-in user ID
4. **Submit playlist request** - should use logged-in user ID
5. **Approve/reject as admin** - should use logged-in admin ID
6. **Logout** and verify session cleared

## Backend API Updates (if needed)

Ensure all API endpoints accept `Guid` for user IDs:
- `ChatbotService.SendMessageAsync(Guid userId, string message)`
- `PlaylistService` methods use `Guid` for user IDs
- Database foreign keys properly set up with UUID type

## Note
The automated file editing tools had issues with these files due to complex replacements. Manual updates are safer and more reliable for these specific changes.
