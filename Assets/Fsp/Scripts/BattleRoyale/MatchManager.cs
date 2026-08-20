using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class MatchManager : MonoBehaviour
    {
        public enum MatchPhase { Waiting, Countdown, Active, Finished }

        private static readonly HashSet<MatchParticipant> participants = new HashSet<MatchParticipant>();
        private static MatchManager instance;

        [SerializeField] private int minimumParticipantsToStart = 2;
        [SerializeField] private bool autoStart = true;
        [SerializeField, Min(0f)] private float preMatchCountdown = 8f;

        private bool networkAuthoritative;
        private bool authoritativeClockSeen;

        public static MatchManager Instance => instance;
        public bool MatchStarted => Phase == MatchPhase.Active || Phase == MatchPhase.Finished;
        public bool MatchEnded => Phase == MatchPhase.Finished;
        public int AliveCount { get; private set; }
        public int TotalParticipants { get; private set; }
        public MatchPhase Phase { get; private set; } = MatchPhase.Waiting;
        public float CountdownRemaining { get; private set; }
        public string AuthoritativeWinnerId { get; private set; } = string.Empty;
        public bool NetworkAuthoritative => networkAuthoritative;

        public event Action<int> AliveCountChanged;
        public event Action<int> ParticipantCountChanged;
        public event Action<float> CountdownChanged;
        public event Action<MatchPhase> PhaseChanged;
        public event Action<MatchParticipant, int> ParticipantEliminated;
        public event Action<MatchParticipant> MatchWon;
        public event Action<string> NetworkWinnerDeclared;

        private void Awake()
        {
            if (instance != null && instance != this)
                Debug.LogWarning("Multiple MatchManager instances detected. The newest instance will become active.");
            instance = this;
            PruneParticipants();
            RecountAlive();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
                participants.Clear();
            }
        }

        private void Start()
        {
            if (autoStart && participants.Count >= minimumParticipantsToStart)
                BeginCountdown();
        }

        private void Update()
        {
            if (networkAuthoritative) return;
            if (Phase != MatchPhase.Countdown) return;
            CountdownRemaining = Mathf.Max(0f, CountdownRemaining - Time.deltaTime);
            CountdownChanged?.Invoke(CountdownRemaining);
            if (CountdownRemaining <= 0f) StartMatch();
        }

        public void SetNetworkAuthoritative(bool enabled)
        {
            if (networkAuthoritative == enabled) return;
            networkAuthoritative = enabled;

            if (enabled)
            {
                if (Phase == MatchPhase.Finished)
                {
                    Phase = MatchPhase.Active;
                    AuthoritativeWinnerId = string.Empty;
                    PhaseChanged?.Invoke(Phase);
                }
                return;
            }

            // An online match can deliberately fall back to local play when relay/config fails.
            // Clear server-only state and immediately rebuild local participant counts so the
            // fallback bot can start and finish the match normally.
            authoritativeClockSeen = false;
            AuthoritativeWinnerId = string.Empty;
            if (Phase == MatchPhase.Finished || Phase == MatchPhase.Active || Phase == MatchPhase.Countdown)
            {
                Phase = MatchPhase.Waiting;
                CountdownRemaining = 0f;
                PhaseChanged?.Invoke(Phase);
                CountdownChanged?.Invoke(0f);
            }

            RecountAlive();
            if (autoStart && Phase == MatchPhase.Waiting && participants.Count >= minimumParticipantsToStart)
                BeginCountdown();
        }

        public void ApplyAuthoritativeClock(float worldElapsed, float countdownSeconds)
        {
            networkAuthoritative = true;
            authoritativeClockSeen = true;
            float remaining = Mathf.Max(0f, countdownSeconds - Mathf.Max(0f, worldElapsed));

            if (remaining > 0f)
            {
                CountdownRemaining = remaining;
                if (Phase != MatchPhase.Countdown)
                {
                    Phase = MatchPhase.Countdown;
                    PhaseChanged?.Invoke(Phase);
                }
                CountdownChanged?.Invoke(CountdownRemaining);
                return;
            }

            CountdownRemaining = 0f;
            CountdownChanged?.Invoke(0f);
            if (Phase != MatchPhase.Active && Phase != MatchPhase.Finished)
            {
                Phase = MatchPhase.Active;
                PhaseChanged?.Invoke(Phase);
            }
        }

        public void BeginCountdown()
        {
            PruneParticipants();
            if (networkAuthoritative || Phase != MatchPhase.Waiting || participants.Count < minimumParticipantsToStart) return;
            Phase = MatchPhase.Countdown;
            CountdownRemaining = preMatchCountdown;
            PhaseChanged?.Invoke(Phase);
            CountdownChanged?.Invoke(CountdownRemaining);
            if (preMatchCountdown <= 0f) StartMatch();
        }

        public void StartMatch()
        {
            if (Phase == MatchPhase.Active || Phase == MatchPhase.Finished) return;
            Phase = MatchPhase.Active;
            CountdownRemaining = 0f;
            RecountAlive();
            PhaseChanged?.Invoke(Phase);
            EvaluateEndCondition();
        }

        public void ApplyAuthoritativeState(int aliveCount, int totalCount, string winnerId, bool finished)
        {
            networkAuthoritative = true;
            AliveCount = Mathf.Max(0, aliveCount);
            TotalParticipants = Mathf.Max(AliveCount, totalCount);
            AliveCountChanged?.Invoke(AliveCount);
            ParticipantCountChanged?.Invoke(TotalParticipants);

            if (!finished)
            {
                if (!authoritativeClockSeen && Phase != MatchPhase.Active)
                {
                    Phase = MatchPhase.Active;
                    CountdownRemaining = 0f;
                    AuthoritativeWinnerId = string.Empty;
                    PhaseChanged?.Invoke(Phase);
                }
                return;
            }

            if (Phase == MatchPhase.Finished && AuthoritativeWinnerId == (winnerId ?? string.Empty)) return;
            AuthoritativeWinnerId = winnerId ?? string.Empty;
            Phase = MatchPhase.Finished;
            PhaseChanged?.Invoke(Phase);
            NetworkWinnerDeclared?.Invoke(AuthoritativeWinnerId);
        }

        public static void Register(MatchParticipant participant)
        {
            if (participant == null) return;
            participants.Add(participant);
            instance?.RecountAlive();
            if (instance != null && instance.autoStart && !instance.networkAuthoritative && instance.Phase == MatchPhase.Waiting && participants.Count >= instance.minimumParticipantsToStart)
                instance.BeginCountdown();
        }

        public static void Unregister(MatchParticipant participant)
        {
            if (participant == null) return;
            participants.Remove(participant);
            if (instance == null) return;
            instance.RecountAlive();
            instance.EvaluateEndCondition();
        }

        public static void NotifyDeath(MatchParticipant participant) => instance?.HandleDeath(participant);

        private void HandleDeath(MatchParticipant participant)
        {
            if (Phase != MatchPhase.Active) return;
            RecountAlive();
            int placement = networkAuthoritative ? Mathf.Max(1, AliveCount) : Mathf.Max(1, AliveCount + 1);
            participant?.SetPlacement(placement);
            ParticipantEliminated?.Invoke(participant, placement);
            EvaluateEndCondition();
        }

        private void EvaluateEndCondition()
        {
            if (networkAuthoritative) return;
            if (Phase != MatchPhase.Active || AliveCount > 1) return;
            Phase = MatchPhase.Finished;
            MatchParticipant winner = null;
            foreach (MatchParticipant p in participants)
            {
                if (p != null && p.IsAlive)
                {
                    winner = p;
                    p.SetPlacement(1);
                    break;
                }
            }
            PhaseChanged?.Invoke(Phase);
            MatchWon?.Invoke(winner);
        }

        private void RecountAlive()
        {
            if (networkAuthoritative) return;
            PruneParticipants();
            int alive = 0;
            int total = 0;
            foreach (MatchParticipant p in participants)
            {
                if (p == null) continue;
                total++;
                if (p.IsAlive) alive++;
            }
            AliveCount = alive;
            TotalParticipants = total;
            AliveCountChanged?.Invoke(AliveCount);
            ParticipantCountChanged?.Invoke(TotalParticipants);
        }

        private static void PruneParticipants() => participants.RemoveWhere(p => p == null);
    }
}
