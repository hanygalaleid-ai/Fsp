using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Localization
{
    /// <summary>Keeps the runtime language selector inside the Lobby safe area and away from Profile/Squad cards.</summary>
    public sealed class FspLanguageSelectorLayoutRuntime : MonoBehaviour
    {
        private float nextTry;
        private float stopAt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<FspLanguageSelectorLayoutRuntime>() == null)
                new GameObject("FspLanguageSelectorLayoutRuntime").AddComponent<FspLanguageSelectorLayoutRuntime>();
        }

        private void Awake()
        {
            stopAt = Time.unscaledTime + 20f;
        }

        private void Update()
        {
            if (Time.unscaledTime < nextTry) return;
            nextTry = Time.unscaledTime + 0.2f;

            GameObject selector = GameObject.Find("LanguageSelector");
            GameObject safeRoot = GameObject.Find("SafeRoot");
            if (selector == null || safeRoot == null)
            {
                if (Time.unscaledTime > stopAt) Destroy(gameObject);
                return;
            }

            RectTransform rt = selector.GetComponent<RectTransform>();
            if (rt == null)
            {
                Destroy(gameObject);
                return;
            }

            rt.SetParent(safeRoot.transform, false);
            rt.anchorMin = new Vector2(0.53f, 0.91f);
            rt.anchorMax = new Vector2(0.70f, 0.975f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.SetAsLastSibling();
            Destroy(gameObject);
        }
    }
}
