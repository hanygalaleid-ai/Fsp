using System;
using Fsp.Lobby;
using UnityEngine;
using UnityEngine.UI;

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
            SetStatus("جاري تسجيل الدخول...");
            StartCoroutine(authClient.SignIn(emailInput.text.Trim(), passwordInput.text, async (ok, error) =>
            {
                if (!ok) { SetStatus("فشل تسجيل الدخول"); Debug.LogWarning(error); return; }
                SetSignedInState(true);
                await EnsureProfileAsync();
            }));
        }

        public void SignUp()
        {
            if (!ValidateCredentials()) return;
            SetStatus("جاري إنشاء الحساب...");
            StartCoroutine(authClient.SignUp(emailInput.text.Trim(), passwordInput.text, (ok, error) =>
            {
                if (!ok) { SetStatus("تعذر إنشاء الحساب"); Debug.LogWarning(error); return; }
                SetStatus("تم إنشاء الحساب. تحقق من بريدك ثم سجل الدخول.");
            }));
        }

        public void SignOut()
        {
            SupabaseSession.Clear();
            SetSignedInState(false);
            SetStatus(string.Empty);
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
                SetStatus("تم تسجيل الدخول");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                SetStatus("تم الدخول، لكن تعذر تحميل الملف الشخصي");
            }
        }

        private bool ValidateCredentials()
        {
            if (authClient == null || profileStore == null) return false;
            string email = emailInput != null ? emailInput.text.Trim() : string.Empty;
            string password = passwordInput != null ? passwordInput.text : string.Empty;
            if (string.IsNullOrWhiteSpace(email) || password.Length < 6)
            {
                SetStatus("اكتب بريدًا صحيحًا وكلمة مرور 6 أحرف على الأقل");
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
