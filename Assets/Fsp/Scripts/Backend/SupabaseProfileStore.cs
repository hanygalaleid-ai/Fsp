using System;
using System.Collections;
using System.Text;
using Fsp.Lobby;
using UnityEngine;
using UnityEngine.Networking;

namespace Fsp.Backend
{
    public sealed class SupabaseProfileStore : MonoBehaviour, IPlayerProfileStore
    {
        [Serializable] private sealed class ProfileRow
        {
            public string user_id;
            public string display_name;
            public int character_id;
            public long xp;
            public int rank_points;
            public int matches_played;
            public int wins;
            public int kills;
        }

        public IEnumerator Load(Action<PlayerProfileData> done, Action<string> failed)
        {
            if (!SupabaseSession.IsSignedIn) { failed?.Invoke("Not signed in."); yield break; }
            string url = SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/profiles?user_id=eq." + UnityWebRequest.EscapeURL(SupabaseSession.UserId) + "&select=*";
            using var req = UnityWebRequest.Get(url);
            ApplyHeaders(req);
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) { failed?.Invoke(req.downloadHandler.text); yield break; }

            string wrapped = "{\"items\":" + req.downloadHandler.text + "}";
            var rows = JsonUtility.FromJson<ProfileRows>(wrapped);
            if (rows == null || rows.items == null || rows.items.Length == 0) { done?.Invoke(null); yield break; }
            done?.Invoke(ToProfile(rows.items[0]));
        }

        public IEnumerator Save(PlayerProfileData profile, Action<bool> done, Action<string> failed)
        {
            if (!SupabaseSession.IsSignedIn || profile == null) { failed?.Invoke("No session/profile."); yield break; }
            var row = new ProfileRow
            {
                user_id = SupabaseSession.UserId,
                display_name = profile.DisplayName,
                character_id = profile.CharacterId,
                xp = profile.Xp,
                rank_points = profile.RankPoints,
                matches_played = profile.MatchesPlayed,
                wins = profile.Wins,
                kills = profile.Kills
            };

            string url = SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/profiles?on_conflict=user_id";
            using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(row)));
            req.downloadHandler = new DownloadHandlerBuffer();
            ApplyHeaders(req);
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Prefer", "resolution=merge-duplicates,return=minimal");
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) { failed?.Invoke(req.downloadHandler.text); yield break; }
            done?.Invoke(true);
        }

        private static void ApplyHeaders(UnityWebRequest req)
        {
            req.SetRequestHeader("apikey", SupabaseRuntimeConfig.PublishableKey);
            req.SetRequestHeader("Authorization", "Bearer " + SupabaseSession.AccessToken);
        }

        private static PlayerProfileData ToProfile(ProfileRow row) => new PlayerProfileData
        {
            DisplayName = row.display_name,
            CharacterId = row.character_id,
            Xp = row.xp,
            RankPoints = row.rank_points,
            MatchesPlayed = row.matches_played,
            Wins = row.wins,
            Kills = row.kills
        };

        [Serializable] private sealed class ProfileRows { public ProfileRow[] items; }
    }
}
