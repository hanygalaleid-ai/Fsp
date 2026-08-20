using System.Collections;
using Fsp.Backend;
using UnityEngine;

namespace Fsp.Networking
{
    public sealed class MatchNetworkRuntimeConfigBootstrap : MonoBehaviour
    {
        private CloudflareWebSocketTransport transport;
        private NetworkSessionManager session;
        private SupabaseRuntimeSettingsClient settings;
        private bool loading;

        public void Configure(CloudflareWebSocketTransport targetTransport, NetworkSessionManager targetSession)
        {
            transport = targetTransport;
            session = targetSession;
            settings = GetComponent<SupabaseRuntimeSettingsClient>();
            if (settings == null) settings = gameObject.AddComponent<SupabaseRuntimeSettingsClient>();
            if (!loading) StartCoroutine(LoadRelay());
        }

        private IEnumerator LoadRelay()
        {
            loading = true;
            bool ok = false;
            string value = string.Empty;
            yield return settings.GetValue("match_relay_ws_url", (success, result) =>
            {
                ok = success;
                value = result;
            });
            loading = false;

            if (!ok)
            {
                Debug.LogWarning("FSP Network: failed to load match_relay_ws_url runtime setting: " + value);
                session?.FallbackOffline("relay runtime configuration unavailable");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                Debug.LogWarning("FSP Network: match_relay_ws_url exists but is empty.");
                session?.FallbackOffline("relay runtime configuration is empty");
                yield break;
            }

            if (transport == null || !transport.ConfigureRelayBaseUrl(value))
            {
                Debug.LogWarning("FSP Network: runtime match relay URL is invalid: " + value);
                session?.FallbackOffline("relay runtime configuration is invalid");
                yield break;
            }

            session?.RetryStartOnlineSession();
        }
    }
}
