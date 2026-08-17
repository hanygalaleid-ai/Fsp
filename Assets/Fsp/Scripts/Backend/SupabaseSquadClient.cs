using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Fsp.Backend
{
    public sealed class SupabaseSquadClient : MonoBehaviour
    {
        [Serializable] public sealed class DirectoryPlayer
        {
            public string user_id;
            public string display_name;
            public string character_id;
        }

        [Serializable] public sealed class SquadMember
        {
            public string squad_id;
            public string user_id;
            public bool is_ready;
        }

        [Serializable] public sealed class SquadInvite
        {
            public string id;
            public string squad_id;
            public string inviter_user_id;
            public string invitee_user_id;
            public string status;
            public string expires_at;
        }

        [Serializable] private sealed class SquadRow
        {
            public string id;
            public string leader_user_id;
        }

        [Serializable] private sealed class SquadCreate
        {
            public string leader_user_id;
        }

        [Serializable] private sealed class MemberCreate
        {
            public string squad_id;
            public string user_id;
            public bool is_ready;
        }

        [Serializable] private sealed class InviteCreate
        {
            public string squad_id;
            public string inviter_user_id;
            public string invitee_user_id;
            public string status;
        }

        [Serializable] private sealed class StatusPatch
        {
            public string status;
        }

        [Serializable] private sealed class ReadyPatch
        {
            public bool is_ready;
        }

        [Serializable] private sealed class SquadRows { public SquadRow[] items; }
        [Serializable] private sealed class PlayerRows { public DirectoryPlayer[] items; }
        [Serializable] private sealed class MemberRows { public SquadMember[] items; }
        [Serializable] private sealed class InviteRows { public SquadInvite[] items; }

        public IEnumerator UpsertDirectory(string displayName, string characterId, Action<bool, string> done)
        {
            if (!RequireSession(done)) yield break;
            string json = "{\"user_id\":\"" + EscapeJson(SupabaseSession.UserId) + "\",\"display_name\":\"" + EscapeJson(displayName) + "\",\"character_id\":\"" + EscapeJson(characterId) + "\"}";
            yield return Post("/rest/v1/player_directory?on_conflict=user_id", json, "resolution=merge-duplicates,return=minimal", done);
        }

        public IEnumerator FindPlayerByName(string displayName, Action<DirectoryPlayer> found, Action<string> failed)
        {
            if (!SupabaseSession.IsSignedIn) { failed?.Invoke("Not signed in."); yield break; }
            string url = SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/player_directory?display_name=ilike." + UnityWebRequest.EscapeURL(displayName.Trim()) + "&select=user_id,display_name,character_id&limit=1";
            using var req = UnityWebRequest.Get(url);
            ApplyHeaders(req);
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) { failed?.Invoke(req.downloadHandler.text); yield break; }
            var rows = JsonUtility.FromJson<PlayerRows>("{\"items\":" + req.downloadHandler.text + "}");
            found?.Invoke(rows?.items != null && rows.items.Length > 0 ? rows.items[0] : null);
        }

        public IEnumerator CreateSquad(Action<string> created, Action<string> failed)
        {
            if (!SupabaseSession.IsSignedIn) { failed?.Invoke("Not signed in."); yield break; }
            var row = new SquadCreate { leader_user_id = SupabaseSession.UserId };
            using var req = NewJsonRequest(SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/squads", UnityWebRequest.kHttpVerbPOST, JsonUtility.ToJson(row));
            req.SetRequestHeader("Prefer", "return=representation");
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) { failed?.Invoke(req.downloadHandler.text); yield break; }
            var rows = JsonUtility.FromJson<SquadRows>("{\"items\":" + req.downloadHandler.text + "}");
            if (rows?.items == null || rows.items.Length == 0) { failed?.Invoke("No squad returned."); yield break; }
            string squadId = rows.items[0].id;
            var member = new MemberCreate { squad_id = squadId, user_id = SupabaseSession.UserId, is_ready = false };
            yield return Post("/rest/v1/squad_members", JsonUtility.ToJson(member), "return=minimal", (ok, err) =>
            {
                if (ok) created?.Invoke(squadId); else failed?.Invoke(err);
            });
        }

        public IEnumerator Invite(string squadId, string inviteeUserId, Action<bool, string> done)
        {
            var row = new InviteCreate { squad_id = squadId, inviter_user_id = SupabaseSession.UserId, invitee_user_id = inviteeUserId, status = "pending" };
            yield return Post("/rest/v1/squad_invites", JsonUtility.ToJson(row), "return=minimal", done);
        }

        public IEnumerator GetPendingInvites(Action<SquadInvite[]> done, Action<string> failed)
        {
            if (!SupabaseSession.IsSignedIn) { failed?.Invoke("Not signed in."); yield break; }
            string url = SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/squad_invites?invitee_user_id=eq." + UnityWebRequest.EscapeURL(SupabaseSession.UserId) + "&status=eq.pending&select=*";
            using var req = UnityWebRequest.Get(url);
            ApplyHeaders(req);
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) { failed?.Invoke(req.downloadHandler.text); yield break; }
            var rows = JsonUtility.FromJson<InviteRows>("{\"items\":" + req.downloadHandler.text + "}");
            done?.Invoke(rows?.items ?? Array.Empty<SquadInvite>());
        }

        public IEnumerator RespondToInvite(SquadInvite invite, bool accept, Action<bool, string> done)
        {
            if (invite == null) { done?.Invoke(false, "Invite missing."); yield break; }
            string status = accept ? "accepted" : "declined";
            using (var req = NewJsonRequest(SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/squad_invites?id=eq." + UnityWebRequest.EscapeURL(invite.id), "PATCH", JsonUtility.ToJson(new StatusPatch { status = status })))
            {
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success) { done?.Invoke(false, req.downloadHandler.text); yield break; }
            }
            if (!accept) { done?.Invoke(true, string.Empty); yield break; }
            var member = new MemberCreate { squad_id = invite.squad_id, user_id = SupabaseSession.UserId, is_ready = false };
            yield return Post("/rest/v1/squad_members", JsonUtility.ToJson(member), "return=minimal", done);
        }

        public IEnumerator SetReady(string squadId, bool ready, Action<bool, string> done)
        {
            using var req = NewJsonRequest(SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/squad_members?squad_id=eq." + UnityWebRequest.EscapeURL(squadId) + "&user_id=eq." + UnityWebRequest.EscapeURL(SupabaseSession.UserId), "PATCH", JsonUtility.ToJson(new ReadyPatch { is_ready = ready }));
            yield return req.SendWebRequest();
            done?.Invoke(req.result == UnityWebRequest.Result.Success, req.result == UnityWebRequest.Result.Success ? string.Empty : req.downloadHandler.text);
        }

        public IEnumerator GetMembers(string squadId, Action<SquadMember[]> done, Action<string> failed)
        {
            string url = SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/squad_members?squad_id=eq." + UnityWebRequest.EscapeURL(squadId) + "&select=squad_id,user_id,is_ready";
            using var req = UnityWebRequest.Get(url);
            ApplyHeaders(req);
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) { failed?.Invoke(req.downloadHandler.text); yield break; }
            var rows = JsonUtility.FromJson<MemberRows>("{\"items\":" + req.downloadHandler.text + "}");
            done?.Invoke(rows?.items ?? Array.Empty<SquadMember>());
        }

        public IEnumerator LeaveSquad(string squadId, Action<bool, string> done)
        {
            string url = SupabaseRuntimeConfig.ProjectUrl + "/rest/v1/squad_members?squad_id=eq." + UnityWebRequest.EscapeURL(squadId) + "&user_id=eq." + UnityWebRequest.EscapeURL(SupabaseSession.UserId);
            using var req = UnityWebRequest.Delete(url);
            req.downloadHandler = new DownloadHandlerBuffer();
            ApplyHeaders(req);
            yield return req.SendWebRequest();
            done?.Invoke(req.result == UnityWebRequest.Result.Success, req.result == UnityWebRequest.Result.Success ? string.Empty : req.downloadHandler.text);
        }

        private IEnumerator Post(string path, string json, string prefer, Action<bool, string> done)
        {
            using var req = NewJsonRequest(SupabaseRuntimeConfig.ProjectUrl + path, UnityWebRequest.kHttpVerbPOST, json);
            if (!string.IsNullOrWhiteSpace(prefer)) req.SetRequestHeader("Prefer", prefer);
            yield return req.SendWebRequest();
            done?.Invoke(req.result == UnityWebRequest.Result.Success, req.result == UnityWebRequest.Result.Success ? string.Empty : req.downloadHandler.text);
        }

        private static UnityWebRequest NewJsonRequest(string url, string method, string json)
        {
            var req = new UnityWebRequest(url, method);
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            ApplyHeaders(req);
            return req;
        }

        private bool RequireSession(Action<bool, string> done)
        {
            if (SupabaseSession.IsSignedIn) return true;
            done?.Invoke(false, "Not signed in.");
            return false;
        }

        private static void ApplyHeaders(UnityWebRequest req)
        {
            req.SetRequestHeader("apikey", SupabaseRuntimeConfig.PublishableKey);
            req.SetRequestHeader("Authorization", "Bearer " + SupabaseSession.AccessToken);
        }

        private static string EscapeJson(string value) => (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
