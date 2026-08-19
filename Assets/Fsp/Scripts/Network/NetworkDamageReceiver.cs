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
        private bool subscribed;

        private void Awake()
        {
            if (localVitals == null) localVitals = GetComponent<PlayerVitals>();
            TryResolveTransport();
        }

        private void OnEnable()
        {
            TryResolveTransport();
            TrySubscribe();
        }

        private void Update()
        {
            if (subscribed) return;
            TryResolveTransport();
            TrySubscribe();
        }

        private void TryResolveTransport()
        {
            transport = transportBehaviour as INetworkTransport;
            if (transport != null) return;

            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (behaviour is not INetworkTransport candidate) continue;
                transportBehaviour = behaviour;
                transport = candidate;
                return;
            }
        }

        private void TrySubscribe()
        {
            if (subscribed || transport == null) return;
            transport.DamageReceived += HandleDamage;
            subscribed = true;
        }

        private void OnDisable()
        {
            if (subscribed && transport != null) transport.DamageReceived -= HandleDamage;
            subscribed = false;
        }

        private void HandleDamage(NetworkDamageEvent damageEvent)
        {
            if (damageEvent == null || localVitals == null || !SupabaseSession.IsSignedIn) return;
            if (damageEvent.targetId != SupabaseSession.UserId) return;
            localVitals.ApplyDamage(Mathf.Clamp(damageEvent.damage, 0f, 200f));
        }
    }
}
