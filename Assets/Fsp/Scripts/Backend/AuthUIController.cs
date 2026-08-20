using System;
using Fsp.Lobby;
using UnityEngine;
using UnityEngine.UI;
using Fsp.Localization;

namespace Fsp.Backend
{
    public sealed class AuthUIController : MonoBehaviour
    {
        [SerializeField] private SupabaseAuthClient authClient;
        [SerializeField] private SupabaseProfileStore profileStore;
        [SerializeField] private InputField emailInput;
        [SerializeField] private InputField passwordInput;
        [SerializeField] private Text statusText;
        [SerializeField] private GameObject authPanel;
        [SerializeField] private GameObject lobbyPanel;

        private void Awake()
        {
            SupabaseSession.Load();
            SetSignedInState(SupabaseSession.IsSignedIn);
        }

        public void SignIn()
        {
            if (!ValidateCredentials()) return;
            SetStatus(FspLocalizationRuntime.T("Signing in..."));
            StartCoroutine(authClient.SignIn(emailInput.text.Trim(), passwordInput.text, async (ok, error) =>
            {
                if (!ok) { SetStatus(FspLocalizationRuntime.T("Sign in failed")); Debug.LogWarning(error); return; }
                SetSignedInState(true);
                await EnsureProfileAsync();
            }));
        }

        public void SignUp()
        {
            if (!ValidateCredentials()) return;
            SetStatus(FspLocalizationRuntime.T("Creating account..."));
            StartCoroutine(authClient.SignUp(emailInput.text.Trim(), passwordInput.text, (ok, error) =>
            {
                if (!ok) { SetStatus(FspLocalizationRuntime.T("Could not create account")); Debug.LogWarning(error); return; }
                SetStatus(FspLocalizationRuntime.T("Account created. Verify your email, then sign in."));
            }));
        }

        public void SignOut()
        {
            if (authClient == null) { SupabaseSession.Clear(); SetSignedInState(false); return; }
            StartCoroutine(authClient.SignOut((ok, error) =>
            {
                SetSignedInState(false);
                SetStatus(string.Empty);
                if (!ok && !string.IsNullOrWhiteSpace(error)) Debug.LogWarning("FSP server sign out failed; local session was cleared: " + error);
            }));
        }

        private async System.Threading.Tasks.Task EnsureProfileAsync()
        {
            try
            {
                var profile = await profileStore.LoadAsync(SupabaseSession.UserId);
                if (profile == null)
                {
                    profile = new PlayerProfile(SupabaseSession.UserId, "Player", "soldier_01");
                    await profileStore.SaveAsync(profile);
                }
                SetStatus(FspLocalizationRuntime.T("Signed in"));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                SetStatus(FspLocalizationRuntime.T("Signed in, but profile could not be loaded"));
            }
        }

        private bool ValidateCredentials()
        {
            if (authClient == null || profileStore == null) return false;
            string email = emailInput != null ? emailInput.text.Trim() : string.Empty;
            string password = passwordInput != null ? passwordInput.text : string.Empty;
            if (string.IsNullOrWhiteSpace(email) || password.Length < 6)
            {
                SetStatus(FspLocalizationRuntime.T("Enter a valid email and a password of at least 6 characters"));
                return false;
            }
            return true;
        }

        private void SetSignedInState(bool signedIn)
        {
            if (authPanel != null) authPanel.SetActive(!signedIn);
            if (lobbyPanel != null) lobbyPanel.SetActive(signedIn);
        }

        private void SetStatus(string message)
        {
            if (statusText != null) statusText.text = message;
        }
    }
}
