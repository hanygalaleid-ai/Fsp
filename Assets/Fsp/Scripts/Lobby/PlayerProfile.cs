using System;
using UnityEngine;

namespace Fsp.Lobby
{
    [Serializable]
    public sealed class PlayerProfile
    {
        [SerializeField] private string playerId;
        [SerializeField] private string displayName = "Player";
        [SerializeField] private string characterId = "soldier_01";
        [SerializeField] private int level = 1;
        [SerializeField] private int xp;
        [SerializeField] private int rankPoints;
        [SerializeField] private int matchesPlayed;
        [SerializeField] private int wins;
        [SerializeField] private int kills;

        public string PlayerId => playerId;
        public string DisplayName => displayName;
        public string CharacterId => characterId;
        public int Level => level;
        public int Xp => xp;
        public int RankPoints => rankPoints;
        public int MatchesPlayed => matchesPlayed;
        public int Wins => wins;
        public int Kills => kills;

        public PlayerProfile(string id, string name, string character)
        {
            playerId = id;
            SetDisplayName(name);
            characterId = string.IsNullOrWhiteSpace(character) ? "soldier_01" : character;
        }

        public void SetDisplayName(string value)
        {
            string trimmed = string.IsNullOrWhiteSpace(value) ? "Player" : value.Trim();
            displayName = trimmed.Length > 18 ? trimmed.Substring(0, 18) : trimmed;
        }

        public void SetCharacter(string id)
        {
            if (!string.IsNullOrWhiteSpace(id)) characterId = id;
        }

        public void SetProgress(int newXp, int newRankPoints, int played, int totalWins, int totalKills)
        {
            xp = Mathf.Max(0, newXp);
            rankPoints = newRankPoints;
            matchesPlayed = Mathf.Max(0, played);
            wins = Mathf.Max(0, totalWins);
            kills = Mathf.Max(0, totalKills);
            level = Mathf.Max(1, 1 + xp / 1000);
        }
    }
}
