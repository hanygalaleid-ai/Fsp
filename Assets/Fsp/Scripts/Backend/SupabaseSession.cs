using UnityEngine;

namespace Fsp.Backend
{
    public static class SupabaseSession
    {
        private const string AccessKey = "fsp.supabase.access";
        private const string RefreshKey = "fsp.supabase.refresh";
        private const string UserKey = "fsp.supabase.user";

        public static string AccessToken { get; private set; }
        public static string RefreshToken { get; private set; }
        public static string UserId { get; private set; }
        public static bool IsSignedIn => !string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(UserId);

        public static void Load()
        {
            AccessToken = PlayerPrefs.GetString(AccessKey, string.Empty);
            RefreshToken = PlayerPrefs.GetString(RefreshKey, string.Empty);
            UserId = PlayerPrefs.GetString(UserKey, string.Empty);
        }

        public static void Save(string accessToken, string refreshToken, string userId)
        {
            AccessToken = accessToken ?? string.Empty;
            RefreshToken = refreshToken ?? string.Empty;
            UserId = userId ?? string.Empty;
            PlayerPrefs.SetString(AccessKey, AccessToken);
            PlayerPrefs.SetString(RefreshKey, RefreshToken);
            PlayerPrefs.SetString(UserKey, UserId);
            PlayerPrefs.Save();
        }

        public static void Clear()
        {
            AccessToken = RefreshToken = UserId = string.Empty;
            PlayerPrefs.DeleteKey(AccessKey);
            PlayerPrefs.DeleteKey(RefreshKey);
            PlayerPrefs.DeleteKey(UserKey);
            PlayerPrefs.Save();
        }
    }
}
