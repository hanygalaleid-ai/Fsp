using Fsp.BattleRoyale;
using Fsp.Lobby;
using Fsp.UI;
using UnityEngine;

namespace Fsp.Backend
{
    public sealed class MatchProgressReporter : MonoBehaviour
    {
        [SerializeField] private MatchManager matchManager;
        [SerializeField] private MatchParticipant localParticipant;
        [SerializeField] private SupabaseProfileStore profileStore;

        private bool saved;

        private void Awake()
        {
            if (matchManager == null) matchManager = FindFirstObjectByType<MatchManager>();
            if (profileStore == null) profileStore = FindFirstObjectByType<SupabaseProfileStore>();
            if (profileStore == null) profileStore = gameObject.AddComponent<SupabaseProfileStore>();

            if (localParticipant == null)
            {
                foreach (var participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
                {
                    if (participant != null && participant.IsLocalPlayer)
                    {
                        localParticipant = participant;
                        break;
                    }
                }
            }
        }

        private void OnEnable()
        {
            if (matchManager == null) return;
            matchManager.ParticipantEliminated += HandleParticipantEliminated;
            matchManager.MatchWon += HandleMatchWon;
            matchManager.NetworkWinnerDeclared += HandleNetworkWinner;
        }

        private void OnDisable()
        {
            if (matchManager == null) return;
            matchManager.ParticipantEliminated -= HandleParticipantEliminated;
            matchManager.MatchWon -= HandleMatchWon;
            matchManager.NetworkWinnerDeclared -= HandleNetworkWinner;
        }

        private void HandleParticipantEliminated(MatchParticipant participant, int placement)
        {
            if (participant != null && participant.IsLocalPlayer)
                SaveResult(false, Mathf.Max(1, placement));
        }

        private void HandleMatchWon(MatchParticipant winner)
        {
            if (matchManager != null && matchManager.NetworkAuthoritative) return;
            if (winner != null && winner.IsLocalPlayer)
                SaveResult(true, 1);
        }

        private void HandleNetworkWinner(string winnerId)
        {
            if (!SupabaseSession.IsSignedIn) return;
            bool won = !string.IsNullOrWhiteSpace(winnerId) && winnerId == SupabaseSession.UserId;
            int placement = won ? 1 : Mathf.Max(2, localParticipant != null ? localParticipant.Placement : 2);
            SaveResult(won, placement);
        }

        private async void SaveResult(bool won, int placement)
        {
            if (saved || !SupabaseSession.IsSignedIn || profileStore == null || localParticipant == null) return;
            saved = true;

            try
            {
                PlayerProfile profile = await profileStore.LoadAsync(SupabaseSession.UserId);
                if (profile == null)
                {
                    profile = new PlayerProfile(
                        SupabaseSession.UserId,
                        LobbyState.Instance != null ? LobbyState.Instance.DisplayName : "Player",
                        LobbyState.Instance != null ? LobbyState.Instance.SelectedCharacterId : "soldier_01");
                }

                profile.ApplyMatchResult(won, KillFeedBus.LocalPlayerKills, Mathf.Max(1, placement));
                await profileStore.SaveAsync(profile);
            }
            catch (System.Exception ex)
            {
                saved = false;
                Debug.LogError("Failed to save match progress: " + ex.Message);
            }
        }
    }
}
