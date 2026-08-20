using System;
using Fsp.Lobby;
using Fsp.UI;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    /// <summary>
    /// Presentation-only source for the local match result.
    /// Persistent profile saving is owned exclusively by MatchProgressReporter
    /// so a match can never increment XP/kills/matches twice.
    /// </summary>
    public sealed class LocalMatchResultController : MonoBehaviour
    {
        [SerializeField] private MatchManager matchManager;
        [SerializeField] private MatchParticipant localParticipant;

        public int LocalKills { get; private set; }
        public int Placement { get; private set; }
        public bool Won => Placement == 1;
        public bool Saved => false;
        public bool HasResult { get; private set; }

        public event Action ResultReady;

        private int legacyRecordedKills;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (matchManager == null) return;
            matchManager.ParticipantEliminated += OnParticipantEliminated;
            matchManager.MatchWon += OnMatchWon;
        }

        private void OnDisable()
        {
            if (matchManager == null) return;
            matchManager.ParticipantEliminated -= OnParticipantEliminated;
            matchManager.MatchWon -= OnMatchWon;
        }

        // Kept for compatibility with any older inspector/event wiring.
        public void RecordLocalKill()
        {
            legacyRecordedKills++;
        }

        private void ResolveReferences()
        {
            if (matchManager == null) matchManager = FindFirstObjectByType<MatchManager>();
            if (localParticipant != null) return;

            foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
            {
                if (participant == null || !participant.IsLocalPlayer) continue;
                localParticipant = participant;
                break;
            }
        }

        private void OnParticipantEliminated(MatchParticipant participant, int placement)
        {
            if (participant == null || !participant.IsLocalPlayer) return;
            Publish(Mathf.Max(1, placement));
        }

        private void OnMatchWon(MatchParticipant winner)
        {
            if (winner == null || !winner.IsLocalPlayer) return;
            Publish(1);
        }

        private void Publish(int placement)
        {
            if (HasResult) return;
            Placement = Mathf.Max(1, placement);
            LocalKills = Mathf.Max(KillFeedBus.LocalPlayerKills, legacyRecordedKills);
            HasResult = true;
            LobbyGameplayProgress.RecordMatch(LocalKills, Placement);
            ResultReady?.Invoke();
        }
    }
}
