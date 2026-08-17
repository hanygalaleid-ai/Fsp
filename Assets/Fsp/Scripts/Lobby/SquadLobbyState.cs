using System;
using Fsp.Backend;
using UnityEngine;

namespace Fsp.Lobby
{
    public sealed class SquadLobbyState : MonoBehaviour
    {
        public static SquadLobbyState Instance { get; private set; }

        public string SquadId { get; private set; }
        public bool IsLeader { get; private set; }
        public SupabaseSquadClient.SquadMember[] Members { get; private set; } = Array.Empty<SupabaseSquadClient.SquadMember>();
        public bool HasSquad => !string.IsNullOrWhiteSpace(SquadId);
        public bool AllReady
        {
            get
            {
                if (Members == null || Members.Length == 0) return false;
                for (int i = 0; i < Members.Length; i++)
                    if (Members[i] == null || !Members[i].is_ready) return false;
                return true;
            }
        }

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

        public void SetSquad(string squadId, bool isLeader)
        {
            SquadId = squadId ?? string.Empty;
            IsLeader = isLeader;
            Members = Array.Empty<SupabaseSquadClient.SquadMember>();
            Changed?.Invoke();
        }

        public void SetMembers(SupabaseSquadClient.SquadMember[] members)
        {
            Members = members ?? Array.Empty<SupabaseSquadClient.SquadMember>();
            Changed?.Invoke();
        }

        public void Clear()
        {
            SquadId = string.Empty;
            IsLeader = false;
            Members = Array.Empty<SupabaseSquadClient.SquadMember>();
            Changed?.Invoke();
        }
    }
}
