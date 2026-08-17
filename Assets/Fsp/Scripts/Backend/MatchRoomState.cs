using System;
using UnityEngine;

namespace Fsp.Backend
{
    public sealed class MatchRoomState : MonoBehaviour
    {
        public static MatchRoomState Instance { get; private set; }

        public string MatchId { get; private set; } = string.Empty;
        public string Mode { get; private set; } = string.Empty;
        public string Region { get; private set; } = string.Empty;
        public int MaxPlayers { get; private set; }
        public int MemberCount { get; private set; }
        public bool HasMatch => !string.IsNullOrWhiteSpace(MatchId);

        public event Action Changed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetMatch(string matchId, string mode, string region, int maxPlayers, int memberCount)
        {
            MatchId = matchId ?? string.Empty;
            Mode = mode ?? string.Empty;
            Region = region ?? string.Empty;
            MaxPlayers = Mathf.Max(0, maxPlayers);
            MemberCount = Mathf.Max(0, memberCount);
            Changed?.Invoke();
        }

        public void Clear()
        {
            MatchId = Mode = Region = string.Empty;
            MaxPlayers = MemberCount = 0;
            Changed?.Invoke();
        }
    }
}
