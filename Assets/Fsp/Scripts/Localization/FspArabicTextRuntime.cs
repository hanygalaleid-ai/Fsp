using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Localization
{
    /// <summary>
    /// Presentation-only Arabic shaper for legacy UnityEngine.UI.Text.
    /// The localization tables keep normal Unicode Arabic. This runtime stores that logical text,
    /// converts connected letters to Arabic Presentation Forms, and reorders Arabic runs for the
    /// legacy left-to-right text renderer. When the player leaves Arabic, logical text is restored
    /// before the localization pass so language switching remains reversible.
    /// </summary>
    public sealed class FspArabicTextRuntime : MonoBehaviour
    {
        private sealed class LogicalArabicText : MonoBehaviour
        {
            [NonSerialized] public string Logical;
        }

        private readonly struct Forms
        {
            public readonly char Isolated;
            public readonly char Final;
            public readonly char Initial;
            public readonly char Medial;
            public readonly bool JoinPrevious;
            public readonly bool JoinNext;

            public Forms(int isolated, int final, int initial, int medial, bool joinPrevious, bool joinNext)
            {
                Isolated = (char)isolated;
                Final = (char)final;
                Initial = initial == 0 ? (char)isolated : (char)initial;
                Medial = medial == 0 ? Final : (char)medial;
                JoinPrevious = joinPrevious;
                JoinNext = joinNext;
            }
        }

        private static FspArabicTextRuntime instance;
        private static readonly Dictionary<char, Forms> Map = BuildMap();
        private float nextScan;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            GameObject go = new GameObject("FspArabicTextRuntime");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<FspArabicTextRuntime>();
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
            SceneManager.sceneLoaded += OnSceneLoaded;
            FspLocalizationRuntime.LanguageChanged += OnLanguageChanged;
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            FspLocalizationRuntime.LanguageChanged -= OnLanguageChanged;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Apply();

        private void OnLanguageChanged(FspLanguage language)
        {
            if (language != FspLanguage.Arabic) RestoreLogicalText();
            else Apply();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 0.25f;
            if (FspLocalizationRuntime.Current != FspLanguage.Arabic) RestoreLogicalText();
        }

        // Match HUD values are rewritten during Update (health, ammo, alive and phase). Shape
        // them in LateUpdate so the rendered frame never exposes unshaped/reversed Arabic.
        private void LateUpdate()
        {
            if (FspLocalizationRuntime.Current == FspLanguage.Arabic) Apply();
        }

        private static void Apply()
        {
            if (FspLocalizationRuntime.Current != FspLanguage.Arabic) return;
            Text[] texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Text text in texts)
            {
                if (text == null || string.IsNullOrEmpty(text.text)) continue;

                LogicalArabicText marker = text.GetComponent<LogicalArabicText>();
                string logical;
                if (ContainsBaseArabic(text.text))
                {
                    logical = text.text;
                    if (marker == null) marker = text.gameObject.AddComponent<LogicalArabicText>();
                    marker.Logical = logical;
                }
                else if (marker != null && !string.IsNullOrEmpty(marker.Logical))
                {
                    logical = marker.Logical;
                }
                else
                {
                    continue;
                }

                text.text = ShapeMultiline(logical);
                text.alignment = MirrorLeftToRight(text.alignment);
            }
        }

        private static void RestoreLogicalText()
        {
            LogicalArabicText[] markers = FindObjectsByType<LogicalArabicText>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (LogicalArabicText marker in markers)
            {
                if (marker == null || string.IsNullOrEmpty(marker.Logical)) continue;
                Text text = marker.GetComponent<Text>();
                if (text != null) text.text = marker.Logical;
                Destroy(marker);
            }
        }

        private static string ShapeMultiline(string value)
        {
            string normalized = value.Replace("\r\n", "\n");
            string[] lines = normalized.Split('\n');
            for (int i = 0; i < lines.Length; i++) lines[i] = ShapeLine(lines[i]);
            return string.Join("\n", lines);
        }

        private static string ShapeLine(string line)
        {
            if (string.IsNullOrEmpty(line) || !ContainsBaseArabic(line)) return line;

            var logicalRuns = new List<string>();
            var run = new StringBuilder();
            bool? arabicRun = null;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                bool isArabic = IsArabicOrMark(c);
                if (arabicRun.HasValue && arabicRun.Value != isArabic)
                {
                    logicalRuns.Add(arabicRun.Value ? ShapeArabicRun(run.ToString()) : run.ToString());
                    run.Clear();
                }
                arabicRun = isArabic;
                run.Append(c);
            }
            if (run.Length > 0)
                logicalRuns.Add(arabicRun == true ? ShapeArabicRun(run.ToString()) : run.ToString());

            // Legacy Text does not apply bidi ordering. Reverse run order, while each Arabic run is
            // already reversed internally and numeric/Latin runs preserve their own character order.
            logicalRuns.Reverse();
            return string.Concat(logicalRuns);
        }

        private static string ShapeArabicRun(string run)
        {
            char[] source = run.ToCharArray();
            char[] shaped = new char[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                if (!Map.TryGetValue(c, out Forms forms))
                {
                    shaped[i] = c;
                    continue;
                }

                int prevIndex = FindPreviousLetter(source, i - 1);
                int nextIndex = FindNextLetter(source, i + 1);
                bool joinsPrev = prevIndex >= 0 && CanJoin(source[prevIndex], c);
                bool joinsNext = nextIndex >= 0 && CanJoin(c, source[nextIndex]);

                if (joinsPrev && joinsNext) shaped[i] = forms.Medial;
                else if (joinsPrev) shaped[i] = forms.Final;
                else if (joinsNext) shaped[i] = forms.Initial;
                else shaped[i] = forms.Isolated;
            }

            Array.Reverse(shaped);
            return new string(shaped);
        }

        private static bool CanJoin(char left, char right)
        {
            return Map.TryGetValue(left, out Forms l) && Map.TryGetValue(right, out Forms r) && l.JoinNext && r.JoinPrevious;
        }

        private static int FindPreviousLetter(char[] text, int index)
        {
            for (int i = index; i >= 0; i--)
            {
                if (IsArabicMark(text[i])) continue;
                return Map.ContainsKey(text[i]) ? i : -1;
            }
            return -1;
        }

        private static int FindNextLetter(char[] text, int index)
        {
            for (int i = index; i < text.Length; i++)
            {
                if (IsArabicMark(text[i])) continue;
                return Map.ContainsKey(text[i]) ? i : -1;
            }
            return -1;
        }

        private static bool ContainsBaseArabic(string value)
        {
            foreach (char c in value)
                if (Map.ContainsKey(c)) return true;
            return false;
        }

        private static bool IsArabicOrMark(char c) => Map.ContainsKey(c) || IsArabicMark(c);
        private static bool IsArabicMark(char c) => c >= '\u064B' && c <= '\u065F';

        private static TextAnchor MirrorLeftToRight(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft: return TextAnchor.UpperRight;
                case TextAnchor.MiddleLeft: return TextAnchor.MiddleRight;
                case TextAnchor.LowerLeft: return TextAnchor.LowerRight;
                default: return anchor;
            }
        }

        private static Dictionary<char, Forms> BuildMap()
        {
            // Arabic Presentation Forms-B. Right-joining letters have no initial/medial form.
            return new Dictionary<char, Forms>
            {
                ['\u0621'] = new Forms(0xFE80, 0xFE80, 0, 0, false, false), // hamza
                ['\u0622'] = new Forms(0xFE81, 0xFE82, 0, 0, true, false),
                ['\u0623'] = new Forms(0xFE83, 0xFE84, 0, 0, true, false),
                ['\u0624'] = new Forms(0xFE85, 0xFE86, 0, 0, true, false),
                ['\u0625'] = new Forms(0xFE87, 0xFE88, 0, 0, true, false),
                ['\u0626'] = new Forms(0xFE89, 0xFE8A, 0xFE8B, 0xFE8C, true, true),
                ['\u0627'] = new Forms(0xFE8D, 0xFE8E, 0, 0, true, false),
                ['\u0628'] = new Forms(0xFE8F, 0xFE90, 0xFE91, 0xFE92, true, true),
                ['\u0629'] = new Forms(0xFE93, 0xFE94, 0, 0, true, false),
                ['\u062A'] = new Forms(0xFE95, 0xFE96, 0xFE97, 0xFE98, true, true),
                ['\u062B'] = new Forms(0xFE99, 0xFE9A, 0xFE9B, 0xFE9C, true, true),
                ['\u062C'] = new Forms(0xFE9D, 0xFE9E, 0xFE9F, 0xFEA0, true, true),
                ['\u062D'] = new Forms(0xFEA1, 0xFEA2, 0xFEA3, 0xFEA4, true, true),
                ['\u062E'] = new Forms(0xFEA5, 0xFEA6, 0xFEA7, 0xFEA8, true, true),
                ['\u062F'] = new Forms(0xFEA9, 0xFEAA, 0, 0, true, false),
                ['\u0630'] = new Forms(0xFEAB, 0xFEAC, 0, 0, true, false),
                ['\u0631'] = new Forms(0xFEAD, 0xFEAE, 0, 0, true, false),
                ['\u0632'] = new Forms(0xFEAF, 0xFEB0, 0, 0, true, false),
                ['\u0633'] = new Forms(0xFEB1, 0xFEB2, 0xFEB3, 0xFEB4, true, true),
                ['\u0634'] = new Forms(0xFEB5, 0xFEB6, 0xFEB7, 0xFEB8, true, true),
                ['\u0635'] = new Forms(0xFEB9, 0xFEBA, 0xFEBB, 0xFEBC, true, true),
                ['\u0636'] = new Forms(0xFEBD, 0xFEBE, 0xFEBF, 0xFEC0, true, true),
                ['\u0637'] = new Forms(0xFEC1, 0xFEC2, 0xFEC3, 0xFEC4, true, true),
                ['\u0638'] = new Forms(0xFEC5, 0xFEC6, 0xFEC7, 0xFEC8, true, true),
                ['\u0639'] = new Forms(0xFEC9, 0xFECA, 0xFECB, 0xFECC, true, true),
                ['\u063A'] = new Forms(0xFECD, 0xFECE, 0xFECF, 0xFED0, true, true),
                ['\u0641'] = new Forms(0xFED1, 0xFED2, 0xFED3, 0xFED4, true, true),
                ['\u0642'] = new Forms(0xFED5, 0xFED6, 0xFED7, 0xFED8, true, true),
                ['\u0643'] = new Forms(0xFED9, 0xFEDA, 0xFEDB, 0xFEDC, true, true),
                ['\u0644'] = new Forms(0xFEDD, 0xFEDE, 0xFEDF, 0xFEE0, true, true),
                ['\u0645'] = new Forms(0xFEE1, 0xFEE2, 0xFEE3, 0xFEE4, true, true),
                ['\u0646'] = new Forms(0xFEE5, 0xFEE6, 0xFEE7, 0xFEE8, true, true),
                ['\u0647'] = new Forms(0xFEE9, 0xFEEA, 0xFEEB, 0xFEEC, true, true),
                ['\u0648'] = new Forms(0xFEED, 0xFEEE, 0, 0, true, false),
                ['\u0649'] = new Forms(0xFEEF, 0xFEF0, 0, 0, true, false),
                ['\u064A'] = new Forms(0xFEF1, 0xFEF2, 0xFEF3, 0xFEF4, true, true),
                ['\u067E'] = new Forms(0xFB56, 0xFB57, 0xFB58, 0xFB59, true, true),
                ['\u0686'] = new Forms(0xFB7A, 0xFB7B, 0xFB7C, 0xFB7D, true, true),
                ['\u0698'] = new Forms(0xFB8A, 0xFB8B, 0, 0, true, false),
                ['\u06A9'] = new Forms(0xFB8E, 0xFB8F, 0xFB90, 0xFB91, true, true),
                ['\u06AF'] = new Forms(0xFB92, 0xFB93, 0xFB94, 0xFB95, true, true)
            };
        }
    }
}
