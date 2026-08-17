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

        private bool finalizing;

        private void OnEnable()
        {
            if (matchManager == null) matchManager = FindObjectOfType<MatchManager>();
            if (profileStore == null) profileStore = FindObjectOfType<SupabaseProfileStore>();
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
            BeginFinalize();
        }

        private void OnMatchWon(MatchParticipant winner)
        {
            if (winner == null || !winner.IsLocalPlayer) return;
            Placement = 1;
            Won = true;
            BeginFinalize();
        }

        private void BeginFinalize()
        {
            if (finalizing) return;
            finalizing = true;
            _ = FinalizeAsync();
        }

        private async Task FinalizeAsync()
        {
            if (!SupabaseSession.IsSignedIn || profileStore == null)
            {
                ResultReady?.Invoke();
                finalizing = false;
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
            finally
            {
                ResultReady?.Invoke();
                finalizing = false;
            }
        }
    }
}
