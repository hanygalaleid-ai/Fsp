using Fsp.Backend;
using TMPro;
using UnityEngine;

namespace Fsp.Lobby
{
    public sealed class SquadLobbyController : MonoBehaviour
    {
        [SerializeField] private SupabaseSquadClient squadClient;
        [SerializeField] private SupabaseMatchmakingClient matchmakingClient;
        [SerializeField] private TMP_InputField inviteNameInput;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private string region = "me";

        public void CreateSquad()
        {
            if (squadClient == null) return;
            StartCoroutine(squadClient.CreateSquad(
                squadId =>
                {
                    SquadLobbyState.Instance?.SetSquad(squadId, true);
                    RefreshMembers();
                    SetStatus("تم إنشاء الفريق");
                },
                err => SetStatus(err)));
        }

        public void InviteByName()
        {
            var state = SquadLobbyState.Instance;
            if (squadClient == null || state == null || !state.HasSquad || !state.IsLeader) return;
            string name = inviteNameInput != null ? inviteNameInput.text : string.Empty;
            if (string.IsNullOrWhiteSpace(name)) { SetStatus("اكتب اسم اللاعب"); return; }

            StartCoroutine(squadClient.FindPlayerByName(name,
                player =>
                {
                    if (player == null) { SetStatus("اللاعب غير موجود"); return; }
                    StartCoroutine(squadClient.Invite(state.SquadId, player.user_id,
                        (ok, err) => SetStatus(ok ? "تم إرسال الدعوة" : err)));
                },
                err => SetStatus(err)));
        }

        public void SetReady(bool ready)
        {
            var state = SquadLobbyState.Instance;
            if (squadClient == null || state == null || !state.HasSquad) return;
            StartCoroutine(squadClient.SetReady(state.SquadId, ready,
                (ok, err) =>
                {
                    if (ok) RefreshMembers();
                    SetStatus(ok ? (ready ? "جاهز" : "غير جاهز") : err);
                }));
        }

        public void RefreshMembers()
        {
            var state = SquadLobbyState.Instance;
            if (squadClient == null || state == null || !state.HasSquad) return;
            StartCoroutine(squadClient.GetMembers(state.SquadId,
                members => state.SetMembers(members),
                err => SetStatus(err)));
        }

        public void StartSquadMatchmaking()
        {
            var state = SquadLobbyState.Instance;
            if (state == null || matchmakingClient == null) return;
            if (!state.HasSquad) { SetStatus("أنشئ فريقًا أولاً"); return; }
            if (!state.IsLeader) { SetStatus("قائد الفريق فقط يبدأ البحث"); return; }
            if (!state.AllReady) { SetStatus("كل أعضاء الفريق يجب أن يكونوا جاهزين"); return; }
            int partySize = state.Members != null ? Mathf.Clamp(state.Members.Length, 1, 4) : 1;
            StartCoroutine(matchmakingClient.JoinSquadQueue(state.SquadId, partySize, region,
                (ok, err) => SetStatus(ok ? "جاري البحث عن مباراة..." : err)));
        }

        public void LeaveSquad()
        {
            var state = SquadLobbyState.Instance;
            if (squadClient == null || state == null || !state.HasSquad) return;
            string squadId = state.SquadId;
            StartCoroutine(squadClient.LeaveSquad(squadId,
                (ok, err) =>
                {
                    if (ok) state.Clear();
                    SetStatus(ok ? "غادرت الفريق" : err);
                }));
        }

        private void SetStatus(string value)
        {
            if (statusText != null) statusText.text = value ?? string.Empty;
        }
    }
}
