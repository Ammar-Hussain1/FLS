using System.Configuration;
using System.IO;
using FLS.Models;

namespace FLS.Helpers
{
    public static class AppSettings
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FLS",
            "settings.txt"
        );

        public static bool HasApiKey()
        {
            return !string.IsNullOrWhiteSpace(GetApiKey());
        }

        public static string GetApiKey()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    return File.ReadAllText(SettingsFilePath).Trim();
                }
            }
            catch
            {
            }
            return string.Empty;
        }

        public static void SetApiKey(string apiKey)
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(SettingsFilePath, apiKey);
            }
            catch
            {
            }
        }

        private static string? _currentUserId;
        private static UserResponse? _currentUser;

        public static void SetCurrentUser(UserResponse user)
        {
            _currentUser = user;
            _currentUserId = user.Id;
        }

        public static string? GetCurrentUserId()
        {
            return _currentUserId;
        }

        public static UserResponse? GetCurrentUser()
        {
            return _currentUser;
        }

        public static void ClearCurrentUser()
        {
            _currentUser = null;
            _currentUserId = null;
        }
    }
}
