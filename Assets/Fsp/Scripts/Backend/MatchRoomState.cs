using System;
using UnityEngine;

namespace Fsp.Backend
{
    public sealed class MatchRoomState : MonoBehaviour
    {
        public static MatchRoomState Instance { get; private set; }

        private string matchId = string.Empty;
        private string mode = string.Empty;
        private string region = string.Empty;
        private int maxPlayers;
        private int memberCount;

        public static string MatchId => Instance != null ? Instance.matchId : string.Empty;
        public static string Mode => Instance != null ? Instance.mode : string.Empty;
        public static string Region => Instance != null ? Instance.region : string.Empty;
        public static int MaxPlayers => Instance != null ? Instance.maxPlayers : 0;
        public static int MemberCount => Instance != null ? Instance.memberCount : 0;
        public static bool HasMatch => !string.IsNullOrWhiteSpace(MatchId);

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

        public void SetMatch(string newMatchId, string newMode, string newRegion, int newMaxPlayers, int newMemberCount)
        {
            matchId = newMatchId ?? string.Empty;
            mode = newMode ?? string.Empty;
            region = newRegion ?? string.Empty;
            maxPlayers = Mathf.Max(0, newMaxPlayers);
            memberCount = Mathf.Max(0, newMemberCount);
            Changed?.Invoke();
        }

        public void Clear()
        {
            matchId = mode = region = string.Empty;
            maxPlayers = memberCount = 0;
            Changed?.Invoke();
        }
    }
}
