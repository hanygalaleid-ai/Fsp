using UnityEngine;

namespace Fsp.Backend
{
    [CreateAssetMenu(menuName = "Fsp/Backend Config", fileName = "BackendConfig")]
    public sealed class BackendConfig : ScriptableObject
    {
        [Tooltip("Supabase project URL. Do not store service_role or secret keys here.")]
        public string supabaseUrl;

        [Tooltip("Client-safe Supabase publishable key only.")]
        public string supabasePublishableKey;

        [Tooltip("Optional Cloudflare API base URL for game services.")]
        public string cloudflareApiBaseUrl;

        public bool HasSupabaseClientConfig =>
            !string.IsNullOrWhiteSpace(supabaseUrl) &&
            !string.IsNullOrWhiteSpace(supabasePublishableKey);
    }
}
