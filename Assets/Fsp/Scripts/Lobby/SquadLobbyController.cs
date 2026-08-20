using System;
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
        public string LastStatus { get; private set; } = string.Empty;
        public event Action<string> StatusChanged;

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

        public void AcceptLatestInvite()
        {
            if (squadClient == null) { SetStatus("Squad service unavailable"); return; }
            if (!SupabaseSession.IsSignedIn) { SetStatus("Sign in to invite players"); return; }
            StartCoroutine(squadClient.GetPendingInvites(
                invites =>
                {
                    if (invites == null || invites.Length == 0) { SetStatus("No pending invites"); return; }
                    SupabaseSquadClient.SquadInvite invite = invites[0];
                    StartCoroutine(squadClient.RespondToInvite(invite, true, (ok, err) =>
                    {
                        if (!ok) { SetStatus(err); return; }
                        SquadLobbyState.Instance?.SetSquad(invite.squad_id, false);
                        RefreshMembers();
                        SetStatus("Invite accepted");
                    }));
                },
                SetStatus));
        }

        public void RefreshMembers()
        {
            var state = SquadLobbyState.Instance;
            if (squadClient == null || state == null || !state.HasSquad) return;
            StartCoroutine(squadClient.GetMembers(state.SquadId,
                members => state.SetMembers(members),
                err => SetStatus(err)));
        }

        public void StartSquadMatchmaking(Action<bool, string> completed = null)
        {
            var state = SquadLobbyState.Instance;
            if (state == null || matchmakingClient == null) { completed?.Invoke(false, "Squad service unavailable"); return; }
            if (!state.HasSquad) { SetStatus("Create a squad first"); completed?.Invoke(false, "Create a squad first"); return; }
            if (!state.IsLeader) { SetStatus("Only the squad leader can start matchmaking"); completed?.Invoke(false, "Only the squad leader can start matchmaking"); return; }
            if (!state.AllReady) { SetStatus("All squad members must be ready"); completed?.Invoke(false, "All squad members must be ready"); return; }
            int partySize = state.Members != null ? Mathf.Clamp(state.Members.Length, 1, 4) : 1;
            StartCoroutine(matchmakingClient.JoinSquadQueue(state.SquadId, partySize, region,
                (ok, err) =>
                {
                    string status = ok ? "Searching for a match..." : err;
                    SetStatus(status);
                    completed?.Invoke(ok, status);
                }));
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
            LastStatus = value ?? string.Empty;
            if (statusText != null) statusText.text = value ?? string.Empty;
            StatusChanged?.Invoke(LastStatus);
            Debug.Log("FSP Squad: " + LastStatus);
        }
    }
}
