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

        public void Configure(CloudflareWebSocketTransport targetTransport, NetworkSessionManager targetSession)
        {
            transport = targetTransport;
            session = targetSession;
            settings = GetComponent<SupabaseRuntimeSettingsClient>();
            if (settings == null) settings = gameObject.AddComponent<SupabaseRuntimeSettingsClient>();
            StartCoroutine(LoadRelay());
        }

        private IEnumerator LoadRelay()
        {
            bool ok = false;
            string value = string.Empty;
            yield return settings.GetValue("match_relay_ws_url", (success, result) =>
            {
                ok = success;
                value = result;
            });

            if (!ok)
            {
                Debug.LogError("FSP Network: failed to load match_relay_ws_url runtime setting: " + value);
                yield break;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                Debug.LogWarning("FSP Network: match_relay_ws_url exists but is empty. Deploy Cloudflare relay and update Supabase runtime config.");
                yield break;
            }

            if (!transport.ConfigureRelayBaseUrl(value))
            {
                Debug.LogError("FSP Network: runtime match relay URL is invalid: " + value);
                yield break;
            }

            session.RetryStartOnlineSession();
        }
    }
}
