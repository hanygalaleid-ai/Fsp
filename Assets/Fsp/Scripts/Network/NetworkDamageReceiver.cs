using Fsp.Backend;
using Fsp.Player;
using UnityEngine;

namespace Fsp.Networking
{
    public sealed class NetworkDamageReceiver : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour transportBehaviour;
        [SerializeField] private PlayerVitals localVitals;

        private INetworkTransport transport;

        private void Awake()
        {
            transport = transportBehaviour as INetworkTransport;
            if (localVitals == null) localVitals = GetComponent<PlayerVitals>();
        }

        private void OnEnable()
        {
            if (transport != null) transport.DamageReceived += HandleDamage;
        }

        private void OnDisable()
        {
            if (transport != null) transport.DamageReceived -= HandleDamage;
        }

        private void HandleDamage(NetworkDamageEvent damageEvent)
        {
            if (damageEvent == null || localVitals == null || !SupabaseSession.IsSignedIn) return;
            if (damageEvent.targetId != SupabaseSession.UserId) return;
            localVitals.ApplyDamage(Mathf.Clamp(damageEvent.damage, 0f, 200f));
        }
    }
}
