using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Localization
{
    public enum FspLanguage
    {
        English,
        Arabic,
        Hindi,
        Turkish,
        PortugueseBrazil,
        Indonesian
    }

    /// <summary>
    /// Lightweight runtime localization for generated Lobby/Match UI.
    /// English source strings are stable keys. The first launch follows the device language,
    /// while a manual choice is persisted in PlayerPrefs.
    /// </summary>
    public sealed class FspLocalizationRuntime : MonoBehaviour
    {
        private const string PrefKey = "fsp.language";
        private static FspLocalizationRuntime instance;
        private static readonly Dictionary<string, string[]> Table = BuildTable();
        private static readonly string[] LanguageLabels =
        {
            "English", "العربية", "हिन्दी", "Türkçe", "Português (Brasil)", "Bahasa Indonesia"
        };

        private float nextScan;
        private Button languageButton;
        private Text languageButtonText;

        public static FspLanguage Current { get; private set; } = FspLanguage.English;
        public static bool IsRtl => Current == FspLanguage.Arabic;
        public static event Action<FspLanguage> LanguageChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            GameObject go = new GameObject("FspLocalizationRuntime");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<FspLocalizationRuntime>();
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
            Current = LoadLanguage();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            languageButton = null;
            languageButtonText = null;
            ApplyAllVisibleText();
            if (string.Equals(scene.name, "Lobby", StringComparison.OrdinalIgnoreCase))
                EnsureLanguageButton();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 0.5f;
            ApplyAllVisibleText();
            if (string.Equals(SceneManager.GetActiveScene().name, "Lobby", StringComparison.OrdinalIgnoreCase))
                EnsureLanguageButton();
        }

        public static string T(string english)
        {
            if (string.IsNullOrEmpty(english)) return english;
            if (!Table.TryGetValue(english, out string[] values)) return english;
            int index = Mathf.Clamp((int)Current, 0, values.Length - 1);
            string value = values[index];
            return string.IsNullOrEmpty(value) ? english : value;
        }

        public static void SetLanguage(FspLanguage language)
        {
            Current = language;
            PlayerPrefs.SetInt(PrefKey, (int)language);
            PlayerPrefs.Save();
            LanguageChanged?.Invoke(language);
            RefreshNow();
        }

        public static void RefreshNow()
        {
            if (instance == null) return;
            instance.ApplyAllVisibleText();
            instance.RefreshLanguageButton();
        }

        private static FspLanguage LoadLanguage()
        {
            if (PlayerPrefs.HasKey(PrefKey))
                return (FspLanguage)Mathf.Clamp(PlayerPrefs.GetInt(PrefKey), 0, 5);

            // Some Unity versions do not expose every locale (notably Hindi) as a strongly typed
            // SystemLanguage enum member. Compare the runtime enum name as text so the code compiles
            // across Unity versions while still recognizing devices that report "Hindi".
            string system = Application.systemLanguage.ToString();
            if (string.Equals(system, "Arabic", StringComparison.OrdinalIgnoreCase)) return FspLanguage.Arabic;
            if (string.Equals(system, "Hindi", StringComparison.OrdinalIgnoreCase)) return FspLanguage.Hindi;
            if (string.Equals(system, "Turkish", StringComparison.OrdinalIgnoreCase)) return FspLanguage.Turkish;
            if (string.Equals(system, "Portuguese", StringComparison.OrdinalIgnoreCase)) return FspLanguage.PortugueseBrazil;
            if (string.Equals(system, "Indonesian", StringComparison.OrdinalIgnoreCase)) return FspLanguage.Indonesian;
            return FspLanguage.English;
        }

        private void ApplyAllVisibleText()
        {
            Text[] texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Text text in texts)
            {
                if (text == null || text == languageButtonText) continue;
                ApplyText(text);
            }
        }

        private static void ApplyText(Text text)
        {
            string source = text.text ?? string.Empty;
            string normalized = RecoverEnglishKey(source);
            string translated = TranslateDynamic(normalized);
            text.text = translated;

            if (IsRtl)
            {
                if (text.alignment == TextAnchor.MiddleLeft) text.alignment = TextAnchor.MiddleRight;
                else if (text.alignment == TextAnchor.UpperLeft) text.alignment = TextAnchor.UpperRight;
                else if (text.alignment == TextAnchor.LowerLeft) text.alignment = TextAnchor.LowerRight;
            }
            else
            {
                if (text.alignment == TextAnchor.MiddleRight) text.alignment = TextAnchor.MiddleLeft;
                else if (text.alignment == TextAnchor.UpperRight) text.alignment = TextAnchor.UpperLeft;
                else if (text.alignment == TextAnchor.LowerRight) text.alignment = TextAnchor.LowerLeft;
            }
        }

        private static string RecoverEnglishKey(string current)
        {
            foreach (KeyValuePair<string, string[]> pair in Table)
            {
                foreach (string value in pair.Value)
                {
                    if (string.Equals(current, value, StringComparison.Ordinal)) return pair.Key;
                }
            }
            return current;
        }

        private static string TranslateDynamic(string source)
        {
            if (Table.ContainsKey(source)) return T(source);

            string[] prefixes = { "ALIVE ", "KILLS ", "AMMO ", "MEDKITS ", "RANK ", "XP ", "MATCHES ", "WINS " };
            foreach (string prefix in prefixes)
            {
                if (!source.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                string suffix = source.Substring(prefix.Length);
                return T(prefix.TrimEnd()) + " " + suffix;
            }

            if (source.StartsWith("Sending invite to ", StringComparison.OrdinalIgnoreCase))
                return T("Sending invite to") + " " + source.Substring("Sending invite to ".Length);

            return source;
        }

        private void EnsureLanguageButton()
        {
            if (languageButton != null) return;
            GameObject canvasGo = GameObject.Find("LobbyCanvas");
            if (canvasGo == null) return;
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            if (canvas == null) return;

            GameObject buttonGo = new GameObject("LanguageSelector", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(canvas.transform, false);
            RectTransform rt = buttonGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.79f, 0.91f);
            rt.anchorMax = new Vector2(0.965f, 0.975f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image image = buttonGo.GetComponent<Image>();
            image.color = new Color(0.025f, 0.055f, 0.09f, 0.92f);
            languageButton = buttonGo.GetComponent<Button>();
            languageButton.onClick.AddListener(CycleLanguage);

            GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(buttonGo.transform, false);
            RectTransform lrt = labelGo.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            languageButtonText = labelGo.GetComponent<Text>();
            try { languageButtonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            languageButtonText.fontSize = 17;
            languageButtonText.alignment = TextAnchor.MiddleCenter;
            languageButtonText.color = new Color(0.96f, 0.94f, 0.89f, 1f);
            languageButtonText.resizeTextForBestFit = true;
            languageButtonText.resizeTextMinSize = 10;
            languageButtonText.resizeTextMaxSize = 17;
            languageButtonText.raycastTarget = false;
            RefreshLanguageButton();
        }

        private void CycleLanguage()
        {
            SetLanguage((FspLanguage)(((int)Current + 1) % 6));
        }

        private void RefreshLanguageButton()
        {
            if (languageButtonText != null)
                languageButtonText.text = LanguageLabels[(int)Current];
        }

        private static Dictionary<string, string[]> BuildTable()
        {
            // Order: EN, AR, HI, TR, PT-BR, ID.
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["START"] = A("START", "ابدأ", "शुरू करें", "BAŞLA", "INICIAR", "MULAI"),
                ["SOLO"] = A("SOLO", "فردي", "सोलो", "SOLO", "SOLO", "SOLO"),
                ["SQUAD"] = A("SQUAD", "فريق", "स्क्वाड", "TAKIM", "ESQUADRÃO", "SKUAD"),
                ["LOADOUT"] = A("LOADOUT", "العتاد", "लोडआउट", "TEÇHİZAT", "EQUIPAMENTO", "PERLENGKAPAN"),
                ["APPEARANCE"] = A("APPEARANCE", "المظهر", "रूप", "GÖRÜNÜM", "APARÊNCIA", "PENAMPILAN"),
                ["CAREER"] = A("CAREER", "المسيرة", "करियर", "KARİYER", "CARREIRA", "KARIER"),
                ["FIRE"] = A("FIRE", "إطلاق", "फ़ायर", "ATEŞ", "ATIRAR", "TEMBAK"),
                ["AIM"] = A("AIM", "تصويب", "निशाना", "NİŞAN", "MIRAR", "BIDIK"),
                ["JUMP"] = A("JUMP", "قفز", "कूदें", "ZIPLA", "PULAR", "LOMPAT"),
                ["RELOAD"] = A("RELOAD", "تلقيم", "रीलोड", "DOLDUR", "RECARREGAR", "ISI ULANG"),
                ["HEAL"] = A("HEAL", "علاج", "हील", "İYİLEŞ", "CURAR", "PULIHKAN"),
                ["USE"] = A("USE", "استخدم", "उपयोग", "KULLAN", "USAR", "GUNAKAN"),
                ["SWAP"] = A("SWAP", "تبديل", "बदलें", "DEĞİŞTİR", "TROCAR", "GANTI"),
                ["SPRINT"] = A("SPRINT", "ركض", "दौड़", "DEPAR", "CORRER", "LARI"),
                ["RETURN TO LOBBY"] = A("RETURN TO LOBBY", "العودة للردهة", "लॉबी पर लौटें", "LOBİYE DÖN", "VOLTAR AO LOBBY", "KEMBALI KE LOBI"),
                ["VICTORY"] = A("VICTORY", "فوز", "विजय", "ZAFER", "VITÓRIA", "MENANG"),
                ["DEFEAT"] = A("DEFEAT", "هزيمة", "हार", "YENİLGİ", "DERROTA", "KALAH"),
                ["ALIVE"] = A("ALIVE", "الأحياء", "जीवित", "HAYATTA", "VIVOS", "HIDUP"),
                ["KILLS"] = A("KILLS", "القتلات", "किल्स", "LEŞ", "ABATES", "KILL"),
                ["AMMO"] = A("AMMO", "الذخيرة", "गोला-बारूद", "MERMİ", "MUNIÇÃO", "AMUNISI"),
                ["MEDKITS"] = A("MEDKITS", "حقائب العلاج", "मेडकिट", "İLK YARDIM", "KITS MÉDICOS", "MEDKIT"),
                ["RANK"] = A("RANK", "الرتبة", "रैंक", "RÜTBE", "RANK", "PERINGKAT"),
                ["XP"] = A("XP", "الخبرة", "XP", "XP", "XP", "XP"),
                ["MATCHES"] = A("MATCHES", "المباريات", "मैच", "MAÇLAR", "PARTIDAS", "MATCH"),
                ["WINS"] = A("WINS", "الانتصارات", "जीत", "GALİBİYET", "VITÓRIAS", "MENANG"),
                ["INVITE PLAYER"] = A("INVITE PLAYER", "دعوة لاعب", "खिलाड़ी आमंत्रित करें", "OYUNCU DAVET ET", "CONVIDAR JOGADOR", "UNDANG PEMAIN"),
                ["SEND"] = A("SEND", "إرسال", "भेजें", "GÖNDER", "ENVIAR", "KIRIM"),
                ["CLOSE"] = A("CLOSE", "إغلاق", "बंद करें", "KAPAT", "FECHAR", "TUTUP"),
                ["Enter a player name first."] = A("Enter a player name first.", "أدخل اسم اللاعب أولاً.", "पहले खिलाड़ी का नाम दर्ज करें।", "Önce oyuncu adını gir.", "Digite o nome do jogador primeiro.", "Masukkan nama pemain terlebih dahulu."),
                ["Sign in first to create a squad and invite players."] = A("Sign in first to create a squad and invite players.", "سجّل الدخول أولاً لإنشاء فريق ودعوة اللاعبين.", "स्क्वाड बनाने और खिलाड़ियों को आमंत्रित करने के लिए पहले साइन इन करें।", "Takım oluşturmak ve oyuncu davet etmek için önce giriş yap.", "Entre primeiro para criar um esquadrão e convidar jogadores.", "Masuk terlebih dahulu untuk membuat skuad dan mengundang pemain."),
                ["Squad service is not connected in this build."] = A("Squad service is not connected in this build.", "خدمة الفرق غير متصلة في هذا الإصدار.", "इस बिल्ड में स्क्वाड सेवा कनेक्ट नहीं है।", "Bu sürümde takım hizmeti bağlı değil.", "O serviço de esquadrão não está conectado nesta versão.", "Layanan skuad tidak terhubung pada build ini."),
                ["RETURN TO THE SAFE ZONE"] = A("RETURN TO THE SAFE ZONE", "ارجع إلى المنطقة الآمنة", "सुरक्षित क्षेत्र में लौटें", "GÜVENLİ BÖLGEYE DÖN", "VOLTE PARA A ZONA SEGURA", "KEMBALI KE ZONA AMAN"),
                ["Sending invite to"] = A("Sending invite to", "جارٍ إرسال الدعوة إلى", "आमंत्रण भेजा जा रहा है", "Davet gönderiliyor", "Enviando convite para", "Mengirim undangan ke")
            };
        }

        private static string[] A(string en, string ar, string hi, string tr, string pt, string id)
            => new[] { en, ar, hi, tr, pt, id };
    }
}
