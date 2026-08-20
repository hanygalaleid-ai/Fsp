using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fsp.Presentation;
using UnityEngine;
using UnityEngine.Networking;

namespace Fsp.Backend
{
    public sealed class SupabaseCosmeticsClient : MonoBehaviour
    {
        [Serializable] private sealed class OwnedRow { public string item_id; }
        [Serializable] private sealed class OwnedRows { public OwnedRow[] items; }
        [Serializable] private sealed class ProfileCosmeticsRow
        {
            public string head_item_id;
            public string face_item_id;
            public string torso_item_id;
            public string legs_item_id;
            public string backpack_item_id;
            public string parachute_item_id;
        }
        [Serializable] private sealed class ProfileRows { public ProfileCosmeticsRow[] items; }

        public async Task<HashSet<string>> LoadOwnedAsync()
        {
            var owned = new HashSet<string>
            {
                "head_default", "face_none", "torso_default", "legs_default", "backpack_default", "parachute_default"
            };
            foreach (string starterId in StarterCosmeticCatalog.AllItemIds) owned.Add(starterId);
            if (!SupabaseSession.IsSignedIn) return owned;

            string url = SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/player_cosmetics?user_id=eq." + UnityWebRequest.EscapeURL(SupabaseSession.UserId) + "&select=item_id";
            using var req = UnityWebRequest.Get(url);
            ApplyHeaders(req);
            await SendAsync(req);
            if (req.result != UnityWebRequest.Result.Success) throw new Exception(req.downloadHandler.text);
            var rows = JsonUtility.FromJson<OwnedRows>("{\"items\":" + req.downloadHandler.text + "}");
            if (rows?.items != null)
                foreach (var row in rows.items)
                    if (row != null && !string.IsNullOrWhiteSpace(row.item_id)) owned.Add(row.item_id);
            return owned;
        }

        public async Task<CosmeticLoadout> LoadEquippedAsync()
        {
            if (!SupabaseSession.IsSignedIn) return StarterWardrobeRuntime.LoadLocal();
            string select = "head_item_id,face_item_id,torso_item_id,legs_item_id,backpack_item_id,parachute_item_id";
            string url = SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/profiles?user_id=eq." + UnityWebRequest.EscapeURL(SupabaseSession.UserId) + "&select=" + select;
            using var req = UnityWebRequest.Get(url);
            ApplyHeaders(req);
            await SendAsync(req);
            if (req.result != UnityWebRequest.Result.Success) throw new Exception(req.downloadHandler.text);
            var rows = JsonUtility.FromJson<ProfileRows>("{\"items\":" + req.downloadHandler.text + "}");
            if (rows?.items == null || rows.items.Length == 0) return StarterWardrobeRuntime.LoadLocal();
            var r = rows.items[0];
            var result = new CosmeticLoadout
            {
                headItemId = r.head_item_id,
                faceItemId = r.face_item_id,
                torsoItemId = r.torso_item_id,
                legsItemId = r.legs_item_id,
                backpackItemId = r.backpack_item_id,
                parachuteItemId = r.parachute_item_id
            };
            StarterWardrobeRuntime.SaveLocal(result);
            return result;
        }

        public async Task SaveEquippedAsync(CosmeticLoadout loadout)
        {
            if (loadout == null) return;
            StarterWardrobeRuntime.SaveLocal(loadout);
            if (!SupabaseSession.IsSignedIn) return;
            var row = new ProfileCosmeticsRow
            {
                head_item_id = loadout.headItemId,
                face_item_id = loadout.faceItemId,
                torso_item_id = loadout.torsoItemId,
                legs_item_id = loadout.legsItemId,
                backpack_item_id = loadout.backpackItemId,
                parachute_item_id = loadout.parachuteItemId
            };
            string url = SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/profiles?user_id=eq." + UnityWebRequest.EscapeURL(SupabaseSession.UserId);
            using var req = new UnityWebRequest(url, "PATCH");
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(row)));
            req.downloadHandler = new DownloadHandlerBuffer();
            ApplyHeaders(req);
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Prefer", "return=minimal");
            await SendAsync(req);
            if (req.result != UnityWebRequest.Result.Success) throw new Exception(req.downloadHandler.text);
        }

        private static void ApplyHeaders(UnityWebRequest req)
        {
            req.SetRequestHeader("apikey", SupabaseRuntimeConfig.PublishableKey);
            req.SetRequestHeader("Authorization", "Bearer " + SupabaseSession.AccessToken);
        }

        private static async Task SendAsync(UnityWebRequest req)
        {
            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();
        }
    }
}
