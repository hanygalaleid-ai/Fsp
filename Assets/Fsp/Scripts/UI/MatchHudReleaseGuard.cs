using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.UI
{
    /// <summary>
    /// Keeps status-only fallback HUD elements behind the interactive mobile controls and prevents
    /// invisible legacy graphics from stealing touches on Android.
    /// </summary>
    public sealed class MatchHudReleaseGuard : MonoBehaviour
    {
        private float nextScan;
        private float stopAt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<MatchHudReleaseGuard>() == null)
                new GameObject("Fsp_MatchHudReleaseGuard").AddComponent<MatchHudReleaseGuard>();
        }

        private void Awake()
        {
            stopAt = Time.unscaledTime + 20f;
            Apply();
        }

        private void Update()
        {
            if (Time.unscaledTime > stopAt) { enabled = false; return; }
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 0.4f;
            Apply();
        }

        private static void Apply()
        {
            GameObject fallback = GameObject.Find("FallbackHUD");
            if (fallback != null)
            {
                Canvas canvas = fallback.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 10;
                }

                GraphicRaycaster raycaster = fallback.GetComponent<GraphicRaycaster>();
                if (raycaster != null) raycaster.enabled = false;

                foreach (Graphic graphic in fallback.GetComponentsInChildren<Graphic>(true))
                    if (graphic != null) graphic.raycastTarget = false;
            }

            GameObject mobile = GameObject.Find("MobileCombatHUD");
            if (mobile != null)
            {
                Canvas canvas = mobile.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 100;
                }
            }
        }
    }
}
