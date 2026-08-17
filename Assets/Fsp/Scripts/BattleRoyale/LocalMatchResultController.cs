using System;
using System.Threading.Tasks;
using Fsp.Backend;
using Fsp.Lobby;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class LocalMatchResultController : MonoBehaviour
    {
        [SerializeField] private MatchManager matchManager;
        [SerializeField] private SupabaseProfileStore profileStore;

        public int LocalKills { get; private set; }
        public int Placement { get; private set; }
        public bool Won { get; private set; }
        public bool Saved { get; private set; }

        public event Action ResultReady;

        private void OnEnable()
        {
            if (matchManager == null) matchManager = FindFirstObjectByType<MatchManager>();
            if (matchManager != null)
            {
                matchManager.ParticipantEliminated += OnParticipantEliminated;
                matchManager.MatchWon += OnMatchWon;
            }
        }

        private void OnDisable()
        {
            if (matchManager != null)
            {
                matchManager.ParticipantEliminated -= OnParticipantEliminated;
                matchManager.MatchWon -= OnMatchWon;
            }
        }

        public void RecordLocalKill()
        {
            LocalKills++;
        }

        private void OnParticipantEliminated(MatchParticipant participant, int placement)
        {
            if (participant == null || !participant.IsLocalPlayer) return;
            Placement = placement;
            Won = false;
            _ = FinalizeAsync();
        }

        private void OnMatchWon(MatchParticipant winner)
        {
            if (winner == null || !winner.IsLocalPlayer) return;
            Placement = 1;
            Won = true;
            _ = FinalizeAsync();
        }

        private async Task FinalizeAsync()
        {
            if (Saved || !SupabaseSession.IsSignedIn || profileStore == null)
            {
                ResultReady?.Invoke();
                return;
            }

            try
            {
                PlayerProfile profile = await profileStore.LoadAsync(SupabaseSession.UserId);
                if (profile != null)
                {
                    profile.ApplyMatchResult(Won, LocalKills, Mathf.Max(1, Placement));
                    await profileStore.SaveAsync(profile);
                    Saved = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed to save match result: " + e.Message);
            }
            ResultReady?.Invoke();
        }
    }
}
