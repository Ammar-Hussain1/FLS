using System.Configuration;
using System.IO;

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
                // Ignore errors
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
                // Ignore errors
            }
        }
    }
}
