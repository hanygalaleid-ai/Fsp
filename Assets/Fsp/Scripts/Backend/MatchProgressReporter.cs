using Fsp.BattleRoyale;
using Fsp.Lobby;
using UnityEngine;

namespace Fsp.Backend
{
    public sealed class MatchProgressReporter : MonoBehaviour
    {
        [SerializeField] private MatchManager matchManager;
        [SerializeField] private MatchParticipant localParticipant;
        [SerializeField] private SupabaseProfileStore profileStore;

        private int matchKills;
        private bool saved;

        private void OnEnable()
        {
            if (matchManager != null) matchManager.MatchWon += HandleMatchWon;
        }

        private void OnDisable()
        {
            if (matchManager != null) matchManager.MatchWon -= HandleMatchWon;
        }

        public void RegisterLocalKill()
        {
            matchKills++;
        }

        private async void HandleMatchWon(MatchParticipant winner)
        {
            if (saved || !SupabaseSession.IsSignedIn || profileStore == null || localParticipant == null) return;
            saved = true;

            PlayerProfile profile = await profileStore.LoadAsync(SupabaseSession.UserId);
            if (profile == null)
                profile = new PlayerProfile(SupabaseSession.UserId, LobbyState.Instance != null ? LobbyState.Instance.DisplayName : "Player", LobbyState.Instance != null ? LobbyState.Instance.SelectedCharacterId : "soldier_01");

            bool won = winner == localParticipant;
            int placement = won ? 1 : Mathf.Max(2, matchManager != null ? matchManager.AliveCount + 1 : 2);
            profile.ApplyMatchResult(won, matchKills, placement);
            await profileStore.SaveAsync(profile);
        }
    }
}
