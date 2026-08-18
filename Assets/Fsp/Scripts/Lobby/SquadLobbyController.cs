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

        public bool HasService => squadClient != null;

        public void ConfigureRuntime(SupabaseSquadClient squad, SupabaseMatchmakingClient matchmaking)
        {
            squadClient = squad;
            matchmakingClient = matchmaking;
        }

        public void CreateSquad()
        {
            if (squadClient == null) { SetStatus("Squad service unavailable"); return; }
            StartCoroutine(squadClient.CreateSquad(
                squadId =>
                {
                    SquadLobbyState.Instance?.SetSquad(squadId, true);
                    RefreshMembers();
                    SetStatus("Squad created");
                },
                err => SetStatus(err)));
        }

        public void InviteByName()
        {
            string name = inviteNameInput != null ? inviteNameInput.text : string.Empty;
            InviteName(name);
        }

        public void InviteName(string name)
        {
            var state = SquadLobbyState.Instance;
            if (squadClient == null) { SetStatus("Squad service unavailable"); return; }
            if (state == null || !state.HasSquad)
            {
                CreateSquadAndInvite(name);
                return;
            }
            if (!state.IsLeader) { SetStatus("Only the squad leader can invite players"); return; }
            if (string.IsNullOrWhiteSpace(name)) { SetStatus("Enter player name"); return; }
            FindAndInvite(name);
        }

        private void CreateSquadAndInvite(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) { SetStatus("Enter player name"); return; }
            if (!SupabaseSession.IsSignedIn) { SetStatus("Sign in to invite players"); return; }
            StartCoroutine(squadClient.CreateSquad(
                squadId =>
                {
                    SquadLobbyState.Instance?.SetSquad(squadId, true);
                    RefreshMembers();
                    FindAndInvite(name);
                },
                err => SetStatus(err)));
        }

        private void FindAndInvite(string name)
        {
            var state = SquadLobbyState.Instance;
            if (state == null || !state.HasSquad) { SetStatus("Create a squad first"); return; }
            StartCoroutine(squadClient.FindPlayerByName(name,
                player =>
                {
                    if (player == null) { SetStatus("Player not found"); return; }
                    StartCoroutine(squadClient.Invite(state.SquadId, player.user_id,
                        (ok, err) => SetStatus(ok ? "Invite sent" : err)));
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
                    SetStatus(ok ? (ready ? "Ready" : "Not ready") : err);
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
            if (!state.HasSquad) { SetStatus("Create a squad first"); return; }
            if (!state.IsLeader) { SetStatus("Only the squad leader can start matchmaking"); return; }
            if (!state.AllReady) { SetStatus("All squad members must be ready"); return; }
            int partySize = state.Members != null ? Mathf.Clamp(state.Members.Length, 1, 4) : 1;
            StartCoroutine(matchmakingClient.JoinSquadQueue(state.SquadId, partySize, region,
                (ok, err) => SetStatus(ok ? "Searching for a match..." : err)));
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
                    SetStatus(ok ? "Left squad" : err);
                }));
        }

        private void SetStatus(string value)
        {
            if (statusText != null) statusText.text = value ?? string.Empty;
            Debug.Log("FSP Squad: " + (value ?? string.Empty));
        }
    }
}
