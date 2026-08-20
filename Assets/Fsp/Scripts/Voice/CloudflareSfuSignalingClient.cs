using System;
using System.Collections;
using System.Text;
using Fsp.Backend;
using UnityEngine;
using UnityEngine.Networking;

namespace Fsp.Voice
{
    public sealed class CloudflareSfuSignalingClient : MonoBehaviour
    {
        [Serializable] private sealed class JoinBody { public string squadId; public string sdp; }
        [Serializable] private sealed class SyncBody { public string squadId; public string sessionId; }
        [Serializable] private sealed class RenegotiateBody { public string squadId; public string sessionId; public string sdp; }
        [Serializable] private sealed class LeaveBody { public string squadId; public string sessionId; }

        [Serializable] public sealed class SignalResponse
        {
            public string sessionId;
            public string sdp;
            public string sdpType;
            public string publishedTrackName;
            public bool changed;
            public bool ok;
            public string error;
        }

        private string endpoint;

        public bool IsConfigured => !string.IsNullOrWhiteSpace(endpoint) && endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        public string Endpoint => endpoint;

        public void Configure(string value)
        {
            endpoint = (value ?? string.Empty).Trim().TrimEnd('/');
        }

        public IEnumerator Join(string squadId, string offerSdp, Action<SignalResponse> success, Action<string> failed)
        {
            yield return Post("/join", new JoinBody { squadId = squadId, sdp = offerSdp }, success, failed);
        }

        public IEnumerator Sync(string squadId, string sessionId, Action<SignalResponse> success, Action<string> failed)
        {
            yield return Post("/sync", new SyncBody { squadId = squadId, sessionId = sessionId }, success, failed);
        }

        public IEnumerator Renegotiate(string squadId, string sessionId, string answerSdp, Action<SignalResponse> success, Action<string> failed)
        {
            yield return Post("/renegotiate", new RenegotiateBody { squadId = squadId, sessionId = sessionId, sdp = answerSdp }, success, failed);
        }

        public IEnumerator Leave(string squadId, string sessionId)
        {
            if (!IsConfigured || !SupabaseSession.IsSignedIn) yield break;
            yield return Post("/leave", new LeaveBody { squadId = squadId, sessionId = sessionId }, _ => { }, _ => { });
        }

        private IEnumerator Post<T>(string path, T body, Action<SignalResponse> success, Action<string> failed)
        {
            if (!IsConfigured)
            {
                failed?.Invoke("Voice signaling endpoint is not configured.");
                yield break;
            }
            if (!SupabaseSession.IsSignedIn)
            {
                failed?.Invoke("Not signed in.");
                yield break;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(body));
            using var req = new UnityWebRequest(endpoint + path, UnityWebRequest.kHttpVerbPOST);
            req.uploadHandler = new UploadHandlerRaw(bytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", "Bearer " + SupabaseSession.AccessToken);
            req.timeout = 12;
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                string server = req.downloadHandler?.text;
                failed?.Invoke(string.IsNullOrWhiteSpace(server) ? req.error : server);
                yield break;
            }

            SignalResponse response;
            try { response = JsonUtility.FromJson<SignalResponse>(req.downloadHandler.text); }
            catch (Exception e)
            {
                failed?.Invoke("Invalid voice signaling response: " + e.Message);
                yield break;
            }

            if (response == null)
            {
                failed?.Invoke("Empty voice signaling response.");
                yield break;
            }
            if (!string.IsNullOrWhiteSpace(response.error))
            {
                failed?.Invoke(response.error);
                yield break;
            }
            success?.Invoke(response);
        }
    }
}
