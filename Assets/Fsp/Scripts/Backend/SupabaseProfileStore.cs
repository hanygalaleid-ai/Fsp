using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            public string character_id;
            public int xp;
            public int rank_points;
            public int matches_played;
            public int wins;
            public int kills;
        }

        [Serializable] private sealed class ProfileRows { public ProfileRow[] items; }

        public async Task<PlayerProfile> LoadAsync(string playerId, CancellationToken cancellationToken = default)
        {
            if (!SupabaseSession.IsSignedIn) return null;
            string id = string.IsNullOrWhiteSpace(playerId) ? SupabaseSession.UserId : playerId;
            string url = SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/profiles?user_id=eq." + UnityWebRequest.EscapeURL(id) + "&select=*";
            using var req = UnityWebRequest.Get(url);
            ApplyHeaders(req);
            await SendAsync(req, cancellationToken);
            if (req.result != UnityWebRequest.Result.Success) throw new Exception(req.downloadHandler.text);

            var rows = JsonUtility.FromJson<ProfileRows>("{\"items\":" + req.downloadHandler.text + "}");
            if (rows?.items == null || rows.items.Length == 0) return null;
            var row = rows.items[0];
            var profile = new PlayerProfile(row.user_id, row.display_name, row.character_id);
            profile.SetProgress(row.xp, row.rank_points, row.matches_played, row.wins, row.kills);
            return profile;
        }

        public async Task SaveAsync(PlayerProfile profile, CancellationToken cancellationToken = default)
        {
            if (!SupabaseSession.IsSignedIn || profile == null) throw new InvalidOperationException("No signed-in profile.");
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
            await SendAsync(req, cancellationToken);
            if (req.result != UnityWebRequest.Result.Success) throw new Exception(req.downloadHandler.text);
        }

        private static async Task SendAsync(UnityWebRequest req, CancellationToken token)
        {
            var op = req.SendWebRequest();
            while (!op.isDone)
            {
                if (token.IsCancellationRequested)
                {
                    req.Abort();
                    token.ThrowIfCancellationRequested();
                }
                await Task.Yield();
            }
        }

        private static void ApplyHeaders(UnityWebRequest req)
        {
            req.SetRequestHeader("apikey", SupabaseRuntimeConfig.PublishableKey);
            req.SetRequestHeader("Authorization", "Bearer " + SupabaseSession.AccessToken);
        }
    }
}
