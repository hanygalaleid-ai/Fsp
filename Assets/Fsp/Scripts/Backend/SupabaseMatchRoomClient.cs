using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Fsp.Backend
{
    public sealed class SupabaseMatchRoomClient : MonoBehaviour
    {
        [Serializable] private sealed class MatchInfo
        {
            public string id;
            public string mode;
            public string region;
            public string status;
            public int max_players;
        }

        [Serializable] private sealed class MatchmakeResponse
        {
            public string status;
            public int players;
            public int member_count;
            public MatchInfo match;
            public string error;
        }

        public IEnumerator TryMatchmake(Action<bool, string> done)
        {
            if (!SupabaseSession.IsSignedIn)
            {
                done?.Invoke(false, "Not signed in.");
                yield break;
            }

            string url = SupabaseRuntimeConfig.ProjectUrl + "/functions/v1/matchmake";
            using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{}"));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("apikey", SupabaseRuntimeConfig.PublishableKey);
            req.SetRequestHeader("Authorization", "Bearer " + SupabaseSession.AccessToken);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                done?.Invoke(false, req.downloadHandler.text);
                yield break;
            }

            var response = JsonUtility.FromJson<MatchmakeResponse>(req.downloadHandler.text);
            if (response == null)
            {
                done?.Invoke(false, "Invalid matchmaker response.");
                yield break;
            }

            if (response.status == "waiting")
            {
                done?.Invoke(true, "waiting:" + response.players);
                yield break;
            }

            if (response.status == "matched" && response.match != null)
            {
                if (MatchRoomState.Instance != null)
                {
                    MatchRoomState.Instance.SetMatch(
                        response.match.id,
                        response.match.mode,
                        response.match.region,
                        response.match.max_players,
                        response.member_count);
                }

                done?.Invoke(true, "matched");
                yield break;
            }

            done?.Invoke(false, string.IsNullOrWhiteSpace(response.error) ? "Matchmaking failed." : response.error);
        }

        public IEnumerator LoadMyMatch(Action<bool, string> done)
        {
            if (!SupabaseSession.IsSignedIn)
            {
                done?.Invoke(false, "Not signed in.");
                yield break;
            }

            string memberUrl = SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/match_room_members?user_id=eq." + UnityWebRequest.EscapeURL(SupabaseSession.UserId) + "&select=match_id&order=joined_at.desc&limit=1";
            using var memberReq = UnityWebRequest.Get(memberUrl);
            ApplyHeaders(memberReq);
            yield return memberReq.SendWebRequest();
            if (memberReq.result != UnityWebRequest.Result.Success)
            {
                done?.Invoke(false, memberReq.downloadHandler.text);
                yield break;
            }

            string wrapped = "{\"items\":" + memberReq.downloadHandler.text + "}";
            var members = JsonUtility.FromJson<MemberRows>(wrapped);
            if (members?.items == null || members.items.Length == 0)
            {
                done?.Invoke(true, "none");
                yield break;
            }

            string matchId = members.items[0].match_id;
            string roomUrl = SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/match_rooms?id=eq." + UnityWebRequest.EscapeURL(matchId) + "&select=id,mode,region,status,max_players&limit=1";
            using var roomReq = UnityWebRequest.Get(roomUrl);
            ApplyHeaders(roomReq);
            yield return roomReq.SendWebRequest();
            if (roomReq.result != UnityWebRequest.Result.Success)
            {
                done?.Invoke(false, roomReq.downloadHandler.text);
                yield break;
            }

            var rooms = JsonUtility.FromJson<RoomRows>("{\"items\":" + roomReq.downloadHandler.text + "}");
            if (rooms?.items == null || rooms.items.Length == 0)
            {
                done?.Invoke(true, "none");
                yield break;
            }

            var room = rooms.items[0];
            MatchRoomState.Instance?.SetMatch(room.id, room.mode, room.region, room.max_players, 0);
            done?.Invoke(true, "matched");
        }

        [Serializable] private sealed class MemberRow { public string match_id; }
        [Serializable] private sealed class MemberRows { public MemberRow[] items; }
        [Serializable] private sealed class RoomRows { public MatchInfo[] items; }

        private static void ApplyHeaders(UnityWebRequest req)
        {
            req.SetRequestHeader("apikey", SupabaseRuntimeConfig.PublishableKey);
            req.SetRequestHeader("Authorization", "Bearer " + SupabaseSession.AccessToken);
        }
    }
}
