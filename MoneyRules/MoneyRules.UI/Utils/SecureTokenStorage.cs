using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Serilog;

namespace MoneyRules.UI.Utils
{

    public static class SecureTokenStorage
    {
        // Унікальний "ключ" для шифрування
        private static readonly byte[] s_entropy = { 1, 2, 3, 4, 5, 6, 7, 8 };

        private static string FilePath
        {
            get
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolderPath = Path.Combine(appDataPath, "MoneyRules");
                // Ця лінія автоматично створює папку, якщо її немає
                Directory.CreateDirectory(appFolderPath);
                return Path.Combine(appFolderPath, "user.token");
            }
        }

        public static void SaveToken(string token)
        {
            try
            {
                byte[] tokenBytes = Encoding.UTF8.GetBytes(token);

                // Шифруємо дані
                byte[] encryptedBytes = ProtectedData.Protect(tokenBytes, s_entropy, DataProtectionScope.CurrentUser);

                // FilePath у get { } вже створив папку, але цей виклик безпечний
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllBytes(FilePath, encryptedBytes);

                Log.Debug("SecureTokenStorage: Токен успішно збережено.");
            }
            catch (Exception ex)
            {
                // НЕ МОВЧІТЬ!
                Log.Error(ex, "SecureTokenStorage: Не вдалося зберегти токен.");
            }
        }

        public static string? LoadToken()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    Log.Debug("SecureTokenStorage: Файл токена не знайдено.");
                    return null;
                }

                byte[] encryptedBytes = File.ReadAllBytes(FilePath);

                // Розшифровуємо дані
                byte[] tokenBytes = ProtectedData.Unprotect(encryptedBytes, s_entropy, DataProtectionScope.CurrentUser);

                Log.Debug("SecureTokenStorage: Токен успішно завантажено.");
                return Encoding.UTF8.GetString(tokenBytes);
            }
            catch (Exception ex)
            {
                // НЕ МОВЧІТЬ!
                Log.Error(ex, "SecureTokenStorage: Не вдалося завантажити/розшифрувати токен.");
                return null;
            }
        }

        public static void ClearToken()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                    Log.Debug("SecureTokenStorage: Токен видалено.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SecureTokenStorage: Не вдалося видалити токен.");
            }
        }
    }
}

