using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Localization
{
    /// <summary>
    /// Chooses a dynamic OS font with glyph coverage for the active language. This keeps the test APK
    /// small while preventing Arabic/Devanagari labels from falling back to missing-glyph squares.
    /// Final release can replace these with a bundled licensed font family after device validation.
    /// </summary>
    public sealed class FspLocalizedFontRuntime : MonoBehaviour
    {
        private static FspLocalizedFontRuntime instance;
        private static readonly Dictionary<FspLanguage, Font> Cache = new();
        private float nextApply;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            GameObject go = new GameObject("FspLocalizedFontRuntime");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<FspLocalizedFontRuntime>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += HandleSceneLoaded;
            FspLocalizationRuntime.LanguageChanged += HandleLanguageChanged;
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            FspLocalizationRuntime.LanguageChanged -= HandleLanguageChanged;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => ApplyFontToAll();
        private void HandleLanguageChanged(FspLanguage language) => ApplyFontToAll();

        private void Update()
        {
            if (Time.unscaledTime < nextApply) return;
            nextApply = Time.unscaledTime + 1f;
            ApplyFontToAll();
        }

        private static void ApplyFontToAll()
        {
            Font font = ResolveFont(FspLocalizationRuntime.Current);
            if (font == null) return;

            Text[] texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Text text in texts)
            {
                if (text != null && text.font != font) text.font = font;
            }
        }

        private static Font ResolveFont(FspLanguage language)
        {
            if (Cache.TryGetValue(language, out Font cached) && cached != null) return cached;

            string[] candidates;
            switch (language)
            {
                case FspLanguage.Arabic:
                    candidates = new[] { "Noto Sans Arabic", "Noto Naskh Arabic", "NotoSansArabic", "Roboto", "Arial" };
                    break;
                case FspLanguage.Hindi:
                    candidates = new[] { "Noto Sans Devanagari", "NotoSansDevanagari", "Noto Sans", "Roboto", "Arial" };
                    break;
                default:
                    candidates = new[] { "Roboto", "Noto Sans", "Arial" };
                    break;
            }

            Font font = null;
            try { font = Font.CreateDynamicFontFromOSFont(candidates, 24); } catch { }
            if (font == null)
            {
                try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            }

            if (font != null)
            {
                font.name = "FSP_Localized_" + language;
                font.hideFlags = HideFlags.DontSave;
                Cache[language] = font;
            }
            return font;
        }
    }
}
