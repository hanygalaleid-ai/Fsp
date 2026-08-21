using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Fsp.Backend
{
    public sealed class SupabaseAuthClient : MonoBehaviour
    {
        private const int RequestTimeoutSeconds = 8;
        private const string PkceVerifierKey = "bmg_google_pkce_verifier";
        public const string OAuthRedirectUri = "com.hanygalaleid.fsp://auth-callback";

        private Action<bool, string> googleSignInDone;

        [Serializable] private sealed class AuthUser { public string id; }
        [Serializable] private sealed class AuthResponse
        {
            public string access_token;
            public string refresh_token;
            public AuthUser user;
        }

        [Serializable] private sealed class PkceExchangeBody
        {
            public string auth_code;
            public string code_verifier;
        }

        private void OnEnable()
        {
            Application.deepLinkActivated += HandleDeepLink;
            if (!string.IsNullOrWhiteSpace(Application.absoluteURL)) HandleDeepLink(Application.absoluteURL);
        }

        private void OnDisable() => Application.deepLinkActivated -= HandleDeepLink;

        public void BeginGoogleSignIn(Action<bool, string> done)
        {
            googleSignInDone = done;
            string verifier = GenerateCodeVerifier();
            string challenge = GenerateCodeChallenge(verifier);
            PlayerPrefs.SetString(PkceVerifierKey, verifier);
            PlayerPrefs.Save();
            string redirect = UnityWebRequest.EscapeURL(OAuthRedirectUri);
            string url = SupabaseRuntimeConfig.ProjectUrl +
                         "/auth/v1/authorize?provider=google&redirect_to=" + redirect +
                         "&scopes=email%20profile&code_challenge=" + UnityWebRequest.EscapeURL(challenge) +
                         "&code_challenge_method=s256";
            Application.OpenURL(url);
        }

        private void HandleDeepLink(string url)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith(OAuthRedirectUri, StringComparison.OrdinalIgnoreCase)) return;
            Dictionary<string, string> values = ParseUrlValues(url);
            if (values.TryGetValue("error_description", out string oauthError) || values.TryGetValue("error", out oauthError))
            {
                CompleteGoogle(false, string.IsNullOrWhiteSpace(oauthError) ? "Google sign in failed." : oauthError);
                return;
            }
            if (values.TryGetValue("code", out string authCode) && !string.IsNullOrWhiteSpace(authCode))
            {
                string verifier = PlayerPrefs.GetString(PkceVerifierKey, string.Empty);
                if (string.IsNullOrWhiteSpace(verifier))
                {
                    CompleteGoogle(false, "Google sign in expired. Please try again.");
                    return;
                }
                StartCoroutine(ExchangePkceCode(authCode, verifier));
                return;
            }
            if (!values.TryGetValue("access_token", out string access) || string.IsNullOrWhiteSpace(access))
            {
                CompleteGoogle(false, "Google sign in did not return a session.");
                return;
            }
            ClearPkceVerifier();
            values.TryGetValue("refresh_token", out string refresh);
            StartCoroutine(CompleteOAuthSession(access, refresh));
        }

        private IEnumerator ExchangePkceCode(string authCode, string verifier)
        {
            string body = JsonUtility.ToJson(new PkceExchangeBody { auth_code = authCode, code_verifier = verifier });
            using var req = new UnityWebRequest(
                SupabaseRuntimeConfig.ProjectUrl + "/auth/v1/token?grant_type=pkce",
                UnityWebRequest.kHttpVerbPOST);
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = RequestTimeoutSeconds;
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("apikey", SupabaseRuntimeConfig.PublishableKey);
            yield return req.SendWebRequest();

            ClearPkceVerifier();
            if (req.result != UnityWebRequest.Result.Success)
            {
                string error = req.downloadHandler != null ? req.downloadHandler.text : req.error;
                CompleteGoogle(false, string.IsNullOrWhiteSpace(error) ? "Google session exchange failed." : error);
                yield break;
            }

            AuthResponse response = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text);
            if (response == null || response.user == null || string.IsNullOrWhiteSpace(response.access_token))
            {
                CompleteGoogle(false, "Google session was not returned.");
                yield break;
            }
            SupabaseSession.Save(response.access_token, response.refresh_token, response.user.id);
            CompleteGoogle(true, string.Empty);
        }

        private IEnumerator CompleteOAuthSession(string accessToken, string refreshToken)
        {
            using var req = UnityWebRequest.Get(SupabaseRuntimeConfig.ProjectUrl + "/auth/v1/user");
            req.timeout = RequestTimeoutSeconds;
            req.SetRequestHeader("apikey", SupabaseRuntimeConfig.PublishableKey);
            req.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                CompleteGoogle(false, "Google session verification failed.");
                yield break;
            }
            AuthUser user = JsonUtility.FromJson<AuthUser>(req.downloadHandler.text);
            if (user == null || string.IsNullOrWhiteSpace(user.id))
            {
                CompleteGoogle(false, "Google account identifier was not returned.");
                yield break;
            }
            SupabaseSession.Save(accessToken, refreshToken, user.id);
            CompleteGoogle(true, string.Empty);
        }

        private void CompleteGoogle(bool ok, string error)
        {
            Action<bool, string> done = googleSignInDone;
            googleSignInDone = null;
            done?.Invoke(ok, error);
        }

        private static string GenerateCodeVerifier()
        {
            byte[] bytes = new byte[32];
            using RandomNumberGenerator random = RandomNumberGenerator.Create();
            random.GetBytes(bytes);
            return Base64Url(bytes);
        }

        private static string GenerateCodeChallenge(string verifier)
        {
            using SHA256 sha = SHA256.Create();
            return Base64Url(sha.ComputeHash(Encoding.ASCII.GetBytes(verifier)));
        }

        private static string Base64Url(byte[] value)
            => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        private static void ClearPkceVerifier()
        {
            PlayerPrefs.DeleteKey(PkceVerifierKey);
            PlayerPrefs.Save();
        }

        private static Dictionary<string, string> ParseUrlValues(string url)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            int question = url.IndexOf('?');
            int hash = url.IndexOf('#');
            if (question >= 0) ParsePairs(url.Substring(question + 1, (hash > question ? hash : url.Length) - question - 1), result);
            if (hash >= 0 && hash + 1 < url.Length) ParsePairs(url.Substring(hash + 1), result);
            return result;
        }

        private static void ParsePairs(string source, Dictionary<string, string> target)
        {
            foreach (string pair in source.Split('&'))
            {
                int equals = pair.IndexOf('=');
                if (equals <= 0) continue;
                string key = Uri.UnescapeDataString(pair.Substring(0, equals).Replace("+", " "));
                string value = Uri.UnescapeDataString(pair.Substring(equals + 1).Replace("+", " "));
                target[key] = value;
            }
        }

        public IEnumerator SignUp(string email, string password, Action<bool, string> done)
        {
            string body = JsonUtility.ToJson(new Credentials { email = email, password = password });
            yield return SendAuth("/auth/v1/signup", body, false, done);
        }

        public IEnumerator SignIn(string email, string password, Action<bool, string> done)
        {
            string body = JsonUtility.ToJson(new Credentials { email = email, password = password });
            yield return SendAuth("/auth/v1/token?grant_type=password", body, true, done);
        }

        public IEnumerator RefreshSession(Action<bool, string> done)
        {
            if (string.IsNullOrWhiteSpace(SupabaseSession.RefreshToken))
            {
                done?.Invoke(false, "No refresh token available.");
                yield break;
            }

            string body = JsonUtility.ToJson(new RefreshBody { refresh_token = SupabaseSession.RefreshToken });
            yield return SendAuth("/auth/v1/token?grant_type=refresh_token", body, true, done);
        }

        public IEnumerator SignOut(Action<bool, string> done)
        {
            if (!SupabaseSession.IsSignedIn)
            {
                SupabaseSession.Clear();
                done?.Invoke(true, string.Empty);
                yield break;
            }

            using var req = new UnityWebRequest(SupabaseRuntimeConfig.ProjectUrl + "/auth/v1/logout", UnityWebRequest.kHttpVerbPOST);
            req.uploadHandler = new UploadHandlerRaw(Array.Empty<byte>());
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = RequestTimeoutSeconds;
            req.SetRequestHeader("apikey", SupabaseRuntimeConfig.PublishableKey);
            req.SetRequestHeader("Authorization", "Bearer " + SupabaseSession.AccessToken);
            yield return req.SendWebRequest();
            bool serverRevoked = req.result == UnityWebRequest.Result.Success;
            string error = serverRevoked ? string.Empty : (req.downloadHandler != null ? req.downloadHandler.text : req.error);
            SupabaseSession.Clear();
            done?.Invoke(serverRevoked, error);
        }

        public IEnumerator DeleteAccount(Action<bool, string> done)
        {
            if (!SupabaseSession.IsSignedIn)
            {
                done?.Invoke(false, "No signed-in account.");
                yield break;
            }

            string url = SupabaseRuntimeConfig.ProjectUrl + "/functions/v1/delete-account";
            using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{}"));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = RequestTimeoutSeconds;
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("apikey", SupabaseRuntimeConfig.PublishableKey);
            req.SetRequestHeader("Authorization", "Bearer " + SupabaseSession.AccessToken);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                string error = req.downloadHandler != null ? req.downloadHandler.text : req.error;
                done?.Invoke(false, string.IsNullOrWhiteSpace(error) ? "Account deletion failed." : error);
                yield break;
            }

            SupabaseSession.Clear();
            done?.Invoke(true, string.Empty);
        }

        public IEnumerator RestoreOrRefresh(Action<bool, string> done)
        {
            SupabaseSession.Load();
            if (!SupabaseSession.IsSignedIn)
            {
                done?.Invoke(false, "No saved session.");
                yield break;
            }

            yield return RefreshSession(done);
        }

        private IEnumerator SendAuth(string path, string json, bool expectSession, Action<bool, string> done)
        {
            using var req = new UnityWebRequest(SupabaseRuntimeConfig.ProjectUrl + path, UnityWebRequest.kHttpVerbPOST);
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = RequestTimeoutSeconds;
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("apikey", SupabaseRuntimeConfig.PublishableKey);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                string error = req.downloadHandler != null ? req.downloadHandler.text : string.Empty;
                if (string.IsNullOrWhiteSpace(error)) error = req.error;
                done?.Invoke(false, string.IsNullOrWhiteSpace(error) ? "Supabase authentication request failed." : error);
                yield break;
            }

            var response = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text);
            if (response != null && response.user != null && !string.IsNullOrWhiteSpace(response.access_token))
            {
                string refresh = string.IsNullOrWhiteSpace(response.refresh_token) ? SupabaseSession.RefreshToken : response.refresh_token;
                SupabaseSession.Save(response.access_token, refresh, response.user.id);
                done?.Invoke(true, string.Empty);
                yield break;
            }

            done?.Invoke(!expectSession, expectSession ? "No session returned." : string.Empty);
        }

        [Serializable] private sealed class Credentials
        {
            public string email;
            public string password;
        }

        [Serializable] private sealed class RefreshBody
        {
            public string refresh_token;
        }
    }
}
