using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace UI.LoraSort.Services
{
    public static class SettingsManager
    {
        private static readonly string SettingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LoraAutoSort", "settings.json");

        public class Settings
        {
            public string EncryptedApiKey { get; set; }
        }

        public static void SaveApiKey(string apiKey)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsFilePath)!);
            var settings = new Settings
            {
                EncryptedApiKey = Encrypt(apiKey)
            };
            File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(settings));
        }

        public static string LoadApiKey()
        {
            if (!File.Exists(SettingsFilePath)) return string.Empty;
            var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsFilePath));
            return settings?.EncryptedApiKey != null ? Decrypt(settings.EncryptedApiKey) : string.Empty;
        }

        private static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;
            var data = System.Text.Encoding.UTF8.GetBytes(plainText);
            var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        private static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText)) return string.Empty;
            var data = Convert.FromBase64String(encryptedText);
            var decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            return System.Text.Encoding.UTF8.GetString(decrypted);
        }
    }
}
