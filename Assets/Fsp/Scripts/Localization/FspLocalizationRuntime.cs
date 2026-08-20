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
        private RawImage languageButtonIcon;

        public static FspLanguage Current { get; private set; } = FspLanguage.English;
        public static string CurrentLabel => LanguageLabels[Mathf.Clamp((int)Current, 0, LanguageLabels.Length - 1)];
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

            string[] prefixes = { "ALIVE ", "KILLS ", "AMMO ", "MEDKITS ", "RANK ", "XP ", "MATCHES ", "WINS ", "HP ", "ARMOR ", "STARTING " };
            foreach (string prefix in prefixes)
            {
                if (!source.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                string suffix = source.Substring(prefix.Length);
                return T(prefix.TrimEnd()) + " " + suffix;
            }

            if (source.StartsWith("Sending invite to ", StringComparison.OrdinalIgnoreCase))
                return T("Sending invite to") + " " + source.Substring("Sending invite to ".Length);

            if (source.StartsWith("WebRTC: ", StringComparison.OrdinalIgnoreCase))
                return T("VOICE CONNECTION") + ": " + T(source.Substring("WebRTC: ".Length));

            if (source.StartsWith("Invalid voice signaling response: ", StringComparison.OrdinalIgnoreCase))
                return T("Invalid voice signaling response") + ": " + source.Substring("Invalid voice signaling response: ".Length);

            return source;
        }

        private void EnsureLanguageButton()
        {
            if (languageButton != null) return;
            GameObject canvasGo = GameObject.Find("ProductionLobbyCanvas") ?? GameObject.Find("LobbyCanvas");
            if (canvasGo == null) return;
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            if (canvas == null) return;
            Transform languageParent = GameObject.Find("LobbyContentFrame")?.transform ?? canvas.transform;

            GameObject buttonGo = new GameObject("LanguageSelector", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(languageParent, false);
            RectTransform rt = buttonGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.79f, 0.91f);
            rt.anchorMax = new Vector2(0.965f, 0.975f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image image = buttonGo.GetComponent<Image>();
            image.color = new Color(0.025f, 0.055f, 0.09f, 0.92f);
            languageButton = buttonGo.GetComponent<Button>();
            languageButton.onClick.AddListener(CycleLanguage);
            buttonGo.AddComponent<Fsp.Audio.FspUiClickAudio>();

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

            GameObject iconGo = new GameObject("LanguageIcon", typeof(RectTransform), typeof(RawImage));
            iconGo.transform.SetParent(buttonGo.transform, false);
            RectTransform iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.02f, 0.12f);
            iconRt.anchorMax = new Vector2(0.25f, 0.88f);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;
            languageButtonIcon = iconGo.GetComponent<RawImage>();
            languageButtonIcon.texture = Resources.Load<Texture2D>("UI/language_icons");
            languageButtonIcon.raycastTarget = false;
            labelGo.GetComponent<RectTransform>().anchorMin = new Vector2(0.25f, 0f);
            RefreshLanguageButton();
        }

        private void CycleLanguage()
        {
            SetLanguage((FspLanguage)(((int)Current + 1) % 6));
        }

        public static void SelectNextLanguage()
        {
            SetLanguage((FspLanguage)(((int)Current + 1) % 6));
        }

        private void RefreshLanguageButton()
        {
            if (languageButtonText != null)
                languageButtonText.text = LanguageLabels[(int)Current];
            if (languageButtonIcon != null)
                languageButtonIcon.uvRect = new Rect((int)Current / 6f, 0f, 1f / 6f, 1f);
        }

        private static Dictionary<string, string[]> BuildTable()
        {
            // Order: EN, AR, HI, TR, PT-BR, ID.
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["CONTINUE WITH GOOGLE"] = A("CONTINUE WITH GOOGLE", "المتابعة باستخدام Google", "Google से जारी रखें", "GOOGLE İLE DEVAM ET", "CONTINUAR COM GOOGLE", "LANJUTKAN DENGAN GOOGLE"),
                ["Opening Google sign in..."] = A("Opening Google sign in...", "جارٍ فتح تسجيل الدخول عبر Google...", "Google साइन इन खुल रहा है...", "Google girişi açılıyor...", "Abrindo login do Google...", "Membuka login Google..."),
                ["Google sign in failed"] = A("Google sign in failed", "فشل تسجيل الدخول عبر Google", "Google साइन इन विफल", "Google girişi başarısız", "Falha no login do Google", "Login Google gagal"),
                ["Signing out..."] = A("Signing out...", "جارٍ تسجيل الخروج...", "साइन आउट हो रहा है...", "Çıkış yapılıyor...", "Saindo...", "Sedang keluar..."),
                ["EQUIP & SAVE"] = A("EQUIP & SAVE", "استخدام وحفظ", "पहनें और सहेजें", "KUŞAN VE KAYDET", "EQUIPAR E SALVAR", "PAKAI & SIMPAN"),
                ["Saved on this device"] = A("Saved on this device", "تم الحفظ على هذا الجهاز", "इस डिवाइस पर सहेजा गया", "Bu cihaza kaydedildi", "Salvo neste dispositivo", "Disimpan di perangkat ini"),
                ["Saved to account"] = A("Saved to account", "تم الحفظ في الحساب", "खाते में सहेजा गया", "Hesaba kaydedildi", "Salvo na conta", "Disimpan ke akun"),
                ["Saved locally; account sync pending"] = A("Saved locally; account sync pending", "تم الحفظ محليًا؛ مزامنة الحساب معلقة", "लोकल सेव हुआ; खाता सिंक बाकी है", "Yerel kaydedildi; hesap eşitlemesi bekliyor", "Salvo localmente; sincronização pendente", "Tersimpan lokal; sinkronisasi akun tertunda"),
                ["Offline selection loaded"] = A("Offline selection loaded", "تم تحميل الاختيار المحلي", "ऑफ़लाइन चयन लोड हुआ", "Çevrimdışı seçim yüklendi", "Seleção offline carregada", "Pilihan offline dimuat"),
                ["HEAD"] = A("HEAD", "الرأس", "सिर", "BAŞ", "CABEÇA", "KEPALA"),
                ["FACE"] = A("FACE", "الوجه", "चेहरा", "YÜZ", "ROSTO", "WAJAH"),
                ["TORSO"] = A("TORSO", "الجزء العلوي", "धड़", "GÖVDE", "TRONCO", "ATASAN"),
                ["LEGS"] = A("LEGS", "الساقان", "पैर", "BACAKLAR", "PERNAS", "CELANA"),
                ["BACKPACK"] = A("BACKPACK", "حقيبة الظهر", "बैकपैक", "SIRT ÇANTASI", "MOCHILA", "RANSEL"),
                ["PARACHUTE"] = A("PARACHUTE", "المظلة", "पैराशूट", "PARAŞÜT", "PARAQUEDAS", "PARASUT"),
                ["FIELD HELMET"] = A("FIELD HELMET", "خوذة ميدانية", "फील्ड हेलमेट", "SAHA KASKI", "CAPACETE DE CAMPO", "HELM LAPANGAN"),
                ["SAND HELMET"] = A("SAND HELMET", "خوذة رملية", "रेत हेलमेट", "KUM KASKI", "CAPACETE AREIA", "HELM PASIR"),
                ["NIGHT HELMET"] = A("NIGHT HELMET", "خوذة ليلية", "नाइट हेलमेट", "GECE KASKI", "CAPACETE NOTURNO", "HELM MALAM"),
                ["CLEAR VISOR"] = A("CLEAR VISOR", "واقٍ شفاف", "साफ़ वाइज़र", "ŞEFFAF VİZÖR", "VISEIRA CLARA", "VISOR BENING"),
                ["AMBER VISOR"] = A("AMBER VISOR", "واقٍ كهرماني", "एम्बर वाइज़र", "KEHRİBAR VİZÖR", "VISEIRA ÂMBAR", "VISOR AMBER"),
                ["ICE VISOR"] = A("ICE VISOR", "واقٍ جليدي", "आइस वाइज़र", "BUZ VİZÖR", "VISEIRA GELO", "VISOR ES"),
                ["FOREST UNIFORM"] = A("FOREST UNIFORM", "زي الغابة", "फ़ॉरेस्ट यूनिफ़ॉर्म", "ORMAN ÜNİFORMASI", "UNIFORME FLORESTA", "SERAGAM HUTAN"),
                ["DESERT UNIFORM"] = A("DESERT UNIFORM", "زي الصحراء", "डेज़र्ट यूनिफ़ॉर्म", "ÇÖL ÜNİFORMASI", "UNIFORME DESERTO", "SERAGAM GURUN"),
                ["NIGHT UNIFORM"] = A("NIGHT UNIFORM", "زي ليلي", "नाइट यूनिफ़ॉर्म", "GECE ÜNİFORMASI", "UNIFORME NOTURNO", "SERAGAM MALAM"),
                ["FOREST TROUSERS"] = A("FOREST TROUSERS", "سروال الغابة", "फ़ॉरेस्ट ट्राउज़र", "ORMAN PANTOLONU", "CALÇA FLORESTA", "CELANA HUTAN"),
                ["DESERT TROUSERS"] = A("DESERT TROUSERS", "سروال الصحراء", "डेज़र्ट ट्राउज़र", "ÇÖL PANTOLONU", "CALÇA DESERTO", "CELANA GURUN"),
                ["NIGHT TROUSERS"] = A("NIGHT TROUSERS", "سروال ليلي", "नाइट ट्राउज़र", "GECE PANTOLONU", "CALÇA NOTURNA", "CELANA MALAM"),
                ["FIELD PACK"] = A("FIELD PACK", "حقيبة ميدانية", "फील्ड पैक", "SAHA ÇANTASI", "MOCHILA DE CAMPO", "RANSEL LAPANGAN"),
                ["SAND PACK"] = A("SAND PACK", "حقيبة رملية", "सैंड पैक", "KUM ÇANTASI", "MOCHILA AREIA", "RANSEL PASIR"),
                ["RESCUE PACK"] = A("RESCUE PACK", "حقيبة إنقاذ", "रेस्क्यू पैक", "KURTARMA ÇANTASI", "MOCHILA RESGATE", "RANSEL PENYELAMAT"),
                ["FOREST CANOPY"] = A("FOREST CANOPY", "مظلة الغابة", "फ़ॉरेस्ट कैनोपी", "ORMAN PARAŞÜTÜ", "PARAQUEDAS FLORESTA", "KANOPI HUTAN"),
                ["SAND CANOPY"] = A("SAND CANOPY", "مظلة رملية", "सैंड कैनोपी", "KUM PARAŞÜTÜ", "PARAQUEDAS AREIA", "KANOPI PASIR"),
                ["NIGHT CANOPY"] = A("NIGHT CANOPY", "مظلة ليلية", "नाइट कैनोपी", "GECE PARAŞÜTÜ", "PARAQUEDAS NOTURNO", "KANOPI MALAM"),
                ["START"] = A("START", "ابدأ", "शुरू करें", "BAŞLA", "INICIAR", "MULAI"),
                ["SOLO"] = A("SOLO", "فردي", "सोलो", "SOLO", "SOLO", "SOLO"),
                ["SQUAD"] = A("SQUAD", "فريق", "स्क्वाड", "TAKIM", "ESQUADRÃO", "SKUAD"),
                ["LOADOUT"] = A("LOADOUT", "العتاد", "लोडआउट", "TEÇHİZAT", "EQUIPAMENTO", "PERLENGKAPAN"),
                ["NEXT LOADOUT"] = A("NEXT LOADOUT", "العتاد التالي", "अगला लोडआउट", "SONRAKİ TEÇHİZAT", "PRÓXIMO EQUIPAMENTO", "PERLENGKAPAN BERIKUTNYA"),
                ["SELECTED"] = A("SELECTED", "المحدد", "चयनित", "SEÇİLİ", "SELECIONADO", "DIPILIH"),
                ["PRIMARY"] = A("PRIMARY", "الأساسي", "मुख्य", "BİRİNCİL", "PRIMÁRIA", "UTAMA"),
                ["TACTICAL"] = A("TACTICAL", "التكتيكي", "टैक्टिकल", "TAKTİK", "TÁTICO", "TAKTIS"),
                ["RIFLE"] = A("RIFLE", "بندقية", "राइफल", "TÜFEK", "RIFLE", "SENAPAN"),
                ["MED KIT"] = A("MED KIT", "حقيبة علاج", "मेड किट", "İLK YARDIM", "KIT MÉDICO", "MEDKIT"),
                ["ASSAULT"] = A("ASSAULT", "هجومي", "असॉल्ट", "TAARRUZ", "ASSALTO", "SERBU"),
                ["SCOUT"] = A("SCOUT", "استطلاع", "स्काउट", "KEŞİF", "BATEDOR", "PENGINTAI"),
                ["HEAVY"] = A("HEAVY", "ثقيل", "भारी", "AĞIR", "PESADO", "BERAT"),
                ["PLAY MATCHES"] = A("PLAY MATCHES", "العب مباريات", "मैच खेलें", "MAÇ OYNA", "JOGUE PARTIDAS", "MAIN MATCH"),
                ["ELIMINATE ENEMIES"] = A("ELIMINATE ENEMIES", "اقضِ على الأعداء", "दुश्मनों को हराएं", "DÜŞMANLARI ELE", "ELIMINE INIMIGOS", "KALAHKAN MUSUH"),
                ["FINISH TOP 10"] = A("FINISH TOP 10", "أنه ضمن أفضل 10", "शीर्ष 10 में रहें", "İLK 10'A GİR", "TERMINE NO TOP 10", "MASUK 10 BESAR"),
                ["CREATE / INVITE"] = A("CREATE / INVITE", "إنشاء / دعوة", "बनाएं / आमंत्रित करें", "OLUŞTUR / DAVET", "CRIAR / CONVIDAR", "BUAT / UNDANG"),
                ["CHECK INVITES"] = A("CHECK INVITES", "فحص الدعوات", "आमंत्रण देखें", "DAVETLERİ KONTROL ET", "VER CONVITES", "CEK UNDANGAN"),
                ["No pending invites"] = A("No pending invites", "لا توجد دعوات معلقة", "कोई लंबित आमंत्रण नहीं", "Bekleyen davet yok", "Nenhum convite pendente", "Tidak ada undangan tertunda"),
                ["Invite accepted"] = A("Invite accepted", "تم قبول الدعوة", "आमंत्रण स्वीकार हुआ", "Davet kabul edildi", "Convite aceito", "Undangan diterima"),
                ["Invite sent"] = A("Invite sent", "تم إرسال الدعوة", "आमंत्रण भेजा गया", "Davet gönderildi", "Convite enviado", "Undangan terkirim"),
                ["Squad created"] = A("Squad created", "تم إنشاء الفريق", "स्क्वाड बन गया", "Takım oluşturuldu", "Equipe criada", "Skuad dibuat"),
                ["Squad service unavailable"] = A("Squad service unavailable", "خدمة الفريق غير متاحة", "स्क्वाड सेवा उपलब्ध नहीं", "Takım hizmeti kullanılamıyor", "Serviço de equipe indisponível", "Layanan skuad tidak tersedia"),
                ["Only the squad leader can invite players"] = A("Only the squad leader can invite players", "قائد الفريق فقط يمكنه دعوة اللاعبين", "केवल स्क्वाड लीडर खिलाड़ियों को आमंत्रित कर सकता है", "Oyuncuları yalnızca takım lideri davet edebilir", "Somente o líder pode convidar jogadores", "Hanya pemimpin skuad yang dapat mengundang pemain"),
                ["Sign in to invite players"] = A("Sign in to invite players", "سجّل الدخول لدعوة اللاعبين", "खिलाड़ियों को आमंत्रित करने के लिए साइन इन करें", "Oyuncu davet etmek için giriş yap", "Entre para convidar jogadores", "Masuk untuk mengundang pemain"),
                ["Enter player name"] = A("Enter player name", "أدخل اسم اللاعب", "खिलाड़ी का नाम दर्ज करें", "Oyuncu adını gir", "Digite o nome do jogador", "Masukkan nama pemain"),
                ["Player not found"] = A("Player not found", "لم يتم العثور على اللاعب", "खिलाड़ी नहीं मिला", "Oyuncu bulunamadı", "Jogador não encontrado", "Pemain tidak ditemukan"),
                ["Ready"] = A("Ready", "جاهز", "तैयार", "Hazır", "Pronto", "Siap"),
                ["Not ready"] = A("Not ready", "غير جاهز", "तैयार नहीं", "Hazır değil", "Não está pronto", "Belum siap"),
                ["Left squad"] = A("Left squad", "غادرت الفريق", "स्क्वाड छोड़ दिया", "Takımdan ayrıldın", "Você saiu da equipe", "Keluar dari skuad"),
                ["Create a squad first"] = A("Create a squad first", "أنشئ فريقًا أولاً", "पहले स्क्वाड बनाएं", "Önce bir takım oluştur", "Crie uma equipe primeiro", "Buat skuad terlebih dahulu"),
                ["Only the squad leader can start matchmaking"] = A("Only the squad leader can start matchmaking", "قائد الفريق فقط يمكنه بدء البحث", "केवल स्क्वाड लीडर मैच खोज सकता है", "Eşleştirmeyi yalnızca takım lideri başlatabilir", "Somente o líder pode iniciar a busca", "Hanya pemimpin skuad yang dapat memulai pencarian"),
                ["All squad members must be ready"] = A("All squad members must be ready", "يجب أن يكون جميع أعضاء الفريق جاهزين", "स्क्वाड के सभी सदस्य तैयार होने चाहिए", "Tüm takım üyeleri hazır olmalı", "Todos os membros devem estar prontos", "Semua anggota skuad harus siap"),
                ["Searching for a match..."] = A("Searching for a match...", "جارٍ البحث عن مباراة...", "मैच खोजा जा रहा है...", "Maç aranıyor...", "Procurando partida...", "Mencari pertandingan..."),
                ["READY"] = A("READY", "جاهز", "तैयार", "HAZIR", "PRONTO", "SIAP"),
                ["LEAVE"] = A("LEAVE", "مغادرة", "छोड़ें", "AYRIL", "SAIR", "KELUAR"),
                ["TEAM"] = A("TEAM", "الفريق", "टीम", "TAKIM", "EQUIPE", "TIM"),
                ["MISSIONS"] = A("MISSIONS", "المهام", "मिशन", "GÖREVLER", "MISSÕES", "MISI"),
                ["SETTINGS"] = A("SETTINGS", "الإعدادات", "सेटिंग्स", "AYARLAR", "CONFIGURAÇÕES", "PENGATURAN"),
                ["BATTLE MODE"] = A("BATTLE MODE", "نمط المعركة", "बैटल मोड", "SAVAŞ MODU", "MODO DE BATALHA", "MODE PERTEMPURAN"),
                ["PLAYER"] = A("PLAYER", "اللاعب", "खिलाड़ी", "OYUNCU", "JOGADOR", "PEMAIN"),
                ["YOU"] = A("YOU", "أنت", "आप", "SEN", "VOCÊ", "KAMU"),
                ["GOLD"] = A("GOLD", "ذهب", "सोना", "ALTIN", "OURO", "EMAS"),
                ["COINS"] = A("COINS", "عملات", "सिक्के", "JETON", "MOEDAS", "KOIN"),
                ["ACCOUNT"] = A("ACCOUNT", "الحساب", "खाता", "HESAP", "CONTA", "AKUN"),
                ["EMAIL"] = A("EMAIL", "البريد الإلكتروني", "ईमेल", "E-POSTA", "E-MAIL", "EMAIL"),
                ["PASSWORD"] = A("PASSWORD", "كلمة المرور", "पासवर्ड", "PAROLA", "SENHA", "KATA SANDI"),
                ["PLAYER NAME"] = A("PLAYER NAME", "اسم اللاعب", "खिलाड़ी का नाम", "OYUNCU ADI", "NOME DO JOGADOR", "NAMA PEMAIN"),
                ["SIGN IN"] = A("SIGN IN", "تسجيل الدخول", "साइन इन", "GİRİŞ YAP", "ENTRAR", "MASUK"),
                ["CREATE ACCOUNT"] = A("CREATE ACCOUNT", "إنشاء حساب", "खाता बनाएं", "HESAP OLUŞTUR", "CRIAR CONTA", "BUAT AKUN"),
                ["SIGN OUT"] = A("SIGN OUT", "تسجيل الخروج", "साइन आउट", "ÇIKIŞ YAP", "SAIR", "KELUAR"),
                ["SOLDIER 01"] = A("SOLDIER 01", "الجندي 01", "सैनिक 01", "ASKER 01", "SOLDADO 01", "PRAJURIT 01"),
                ["SOLDIER 02"] = A("SOLDIER 02", "الجندي 02", "सैनिक 02", "ASKER 02", "SOLDADO 02", "PRAJURIT 02"),
                ["SOLDIER 03"] = A("SOLDIER 03", "الجندي 03", "सैनिक 03", "ASKER 03", "SOLDADO 03", "PRAJURIT 03"),
                ["ACCOUNT CONNECTED"] = A("ACCOUNT CONNECTED", "الحساب متصل", "खाता कनेक्ट है", "HESAP BAĞLI", "CONTA CONECTADA", "AKUN TERHUBUNG"),
                ["ACCOUNT OFFLINE"] = A("ACCOUNT OFFLINE", "الحساب دون اتصال", "खाता ऑफ़लाइन", "HESAP ÇEVRİMDIŞI", "CONTA OFFLINE", "AKUN OFFLINE"),
                ["GUEST • OFFLINE MATCH"] = A("GUEST • OFFLINE MATCH", "ضيف • مباراة دون اتصال", "अतिथि • ऑफ़लाइन मैच", "MİSAFİR • ÇEVRİMDIŞI MAÇ", "CONVIDADO • PARTIDA OFFLINE", "TAMU • MATCH OFFLINE"),
                ["LV. 1  •  ROOKIE"] = A("LV. 1  •  ROOKIE", "المستوى 1  •  مبتدئ", "स्तर 1  •  नौसिखिया", "SV. 1  •  ACEMİ", "NV. 1  •  NOVATO", "LV. 1  •  PEMULA"),
                ["GOLD  0     COINS  0"] = A("GOLD  0     COINS  0", "ذهب  0     عملات  0", "सोना  0     सिक्के  0", "ALTIN  0     JETON  0", "OURO  0     MOEDAS  0", "EMAS  0     KOIN  0"),
                ["MUSIC"] = A("MUSIC", "الموسيقى", "संगीत", "MÜZİK", "MÚSICA", "MUSIK"),
                ["SFX"] = A("SFX", "المؤثرات", "ध्वनि", "SES EFEKTLERİ", "EFEITOS", "EFEK SUARA"),
                ["GRAPHICS"] = A("GRAPHICS", "الجرافيك", "ग्राफ़िक्स", "GRAFİK", "GRÁFICOS", "GRAFIS"),
                ["LANGUAGE"] = A("LANGUAGE", "اللغة", "भाषा", "DİL", "IDIOMA", "BAHASA"),
                ["ON"] = A("ON", "تشغيل", "चालू", "AÇIK", "LIGADO", "NYALA"),
                ["OFF"] = A("OFF", "إيقاف", "बंद", "KAPALI", "DESLIGADO", "MATI"),
                ["LOW"] = A("LOW", "منخفض", "कम", "DÜŞÜK", "BAIXO", "RENDAH"),
                ["MEDIUM"] = A("MEDIUM", "متوسط", "मध्यम", "ORTA", "MÉDIO", "SEDANG"),
                ["HIGH"] = A("HIGH", "مرتفع", "उच्च", "YÜKSEK", "ALTO", "TINGGI"),
                ["TOGGLE"] = A("TOGGLE", "تغيير", "बदलें", "DEĞİŞTİR", "ALTERNAR", "UBAH"),
                ["CHANGE"] = A("CHANGE", "تغيير", "बदलें", "DEĞİŞTİR", "ALTERAR", "UBAH"),
                ["SAVE & CLOSE"] = A("SAVE & CLOSE", "حفظ وإغلاق", "सहेजें और बंद करें", "KAYDET VE KAPAT", "SALVAR E FECHAR", "SIMPAN & TUTUP"),
                ["PRIVACY"] = A("PRIVACY", "الخصوصية", "गोपनीयता", "GİZLİLİK", "PRIVACIDADE", "PRIVASI"),
                ["ACCOUNT & PRIVACY"] = A("ACCOUNT & PRIVACY", "الحساب والخصوصية", "खाता और गोपनीयता", "HESAP VE GİZLİLİK", "CONTA E PRIVACIDADE", "AKUN & PRIVASI"),
                ["OPEN FULL POLICY"] = A("OPEN FULL POLICY", "فتح السياسة كاملة", "पूरी नीति खोलें", "TAM POLİTİKAYI AÇ", "ABRIR POLÍTICA COMPLETA", "BUKA KEBIJAKAN LENGKAP"),
                ["DELETE ACCOUNT"] = A("DELETE ACCOUNT", "حذف الحساب", "खाता हटाएं", "HESABI SİL", "EXCLUIR CONTA", "HAPUS AKUN"),
                ["PRESS AGAIN TO DELETE"] = A("PRESS AGAIN TO DELETE", "اضغط مرة أخرى للحذف", "हटाने के लिए फिर दबाएं", "SİLMEK İÇİN TEKRAR BAS", "PRESSIONE NOVAMENTE PARA EXCLUIR", "TEKAN LAGI UNTUK MENGHAPUS"),
                ["NO ACCOUNT"] = A("NO ACCOUNT", "لا يوجد حساب", "कोई खाता नहीं", "HESAP YOK", "SEM CONTA", "TIDAK ADA AKUN"),
                ["ACCOUNT DELETED"] = A("ACCOUNT DELETED", "تم حذف الحساب", "खाता हटा दिया गया", "HESAP SİLİNDİ", "CONTA EXCLUÍDA", "AKUN DIHAPUS"),
                ["DELETE FAILED"] = A("DELETE FAILED", "فشل الحذف", "हटाना विफल", "SİLME BAŞARISIZ", "FALHA AO EXCLUIR", "GAGAL MENGHAPUS"),
                ["FSP stores your profile, match progress and squad data when you sign in. Voice chat uses the microphone only after permission. You can play offline without an account."] = A("FSP stores your profile, match progress and squad data when you sign in. Voice chat uses the microphone only after permission. You can play offline without an account.", "تحفظ FSP ملفك وتقدم المباريات وبيانات الفريق عند تسجيل الدخول. تستخدم المحادثة الصوتية الميكروفون بعد منح الإذن فقط. يمكنك اللعب دون اتصال وبدون حساب.", "साइन इन करने पर FSP आपकी प्रोफ़ाइल, मैच प्रगति और टीम डेटा सहेजता है। वॉइस चैट अनुमति के बाद ही माइक्रोफ़ोन उपयोग करता है। आप बिना खाते ऑफ़लाइन खेल सकते हैं।", "FSP giriş yaptığında profilini, maç ilerlemeni ve takım verilerini saklar. Sesli sohbet mikrofonu yalnızca izin sonrası kullanır. Hesapsız çevrimdışı oynayabilirsin.", "Ao entrar, o FSP armazena seu perfil, progresso e dados da equipe. O chat de voz usa o microfone somente após permissão. Você pode jogar offline sem conta.", "Saat masuk, FSP menyimpan profil, progres pertandingan, dan data tim. Obrolan suara memakai mikrofon hanya setelah izin. Kamu dapat bermain offline tanpa akun."),
                ["FSP stores your profile, match progress and squad data when you sign in. Voice chat uses the microphone only after permission. You can play offline without an account. Account deletion permanently removes your account and associated game data."] = A("FSP stores your profile, match progress and squad data when you sign in. Voice chat uses the microphone only after permission. You can play offline without an account. Account deletion permanently removes your account and associated game data.", "تحفظ FSP ملفك وتقدم المباريات وبيانات الفريق عند تسجيل الدخول. تستخدم المحادثة الصوتية الميكروفون بعد منح الإذن فقط. يمكنك اللعب دون اتصال وبدون حساب. يؤدي حذف الحساب إلى إزالة حسابك وبيانات اللعبة المرتبطة به نهائيًا.", "साइन इन करने पर FSP आपकी प्रोफ़ाइल, मैच प्रगति और टीम डेटा सहेजता है। वॉइस चैट अनुमति के बाद ही माइक्रोफ़ोन उपयोग करता है। आप बिना खाते ऑफ़लाइन खेल सकते हैं। खाता हटाने पर खाता और उससे जुड़ा गेम डेटा स्थायी रूप से मिट जाएगा।", "FSP giriş yaptığında profilini, maç ilerlemeni ve takım verilerini saklar. Sesli sohbet mikrofonu yalnızca izin sonrası kullanır. Hesapsız çevrimdışı oynayabilirsin. Hesap silme işlemi hesabını ve ilişkili oyun verilerini kalıcı olarak kaldırır.", "Ao entrar, o FSP armazena seu perfil, progresso e dados da equipe. O chat de voz usa o microfone somente após permissão. Você pode jogar offline sem conta. A exclusão remove permanentemente sua conta e os dados associados do jogo.", "Saat masuk, FSP menyimpan profil, progres pertandingan, dan data tim. Obrolan suara memakai mikrofon hanya setelah izin. Kamu dapat bermain offline tanpa akun. Penghapusan akun menghapus akun dan data game terkait secara permanen."),
                ["SECTION"] = A("SECTION", "القسم", "अनुभाग", "BÖLÜM", "SEÇÃO", "BAGIAN"),
                ["Invite players and manage your squad before deployment."] = A("Invite players and manage your squad before deployment.", "ادعُ اللاعبين وأدر فريقك قبل بدء المعركة.", "तैनाती से पहले खिलाड़ियों को आमंत्रित करें और टीम संभालें।", "Savaştan önce oyuncuları davet et ve takımını yönet.", "Convide jogadores e gerencie sua equipe antes da batalha.", "Undang pemain dan kelola tim sebelum bertempur."),
                ["PRIMARY  •  ASSAULT RIFLE\nSECONDARY  •  PISTOL\nTACTICAL  •  MED KIT"] = A("PRIMARY  •  ASSAULT RIFLE\nSECONDARY  •  PISTOL\nTACTICAL  •  MED KIT", "الأساسي  •  بندقية هجومية\nالثانوي  •  مسدس\nالتكتيكي  •  حقيبة علاج", "मुख्य  •  असॉल्ट राइफल\nद्वितीय  •  पिस्तौल\nटैक्टिकल  •  मेड किट", "BİRİNCİL  •  TAARRUZ TÜFEĞİ\nİKİNCİL  •  TABANCA\nTAKTİK  •  İLK YARDIM", "PRIMÁRIA  •  RIFLE DE ASSALTO\nSECUNDÁRIA  •  PISTOLA\nTÁTICO  •  KIT MÉDICO", "UTAMA  •  SENAPAN SERBU\nSEKUNDER  •  PISTOL\nTAKTIS  •  MEDKIT"),
                ["SURVIVE 5 MINUTES   0 / 1\nELIMINATE ENEMIES   0 / 3\nFINISH TOP 10       0 / 1"] = A("SURVIVE 5 MINUTES   0 / 1\nELIMINATE ENEMIES   0 / 3\nFINISH TOP 10       0 / 1", "ابقَ حيًا 5 دقائق   0 / 1\nاقضِ على الأعداء   0 / 3\nأنه ضمن أفضل 10   0 / 1", "5 मिनट जीवित रहें   0 / 1\nदुश्मनों को हराएं   0 / 3\nशीर्ष 10 में रहें   0 / 1", "5 DAKİKA HAYATTA KAL   0 / 1\nDÜŞMANLARI ELE   0 / 3\nİLK 10'A GİR   0 / 1", "SOBREVIVA 5 MINUTOS   0 / 1\nELIMINE INIMIGOS   0 / 3\nTERMINE NO TOP 10   0 / 1", "BERTAHAN 5 MENIT   0 / 1\nKALAHKAN MUSUH   0 / 3\nMASUK 10 BESAR   0 / 1"),
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
                ["VOICE"] = A("VOICE", "الصوت", "वॉइस", "SES", "VOZ", "SUARA"),
                ["VOICE OFFLINE"] = A("VOICE OFFLINE", "الصوت غير متصل", "वॉइस ऑफ़लाइन", "SES ÇEVRİMDIŞI", "VOZ OFFLINE", "SUARA OFFLINE"),
                ["SQUAD VOICE • READY"] = A("SQUAD VOICE • READY", "صوت الفريق • جاهز", "स्क्वाड वॉइस • तैयार", "TAKIM SESİ • HAZIR", "VOZ DO ESQUADRÃO • PRONTA", "SUARA SKUAD • SIAP"),
                ["SQUAD VOICE • TALKING"] = A("SQUAD VOICE • TALKING", "صوت الفريق • يتحدث", "स्क्वाड वॉइस • बोल रहे हैं", "TAKIM SESİ • KONUŞUYOR", "VOZ DO ESQUADRÃO • FALANDO", "SUARA SKUAD • BERBICARA"),
                ["HOLD TO TALK"] = A("HOLD TO TALK", "اضغط مطولاً للتحدث", "बोलने के लिए दबाए रखें", "KONUŞMAK İÇİN BASILI TUT", "SEGURE PARA FALAR", "TAHAN UNTUK BICARA"),
                ["TALKING..."] = A("TALKING...", "جارٍ التحدث...", "बोल रहे हैं...", "KONUŞUYOR...", "FALANDO...", "BERBICARA..."),
                ["Voice runtime not connected"] = A("Voice runtime not connected", "خدمة الصوت غير متصلة", "वॉइस सेवा कनेक्ट नहीं है", "Ses hizmeti bağlı değil", "Serviço de voz não conectado", "Layanan suara tidak terhubung"),
                ["Voice connected"] = A("Voice connected", "الصوت متصل", "वॉइस कनेक्ट है", "Ses bağlı", "Voz conectada", "Suara terhubung"),
                ["Voice connection failed"] = A("Voice connection failed", "فشل اتصال الصوت", "वॉइस कनेक्शन विफल", "Ses bağlantısı başarısız", "Falha na conexão de voz", "Koneksi suara gagal"),
                ["Voice disconnected"] = A("Voice disconnected", "تم قطع الصوت", "वॉइस डिस्कनेक्ट हो गई", "Ses bağlantısı kesildi", "Voz desconectada", "Suara terputus"),
                ["VOICE CONNECTION"] = A("VOICE CONNECTION", "اتصال الصوت", "वॉइस कनेक्शन", "SES BAĞLANTISI", "CONEXÃO DE VOZ", "KONEKSI SUARA"),
                ["Preparing squad voice..."] = A("Preparing squad voice...", "جارٍ تجهيز صوت الفريق...", "स्क्वाड वॉइस तैयार हो रही है...", "Takım sesi hazırlanıyor...", "Preparando voz da equipe...", "Menyiapkan suara skuad..."),
                ["Voice service endpoint is not configured."] = A("Voice service endpoint is not configured.", "عنوان خدمة الصوت غير مضبوط.", "वॉइस सेवा का पता कॉन्फ़िगर नहीं है।", "Ses hizmeti adresi yapılandırılmamış.", "O endereço do serviço de voz não está configurado.", "Alamat layanan suara belum dikonfigurasi."),
                ["Voice signaling endpoint is not configured."] = A("Voice signaling endpoint is not configured.", "عنوان إشارات الصوت غير مضبوط.", "वॉइस सिग्नलिंग पता कॉन्फ़िगर नहीं है।", "Ses sinyalleşme adresi yapılandırılmamış.", "O endereço de sinalização de voz não está configurado.", "Alamat pensinyalan suara belum dikonfigurasi."),
                ["Microphone permission denied."] = A("Microphone permission denied.", "تم رفض إذن الميكروفون.", "माइक्रोफ़ोन अनुमति अस्वीकार की गई।", "Mikrofon izni reddedildi.", "Permissão do microfone negada.", "Izin mikrofon ditolak."),
                ["Could not start microphone."] = A("Could not start microphone.", "تعذر تشغيل الميكروفون.", "माइक्रोफ़ोन शुरू नहीं हो सका।", "Mikrofon başlatılamadı.", "Não foi possível iniciar o microfone.", "Mikrofon tidak dapat dimulai."),
                ["Microphone did not begin capturing audio."] = A("Microphone did not begin capturing audio.", "لم يبدأ الميكروفون في التقاط الصوت.", "माइक्रोफ़ोन ने ऑडियो कैप्चर शुरू नहीं किया।", "Mikrofon ses yakalamaya başlamadı.", "O microfone não começou a captar áudio.", "Mikrofon tidak mulai menangkap audio."),
                ["Voice AudioSource setup failed."] = A("Voice AudioSource setup failed.", "فشل إعداد مصدر صوت المحادثة.", "वॉइस ऑडियो स्रोत सेटअप विफल रहा।", "Ses kaynağı kurulumu başarısız.", "Falha ao configurar a fonte de áudio da voz.", "Penyiapan sumber audio suara gagal."),
                ["Voice runtime is not initialized."] = A("Voice runtime is not initialized.", "نظام الصوت غير مهيأ.", "वॉइस सिस्टम आरंभ नहीं हुआ है।", "Ses sistemi başlatılmamış.", "O sistema de voz não foi inicializado.", "Sistem suara belum diinisialisasi."),
                ["Invalid remote SDP."] = A("Invalid remote SDP.", "بيانات اتصال الصوت البعيدة غير صالحة.", "रिमोट वॉइस कनेक्शन डेटा अमान्य है।", "Uzak ses bağlantı verisi geçersiz.", "Os dados da conexão de voz remota são inválidos.", "Data koneksi suara jarak jauh tidak valid."),
                ["Empty voice signaling response."] = A("Empty voice signaling response.", "استجابة خدمة الصوت فارغة.", "वॉइस सिग्नलिंग प्रतिक्रिया खाली है।", "Ses sinyalleşme yanıtı boş.", "A resposta da sinalização de voz está vazia.", "Respons pensinyalan suara kosong."),
                ["Invalid voice signaling response"] = A("Invalid voice signaling response", "استجابة خدمة الصوت غير صالحة", "वॉइस सिग्नलिंग प्रतिक्रिया अमान्य है", "Ses sinyalleşme yanıtı geçersiz", "A resposta da sinalização de voz é inválida", "Respons pensinyalan suara tidak valid"),
                ["Voice sync retrying"] = A("Voice sync retrying", "إعادة محاولة مزامنة الصوت", "वॉइस सिंक फिर आज़माया जा रहा है", "Ses eşitleme yeniden deneniyor", "Tentando sincronizar a voz novamente", "Mencoba sinkronisasi suara lagi"),
                ["Voice renegotiation retrying"] = A("Voice renegotiation retrying", "إعادة محاولة تحديث اتصال الصوت", "वॉइस कनेक्शन अपडेट फिर आज़माया जा रहा है", "Ses bağlantısı güncellemesi yeniden deneniyor", "Tentando atualizar a conexão de voz novamente", "Mencoba memperbarui koneksi suara lagi"),
                ["Voice stopped"] = A("Voice stopped", "تم إيقاف الصوت", "वॉइस बंद हो गई", "Ses durduruldu", "Voz interrompida", "Suara dihentikan"),
                ["WebRTC audio ready"] = A("WebRTC audio ready", "صوت WebRTC جاهز", "WebRTC ऑडियो तैयार है", "WebRTC sesi hazır", "Áudio WebRTC pronto", "Audio WebRTC siap"),
                ["Microphone muted"] = A("Microphone muted", "الميكروفون مكتوم", "माइक्रोफ़ोन म्यूट है", "Mikrofon kapalı", "Microfone silenciado", "Mikrofon dibisukan"),
                ["Microphone live"] = A("Microphone live", "الميكروفون مفتوح", "माइक्रोफ़ोन चालू है", "Mikrofon açık", "Microfone ativo", "Mikrofon aktif"),
                ["Not signed in."] = A("Not signed in.", "لم يتم تسجيل الدخول.", "साइन इन नहीं है।", "Giriş yapılmamış.", "Não conectado.", "Belum masuk."),
                ["New"] = A("New", "جديد", "नيو", "Yeni", "Nova", "Baru"),
                ["Connecting"] = A("Connecting", "جارٍ الاتصال", "कनेक्ट हो रहा है", "Bağlanıyor", "Conectando", "Menghubungkan"),
                ["Connected"] = A("Connected", "متصل", "कनेक्ट है", "Bağlandı", "Conectada", "Terhubung"),
                ["Disconnected"] = A("Disconnected", "غير متصل", "डिस्कनेक्ट है", "Bağlantı kesildi", "Desconectada", "Terputus"),
                ["Failed"] = A("Failed", "فشل", "विफल", "Başarısız", "Falhou", "Gagal"),
                ["Closed"] = A("Closed", "مغلق", "बंद", "Kapalı", "Fechada", "Ditutup"),
                ["RETURN TO LOBBY"] = A("RETURN TO LOBBY", "العودة للردهة", "लॉबी पर लौटें", "LOBİYE DÖN", "VOLTAR AO LOBBY", "KEMBALI KE LOBI"),
                ["MATCH COMPLETE"] = A("MATCH COMPLETE", "انتهت المباراة", "मैच समाप्त", "MAÇ TAMAMLANDI", "PARTIDA CONCLUÍDA", "PERTANDINGAN SELESAI"),
                ["VICTORY"] = A("VICTORY", "فوز", "विजय", "ZAFER", "VITÓRIA", "MENANG"),
                ["DEFEAT"] = A("DEFEAT", "هزيمة", "हार", "YENİLGİ", "DERROTA", "KALAH"),
                ["YOU WIN!"] = A("YOU WIN!", "الفوز لك!", "आप जीत गए!", "KAZANDIN!", "VOCÊ VENCEU!", "KAMU MENANG!"),
                ["You are the last player alive"] = A("You are the last player alive", "أنت آخر لاعب على قيد الحياة", "आप अंतिम जीवित खिलाड़ी हैं", "Hayatta kalan son oyuncusun", "Você é o último jogador vivo", "Kamu pemain terakhir yang hidup"),
                ["Winner:"] = A("Winner:", "الفائز:", "विजेता:", "Kazanan:", "Vencedor:", "Pemenang:"),
                ["No player remains alive"] = A("No player remains alive", "لم يتبق لاعب حي", "कोई खिलाड़ी जीवित नहीं है", "Hayatta oyuncu kalmadı", "Nenhum jogador permanece vivo", "Tidak ada pemain yang hidup"),
                ["ZONE"] = A("ZONE", "المنطقة", "ज़ोन", "BÖLGE", "ZONA", "ZONA"),
                ["HP"] = A("HP", "الصحة", "एचपी", "CAN", "VIDA", "HP"),
                ["ARMOR"] = A("ARMOR", "الدرع", "कवच", "ZIRH", "ARMADURA", "ARMOR"),
                ["LOADING MATCH"] = A("LOADING MATCH", "جارٍ تحميل المباراة", "मैच लोड हो रहा है", "MAÇ YÜKLENİYOR", "CARREGANDO PARTIDA", "MEMUAT PERTANDINGAN"),
                ["STARTING"] = A("STARTING", "تبدأ خلال", "शुरू हो रहा है", "BAŞLIYOR", "INICIANDO", "MULAI"),
                ["WAITING"] = A("WAITING", "في الانتظار", "प्रतीक्षा", "BEKLENİYOR", "AGUARDANDO", "MENUNGGU"),
                ["ACTIVE"] = A("ACTIVE", "نشطة", "सक्रिय", "AKTİF", "ATIVA", "AKTIF"),
                ["FINISHED"] = A("FINISHED", "انتهت", "समाप्त", "BİTTİ", "ENCERRADA", "SELESAI"),
                ["INTERACT"] = A("INTERACT", "تفاعل", "इंटरैक्ट", "ETKİLEŞİM", "INTERAGIR", "INTERAKSI"),
                ["Signing in..."] = A("Signing in...", "جارٍ تسجيل الدخول...", "साइन इन हो रहा है...", "Giriş yapılıyor...", "Entrando...", "Sedang masuk..."),
                ["Sign in failed"] = A("Sign in failed", "فشل تسجيل الدخول", "साइन इन विफल", "Giriş başarısız", "Falha ao entrar", "Gagal masuk"),
                ["Creating account..."] = A("Creating account...", "جارٍ إنشاء الحساب...", "खाता बनाया जा रहा है...", "Hesap oluşturuluyor...", "Criando conta...", "Membuat akun..."),
                ["Could not create account"] = A("Could not create account", "تعذر إنشاء الحساب", "खाता नहीं बन सका", "Hesap oluşturulamadı", "Não foi possível criar a conta", "Tidak dapat membuat akun"),
                ["Account created. Verify your email, then sign in."] = A("Account created. Verify your email, then sign in.", "تم إنشاء الحساب. تحقق من بريدك ثم سجل الدخول.", "खाता बन गया। ईमेल सत्यापित करके साइन इन करें।", "Hesap oluşturuldu. E-postanı doğrulayıp giriş yap.", "Conta criada. Verifique seu e-mail e entre.", "Akun dibuat. Verifikasi email lalu masuk."),
                ["Signed in"] = A("Signed in", "تم تسجيل الدخول", "साइन इन हो गया", "Giriş yapıldı", "Conectado", "Berhasil masuk"),
                ["Signed in, but profile could not be loaded"] = A("Signed in, but profile could not be loaded", "تم الدخول، لكن تعذر تحميل الملف الشخصي", "साइन इन हुआ, लेकिन प्रोफ़ाइल लोड नहीं हुई", "Giriş yapıldı ancak profil yüklenemedi", "Conectado, mas o perfil não pôde ser carregado", "Berhasil masuk, tetapi profil gagal dimuat"),
                ["Enter a valid email and a password of at least 6 characters"] = A("Enter a valid email and a password of at least 6 characters", "اكتب بريدًا صحيحًا وكلمة مرور 6 أحرف على الأقل", "मान्य ईमेल और कम से कम 6 अक्षरों का पासवर्ड दर्ज करें", "Geçerli e-posta ve en az 6 karakterli parola gir", "Digite um e-mail válido e senha de ao menos 6 caracteres", "Masukkan email valid dan kata sandi minimal 6 karakter"),
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
