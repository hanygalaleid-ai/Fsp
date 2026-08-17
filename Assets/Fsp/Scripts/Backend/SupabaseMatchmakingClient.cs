using System;
using System.Collections;
using System.Text;
using Fsp.Lobby;
using UnityEngine;
using UnityEngine.Networking;

namespace Fsp.Backend
{
    public sealed class SupabaseMatchmakingClient : MonoBehaviour
    {
        [Serializable] private sealed class Ticket
        {
            public string user_id;
            public string mode;
            public string region;
            public string status;
            public string squad_id;
            public int party_size;
        }

        public IEnumerator JoinQueue(MatchMode mode, string region, Action<bool, string> done)
        {
            yield return JoinQueueInternal(mode, region, null, 1, done);
        }

        public IEnumerator JoinSquadQueue(string squadId, int partySize, string region, Action<bool, string> done)
        {
            if (string.IsNullOrWhiteSpace(squadId))
            {
                done?.Invoke(false, "Squad missing.");
                yield break;
            }
            yield return JoinQueueInternal(MatchMode.Squad, region, squadId, Mathf.Clamp(partySize, 1, 4), done);
        }

        private IEnumerator JoinQueueInternal(MatchMode mode, string region, string squadId, int partySize, Action<bool, string> done)
        {
            if (!SupabaseSession.IsSignedIn)
            {
                done?.Invoke(false, "Not signed in.");
                yield break;
            }

            var ticket = new Ticket
            {
                user_id = SupabaseSession.UserId,
                mode = mode == MatchMode.Squad ? "squad" : "solo",
                region = string.IsNullOrWhiteSpace(region) ? "me" : region.Trim().ToLowerInvariant(),
                status = "searching",
                squad_id = mode == MatchMode.Squad ? squadId : null,
                party_size = mode == MatchMode.Squad ? Mathf.Clamp(partySize, 1, 4) : 1
            };

            string url = SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/matchmaking_tickets?on_conflict=user_id";
            using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(ticket)));
            req.downloadHandler = new DownloadHandlerBuffer();
            ApplyHeaders(req);
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Prefer", "resolution=merge-duplicates,return=minimal");
            yield return req.SendWebRequest();
            done?.Invoke(req.result == UnityWebRequest.Result.Success, req.result == UnityWebRequest.Result.Success ? string.Empty : req.downloadHandler.text);
        }

        public IEnumerator CancelQueue(Action<bool, string> done)
        {
            if (!SupabaseSession.IsSignedIn)
            {
                done?.Invoke(false, "Not signed in.");
                yield break;
            }

            string url = SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/matchmaking_tickets?user_id=eq." + UnityWebRequest.EscapeURL(SupabaseSession.UserId);
            using var req = UnityWebRequest.Delete(url);
            req.downloadHandler = new DownloadHandlerBuffer();
            ApplyHeaders(req);
            yield return req.SendWebRequest();
            done?.Invoke(req.result == UnityWebRequest.Result.Success, req.result == UnityWebRequest.Result.Success ? string.Empty : req.downloadHandler.text);
        }

        private static void ApplyHeaders(UnityWebRequest req)
        {
            req.SetRequestHeader("apikey", SupabaseRuntimeConfig.PublishableKey);
            req.SetRequestHeader("Authorization", "Bearer " + SupabaseSession.AccessToken);
        }
    }
}
