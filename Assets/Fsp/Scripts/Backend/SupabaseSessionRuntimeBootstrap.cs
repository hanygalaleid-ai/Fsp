using System.Collections;
using UnityEngine;

namespace Fsp.Backend
{
    /// <summary>
    /// Restores a saved Supabase session when the Lobby is entered. Failure is deliberately non-fatal:
    /// the game remains playable offline and matchmaking can fall back without blocking START.
    /// </summary>
    public sealed class SupabaseSessionRuntimeBootstrap : MonoBehaviour
    {
        private SupabaseAuthClient authClient;
        private bool restoring;
        private bool attempted;

        public void Configure(SupabaseAuthClient client) => authClient = client;

        public void EnsureRestoreStarted()
        {
            if (attempted || restoring || SupabaseSession.IsSignedIn) return;
            attempted = true;
            StartCoroutine(RestoreRoutine());
        }

        private IEnumerator RestoreRoutine()
        {
            restoring = true;
            if (authClient == null) authClient = GetComponent<SupabaseAuthClient>();
            if (authClient == null)
            {
                restoring = false;
                yield break;
            }

            bool finished = false;
            bool ok = false;
            string message = string.Empty;
            yield return authClient.RestoreOrRefresh((success, info) =>
            {
                finished = true;
                ok = success;
                message = info ?? string.Empty;
            });

            restoring = false;
            if (finished && ok)
                Debug.Log("FSP Supabase: saved session restored for online services.");
            else if (finished && !string.IsNullOrWhiteSpace(message) && message != "No saved session.")
                Debug.LogWarning("FSP Supabase: session restore unavailable; offline play remains enabled: " + message);
        }
    }
}
