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
            if (matchManager == null) matchManager = FindObjectOfType<MatchManager>();
            if (profileStore == null) profileStore = FindObjectOfType<SupabaseProfileStore>();
            if (profileStore == null) profileStore = gameObject.AddComponent<SupabaseProfileStore>();

            if (localParticipant == null)
            {
                foreach (var participant in FindObjectsOfType<MatchParticipant>())
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
            if (matchManager != null)
            {
                matchManager.ParticipantEliminated += HandleParticipantEliminated;
                matchManager.MatchWon += HandleMatchWon;
            }
        }

        private void OnDisable()
        {
            if (matchManager != null)
            {
                matchManager.ParticipantEliminated -= HandleParticipantEliminated;
                matchManager.MatchWon -= HandleMatchWon;
            }
        }

        private void HandleParticipantEliminated(MatchParticipant participant, int placement)
        {
            if (participant != null && participant.IsLocalPlayer)
                SaveResult(false, Mathf.Max(1, placement));
        }

        private void HandleMatchWon(MatchParticipant winner)
        {
            if (winner != null && winner.IsLocalPlayer)
                SaveResult(true, 1);
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
