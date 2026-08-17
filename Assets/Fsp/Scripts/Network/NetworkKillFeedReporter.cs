using Fsp.Backend;
using Fsp.Player;
using Fsp.UI;
using UnityEngine;

namespace Fsp.Networking
{
    public sealed class NetworkKillFeedReporter : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour transportBehaviour;
        [SerializeField] private PlayerVitals localVitals;
        [SerializeField] private string localDisplayName = "Player";

        private INetworkTransport transport;
        private string lastAttackerId;

        private void Awake()
        {
            transport = transportBehaviour as INetworkTransport;
            if (localVitals == null) localVitals = GetComponent<PlayerVitals>();
        }

        private void OnEnable()
        {
            if (transport != null) transport.DamageReceived += HandleDamage;
            if (localVitals != null) localVitals.Died += HandleDeath;
        }

        private void OnDisable()
        {
            if (transport != null) transport.DamageReceived -= HandleDamage;
            if (localVitals != null) localVitals.Died -= HandleDeath;
        }

        private void HandleDamage(NetworkDamageEvent damageEvent)
        {
            if (damageEvent == null || !SupabaseSession.IsSignedIn) return;
            if (damageEvent.targetId != SupabaseSession.UserId) return;
            lastAttackerId = damageEvent.attackerId;
        }

        private void HandleDeath()
        {
            if (string.IsNullOrWhiteSpace(lastAttackerId)) return;
            KillFeedBus.ReportNetwork(ShortName(lastAttackerId), localDisplayName);
            lastAttackerId = string.Empty;
        }

        private static string ShortName(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "Player";
            return id.Length <= 8 ? id : id.Substring(0, 8);
        }
    }
}
