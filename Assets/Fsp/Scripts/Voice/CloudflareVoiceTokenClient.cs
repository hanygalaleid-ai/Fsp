using System;
using System.Collections;
using System.Text;
using Fsp.Backend;
using UnityEngine;
using UnityEngine.Networking;

namespace Fsp.Voice
{
    public sealed class CloudflareVoiceTokenClient : MonoBehaviour
    {
        [Serializable] public sealed class VoiceToken
        {
            public string meetingId;
            public string participantId;
            public string token;
        }

        [Serializable] private sealed class VoiceRequest
        {
            public string squadId;
            public string displayName;
        }

        [SerializeField] private string tokenEndpoint;

        public IEnumerator RequestToken(string squadId, string displayName, Action<VoiceToken> success, Action<string> failed)
        {
            if (!SupabaseSession.IsSignedIn)
            {
                failed?.Invoke("Not signed in.");
                yield break;
            }
            if (string.IsNullOrWhiteSpace(tokenEndpoint))
            {
                failed?.Invoke("Voice endpoint is not configured.");
                yield break;
            }

            var payload = new VoiceRequest { squadId = squadId, displayName = displayName };
            using var req = new UnityWebRequest(tokenEndpoint, UnityWebRequest.kHttpVerbPOST);
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload)));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", "Bearer " + SupabaseSession.AccessToken);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                failed?.Invoke(req.downloadHandler.text);
                yield break;
            }

            var token = JsonUtility.FromJson<VoiceToken>(req.downloadHandler.text);
            if (token == null || string.IsNullOrWhiteSpace(token.token))
            {
                failed?.Invoke("Voice token missing.");
                yield break;
            }
            success?.Invoke(token);
        }
    }
}
