using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Localization
{
    public sealed class FspLocalizationCoverageRuntime : MonoBehaviour
    {
        private static FspLocalizationCoverageRuntime instance;
        private static readonly Dictionary<string, string[]> Table = BuildTable();
        private float nextScan;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            GameObject go = new GameObject("FspLocalizationCoverageRuntime");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<FspLocalizationCoverageRuntime>();
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

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => ApplyAll();
        private void HandleLanguageChanged(FspLanguage language) => ApplyAll();

        private void Update()
        {
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 0.5f;
            ApplyAll();
        }

        private static void ApplyAll()
        {
            Text[] texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Text text in texts)
            {
                if (text == null || string.IsNullOrEmpty(text.text)) continue;
                string translated = TranslateBlock(text.text);
                if (!string.Equals(translated, text.text, StringComparison.Ordinal)) text.text = translated;
            }
        }

        private static string TranslateBlock(string source)
        {
            string exactKey = RecoverKey(source);
            if (Table.ContainsKey(exactKey)) return T(exactKey);

            string normalized = source.Replace("\r\n", "\n");
            if (!normalized.Contains("\n")) return TranslateLine(normalized);

            string[] lines = normalized.Split('\n');
            for (int i = 0; i < lines.Length; i++) lines[i] = TranslateLine(lines[i]);
            return string.Join("\n", lines);
        }

        private static string TranslateLine(string source)
        {
            if (string.IsNullOrEmpty(source)) return source;
            string key = RecoverKey(source);
            if (Table.ContainsKey(key)) return T(key);

            string[] prefixes =
            {
                "PLACE #", "RANK POINTS ", "PRIMARY: ", "ALIVE ", "KILLS ", "AMMO ",
                "MEDKITS ", "RANK ", "XP ", "MATCHES ", "WINS "
            };

            foreach (string prefix in prefixes)
            {
                string englishPrefix = prefix.TrimEnd();
                string recovered = RecoverDynamicPrefix(source, englishPrefix);
                if (recovered == null) continue;
                string suffix = englishPrefix == "PLACE #"
                    ? recovered.Substring("PLACE #".Length)
                    : recovered.Length > englishPrefix.Length ? recovered.Substring(englishPrefix.Length).TrimStart() : string.Empty;
                string translatedPrefix = T(englishPrefix);
                if (englishPrefix == "PLACE #") return translatedPrefix + suffix;
                return string.IsNullOrEmpty(suffix) ? translatedPrefix : translatedPrefix + " " + suffix;
            }

            if (source.EndsWith("...", StringComparison.Ordinal))
            {
                string withoutDots = source.Substring(0, source.Length - 3);
                string invitePrefix = RecoverDynamicPrefix(withoutDots, "Sending invite to");
                if (invitePrefix != null)
                {
                    string player = invitePrefix.Substring("Sending invite to".Length).TrimStart();
                    return T("Sending invite to") + " " + player + "...";
                }
            }
            return source;
        }

        private static string RecoverDynamicPrefix(string current, string englishPrefix)
        {
            if (current.StartsWith(englishPrefix, StringComparison.OrdinalIgnoreCase)) return current;
            if (!Table.TryGetValue(englishPrefix, out string[] values)) return null;
            foreach (string value in values)
            {
                if (!string.IsNullOrEmpty(value) && current.StartsWith(value, StringComparison.OrdinalIgnoreCase))
                    return englishPrefix + current.Substring(value.Length);
            }
            return null;
        }

        private static string RecoverKey(string current)
        {
            foreach (KeyValuePair<string, string[]> pair in Table)
                foreach (string value in pair.Value)
                    if (string.Equals(current, value, StringComparison.Ordinal)) return pair.Key;
            return current;
        }

        private static string T(string english)
        {
            if (!Table.TryGetValue(english, out string[] values)) return english;
            int index = Mathf.Clamp((int)FspLocalizationRuntime.Current, 0, values.Length - 1);
            string value = values[index];
            return string.IsNullOrEmpty(value) ? english : value;
        }

        private static Dictionary<string, string[]> BuildTable()
        {
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["MATCH COMPLETE"] = A("MATCH COMPLETE", "انتهت المباراة", "मैच समाप्त", "MAÇ TAMAMLANDI", "PARTIDA CONCLUÍDA", "PERTANDINGAN SELESAI"),
                ["PLACE #"] = A("PLACE #", "المركز #", "स्थान #", "SIRA #", "POSIÇÃO #", "POSISI #"),
                ["RANK POINTS"] = A("RANK POINTS", "نقاط التصنيف", "रैंक अंक", "RÜTBE PUANI", "PONTOS DE RANK", "POIN PERINGKAT"),
                ["PRIMARY:"] = A("PRIMARY:", "السلاح الأساسي:", "मुख्य:", "BİRİNCİL:", "PRIMÁRIA:", "UTAMA:"),
                ["PROFILE LOADING..."] = A("PROFILE LOADING...", "جارٍ تحميل الملف الشخصي...", "प्रोफ़ाइल लोड हो रही है...", "PROFİL YÜKLENİYOR...", "CARREGANDO PERFIL...", "MEMUAT PROFIL..."),
                ["Open CAREER again in a moment."] = A("Open CAREER again in a moment.", "افتح المسيرة مرة أخرى بعد قليل.", "कुछ देर बाद करियर फिर खोलें।", "Birazdan KARİYER'i tekrar aç.", "Abra CARREIRA novamente em instantes.", "Buka KARIER lagi sebentar lagi."),
                ["LOCAL OPERATIVE"] = A("LOCAL OPERATIVE", "لاعب محلي", "स्थानीय ऑपरेटिव", "YEREL OPERATİF", "OPERADOR LOCAL", "OPERATIF LOKAL"),
                ["Sign in to sync RANK, MATCHES, KILLS and WINS across devices."] = A("Sign in to sync RANK, MATCHES, KILLS and WINS across devices.", "سجّل الدخول لمزامنة الرتبة والمباريات والقتلات والانتصارات بين أجهزتك.", "डिवाइसों के बीच रैंक, मैच, किल और जीत सिंक करने के लिए साइन इन करें।", "RÜTBE, MAÇLAR, LEŞ ve GALİBİYET bilgilerini cihazlar arasında eşitlemek için giriş yap.", "Entre para sincronizar RANK, PARTIDAS, ABATES e VITÓRIAS entre dispositivos.", "Masuk untuk menyinkronkan PERINGKAT, MATCH, KILL, dan MENANG antar perangkat."),
                ["Your combat loadout is equipped for the next match."] = A("Your combat loadout is equipped for the next match.", "تم تجهيز عتادك القتالي للمباراة القادمة.", "आपका कॉम्बैट लोडआउट अगले मैच के लिए तैयार है।", "Savaş teçhizatın sonraki maç için hazır.", "Seu equipamento de combate está pronto para a próxima partida.", "Perlengkapan tempurmu siap untuk pertandingan berikutnya."),
                ["Use the left/right arrows below your operative to change character."] = A("Use the left/right arrows below your operative to change character.", "استخدم السهمين يمينًا ويسارًا أسفل الشخصية لتغييرها.", "किरदार बदलने के लिए ऑपरेटिव के नीचे बाएँ/दाएँ तीर इस्तेमाल करें।", "Karakteri değiştirmek için operatifin altındaki sol/sağ okları kullan.", "Use as setas esquerda/direita abaixo do operador para trocar de personagem.", "Gunakan panah kiri/kanan di bawah operatif untuk mengganti karakter."),
                ["Selected appearance is kept in the lobby profile and synced when signed in."] = A("Selected appearance is kept in the lobby profile and synced when signed in.", "يُحفظ المظهر المختار في ملف الردهة وتتم مزامنته عند تسجيل الدخول.", "चुना गया रूप लॉबी प्रोफ़ाइल में सहेजा जाता है और साइन इन पर सिंक होता है।", "Seçilen görünüm lobi profilinde tutulur ve giriş yapıldığında eşitlenir.", "A aparência selecionada fica salva no perfil do lobby e é sincronizada ao entrar.", "Penampilan terpilih disimpan di profil lobi dan disinkronkan saat masuk."),
                ["Enter the player name to send a squad invite. A squad will be created automatically if needed."] = A("Enter the player name to send a squad invite. A squad will be created automatically if needed.", "أدخل اسم اللاعب لإرسال دعوة للفريق. سيتم إنشاء فريق تلقائيًا عند الحاجة.", "स्क्वाड आमंत्रण भेजने के लिए खिलाड़ी का नाम दर्ज करें। जरूरत होने पर स्क्वाड अपने आप बनेगा।", "Takım daveti göndermek için oyuncu adını gir. Gerekirse takım otomatik oluşturulur.", "Digite o nome do jogador para enviar um convite de esquadrão. Um esquadrão será criado automaticamente se necessário.", "Masukkan nama pemain untuk mengirim undangan skuad. Skuad akan dibuat otomatis bila diperlukan."),
                ["Enter a player name first."] = A("Enter a player name first.", "أدخل اسم اللاعب أولاً.", "पहले खिलाड़ी का नाम दर्ज करें।", "Önce oyuncu adını gir.", "Digite o nome do jogador primeiro.", "Masukkan nama pemain terlebih dahulu."),
                ["Sign in first to create a squad and invite players."] = A("Sign in first to create a squad and invite players.", "سجّل الدخول أولاً لإنشاء فريق ودعوة اللاعبين.", "स्क्वाड बनाने और खिलाड़ियों को आमंत्रित करने के लिए पहले साइन इन करें।", "Takım oluşturmak ve oyuncu davet etmek için önce giriş yap.", "Entre primeiro para criar um esquadrão e convidar jogadores.", "Masuk terlebih dahulu untuk membuat skuad dan mengundang pemain."),
                ["Squad service is not connected in this build."] = A("Squad service is not connected in this build.", "خدمة الفرق غير متصلة في هذا الإصدار.", "इस बिल्ड में स्क्वाड सेवा कनेक्ट नहीं है।", "Bu sürümde takım hizmeti bağlı değil.", "O serviço de esquadrão não está conectado nesta versão.", "Layanan skuad tidak terhubung pada build ini."),
                ["Sending invite to"] = A("Sending invite to", "جارٍ إرسال الدعوة إلى", "आमंत्रण भेजा जा रहा है", "Davet gönderiliyor", "Enviando convite para", "Mengirim undangan ke"),
                ["YOU   READY"] = A("YOU   READY", "أنت   جاهز", "आप   तैयार", "SEN   HAZIRSIN", "VOCÊ   PRONTO", "KAMU   SIAP"),
                ["+ INVITE"] = A("+ INVITE", "+ دعوة", "+ आमंत्रित", "+ DAVET", "+ CONVIDAR", "+ UNDANG"),
                ["BATTLE ROYALE  •  REGION AUTO"] = A("BATTLE ROYALE  •  REGION AUTO", "باتل رويال  •  المنطقة تلقائيًا", "बैटल रॉयल  •  क्षेत्र ऑटो", "BATTLE ROYALE  •  BÖLGE OTOMATİK", "BATTLE ROYALE  •  REGIÃO AUTO", "BATTLE ROYALE  •  REGION OTOMATIS"),
                ["ONLINE  •  PING --"] = A("ONLINE  •  PING --", "متصل  •  البنغ --", "ऑनलाइन  •  पिंग --", "ÇEVRİMİÇİ  •  PING --", "ONLINE  •  PING --", "ONLINE  •  PING --"),
                ["ONLINE  •  SOLO READY"] = A("ONLINE  •  SOLO READY", "متصل  •  الفردي جاهز", "ऑनलाइन  •  सोलो तैयार", "ÇEVRİMİÇİ  •  SOLO HAZIR", "ONLINE  •  SOLO PRONTO", "ONLINE  •  SOLO SIAP"),
                ["ONLINE  •  SQUAD READY"] = A("ONLINE  •  SQUAD READY", "متصل  •  الفريق جاهز", "ऑनलाइन  •  स्क्वाड तैयार", "ÇEVRİMİÇİ  •  TAKIM HAZIR", "ONLINE  •  ESQUADRÃO PRONTO", "ONLINE  •  SKUAD SIAP"),
                ["Lobby state unavailable"] = A("Lobby state unavailable", "حالة الردهة غير متاحة", "लॉबी स्थिति उपलब्ध नहीं है", "Lobi durumu kullanılamıyor", "Estado do lobby indisponível", "Status lobi tidak tersedia"),
                ["ALIVE"] = A("ALIVE", "الأحياء", "जीवित", "HAYATTA", "VIVOS", "HIDUP"),
                ["KILLS"] = A("KILLS", "القتلات", "किल्स", "LEŞ", "ABATES", "KILL"),
                ["AMMO"] = A("AMMO", "الذخيرة", "गोला-बारूद", "MERMİ", "MUNIÇÃO", "AMUNISI"),
                ["MEDKITS"] = A("MEDKITS", "حقائب العلاج", "मेडकिट", "İLK YARDIM", "KITS MÉDICOS", "MEDKIT"),
                ["RANK"] = A("RANK", "الرتبة", "रैंक", "RÜTBE", "RANK", "PERINGKAT"),
                ["XP"] = A("XP", "الخبرة", "XP", "XP", "XP", "XP"),
                ["MATCHES"] = A("MATCHES", "المباريات", "मैच", "MAÇLAR", "PARTIDAS", "MATCH"),
                ["WINS"] = A("WINS", "الانتصارات", "जीत", "GALİBİYET", "VITÓRIAS", "MENANG")
            };
        }

        private static string[] A(string en, string ar, string hi, string tr, string pt, string id)
            => new[] { en, ar, hi, tr, pt, id };
    }
}
