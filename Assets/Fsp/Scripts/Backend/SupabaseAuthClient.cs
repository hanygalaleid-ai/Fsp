using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Fsp.Backend
{
    public sealed class SupabaseAuthClient : MonoBehaviour
    {
        [Serializable] private sealed class AuthUser { public string id; }
        [Serializable] private sealed class AuthResponse
        {
            public string access_token;
            public string refresh_token;
            public AuthUser user;
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
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("apikey", SupabaseRuntimeConfig.PublishableKey);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                done?.Invoke(false, req.downloadHandler.text);
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
