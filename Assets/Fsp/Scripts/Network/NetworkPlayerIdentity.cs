using Fsp.Backend;
using UnityEngine;

namespace Fsp.Networking
{
    public sealed class NetworkPlayerIdentity : MonoBehaviour
    {
        [SerializeField] private bool localPlayer;
        public string PlayerId { get; private set; }
        public bool IsLocalPlayer => localPlayer;

        public void Initialize(string playerId, bool isLocal)
        {
            PlayerId = playerId;
            localPlayer = isLocal;
        }

        private void Awake()
        {
            if (localPlayer && string.IsNullOrWhiteSpace(PlayerId))
                PlayerId = SupabaseSession.UserId;
        }
    }
}
