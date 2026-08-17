using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class MatchManager : MonoBehaviour
    {
        private static readonly HashSet<MatchParticipant> participants = new HashSet<MatchParticipant>();
        private static MatchManager instance;

        [SerializeField] private int minimumParticipantsToStart = 2;
        [SerializeField] private bool autoStart = true;

        public bool MatchStarted { get; private set; }
        public bool MatchEnded { get; private set; }
        public int AliveCount { get; private set; }

        public event Action<int> AliveCountChanged;
        public event Action<MatchParticipant> MatchWon;

        private void Awake()
        {
            instance = this;
            RecountAlive();
        }

        private void Start()
        {
            if (autoStart && participants.Count >= minimumParticipantsToStart)
                StartMatch();
        }

        public void StartMatch()
        {
            if (MatchStarted || MatchEnded) return;
            MatchStarted = true;
            RecountAlive();
        }

        public static void Register(MatchParticipant participant)
        {
            if (participant == null) return;
            participants.Add(participant);
            instance?.RecountAlive();
        }

        public static void Unregister(MatchParticipant participant)
        {
            if (participant == null) return;
            participants.Remove(participant);
            instance?.RecountAlive();
        }

        public static void NotifyDeath(MatchParticipant participant)
        {
            instance?.HandleDeath(participant);
        }

        private void HandleDeath(MatchParticipant participant)
        {
            if (!MatchStarted || MatchEnded) return;
            RecountAlive();
            if (AliveCount > 1) return;

            MatchEnded = true;
            MatchParticipant winner = null;
            foreach (var p in participants)
            {
                if (p != null && p.IsAlive)
                {
                    winner = p;
                    break;
                }
            }
            MatchWon?.Invoke(winner);
        }

        private void RecountAlive()
        {
            int count = 0;
            foreach (var p in participants)
                if (p != null && p.IsAlive) count++;

            AliveCount = count;
            AliveCountChanged?.Invoke(AliveCount);
        }
    }
}
