namespace Fsp.Backend
{
    public static class LegalRuntimeConfig
    {
        public static string PrivacyPolicyUrl => SupabaseRuntimeConfig.ProjectUrl + "/functions/v1/privacy";
    }
}
