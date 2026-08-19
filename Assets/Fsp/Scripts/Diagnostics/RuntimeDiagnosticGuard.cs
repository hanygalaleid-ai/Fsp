using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Diagnostics
{
    public sealed class RuntimeDiagnosticGuard : MonoBehaviour
    {
        private static RuntimeDiagnosticGuard instance;
        private static readonly object Gate = new object();
        private static string logPath;
        private float nextProbe;
        private float sceneEnteredAt;
        private string lastScene;
        private bool visualCheckQueued;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            if (instance != null) return;
            GameObject go = new GameObject("FSP_RuntimeDiagnosticGuard");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<RuntimeDiagnosticGuard>();
        }

        private void Awake()
        {
            logPath = Path.Combine(Application.persistentDataPath, "fsp_runtime_diagnostic.log");
            Application.logMessageReceivedThreaded += OnLog;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Write("BOOT", DeviceSnapshot());
            lastScene = SceneManager.GetActiveScene().name;
            sceneEnteredAt = Time.realtimeSinceStartup;
        }

        private void Start()
        {
            EnsureAlwaysAvailableLogButton();
        }

        private void OnDestroy()
        {
            Application.logMessageReceivedThreaded -= OnLog;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (instance == this) instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            lastScene = scene.name;
            sceneEnteredAt = Time.realtimeSinceStartup;
            visualCheckQueued = false;
            Write("SCENE", "Loaded=" + scene.name + " mode=" + mode);
            ProbeScene(scene.name, true);
            EnsureAlwaysAvailableLogButton();
        }

        private void Update()
        {
            if (Time.realtimeSinceStartup < nextProbe) return;
            nextProbe = Time.realtimeSinceStartup + 2f;
            string scene = SceneManager.GetActiveScene().name;
            if (scene != lastScene)
            {
                lastScene = scene;
                sceneEnteredAt = Time.realtimeSinceStartup;
                visualCheckQueued = false;
            }
            ProbeScene(scene, false);

            if (!visualCheckQueued && Time.realtimeSinceStartup - sceneEnteredAt > 4f)
            {
                visualCheckQueued = true;
                StartCoroutine(CheckRenderedFrame(scene));
            }
        }

        private void ProbeScene(string scene, bool immediate)
        {
            Camera main = Camera.main;
            Canvas[] canvasList = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            int canvases = canvasList.Length;
            Texture2D lobbyArt = Resources.Load<Texture2D>("Lobby/lobby_reference");

            int primitiveLike = 0;
            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (Renderer r in renderers)
            {
                if (r == null || !r.enabled) continue;
                string n = r.gameObject.name.ToLowerInvariant();
                if (n.Contains("cube") || n.Contains("capsule") || n.Contains("placeholder") || n.Contains("prototype")) primitiveLike++;
            }

            Write("PROBE", "scene=" + scene + " mainCamera=" + (main != null) + " canvases=" + canvases + " lobbyArt=" + (lobbyArt != null) + " primitiveLike=" + primitiveLike + " renderers=" + renderers.Length);

            if (scene.Equals("Lobby", StringComparison.OrdinalIgnoreCase))
            {
                GameObject lobbyCanvas = GameObject.Find("LobbyCanvas");
                bool lobbyVisible = lobbyCanvas != null || GameObject.Find("FSP_DiagnosticRecovery") != null;
                RawImage backdrop = null;
                if (lobbyCanvas != null)
                {
                    RawImage[] raws = lobbyCanvas.GetComponentsInChildren<RawImage>(true);
                    foreach (RawImage raw in raws)
                    {
                        if (raw != null && raw.gameObject.name == "SunscarBackdrop") { backdrop = raw; break; }
                    }
                }

                bool artBound = backdrop != null && backdrop.texture != null;
                Write("LOBBY_VISUAL", "canvas=" + (lobbyCanvas != null) + " artBound=" + artBound + " primitiveLike=" + primitiveLike);

                if (!lobbyVisible && Time.realtimeSinceStartup - sceneEnteredAt > 3.5f)
                    BuildRecovery("Lobby UI did not appear", "LobbyArt=" + (lobbyArt != null) + " Camera=" + (main != null) + " CanvasCount=" + canvases);
                else if (lobbyCanvas != null && !artBound && Time.realtimeSinceStartup - sceneEnteredAt > 3.5f)
                    BuildRecovery("Lobby background is not bound", "ResourceArt=" + (lobbyArt != null) + " RawImage=" + (backdrop != null) + " primitiveLike=" + primitiveLike);
                else if (primitiveLike >= 3 && Time.realtimeSinceStartup - sceneEnteredAt > 4f)
                    BuildRecovery("Prototype geometry is visible in Lobby", "primitiveLike=" + primitiveLike + " ResourceArt=" + (lobbyArt != null));
            }
            else if (scene.Equals("Match", StringComparison.OrdinalIgnoreCase))
            {
                if (main == null && Time.realtimeSinceStartup - sceneEnteredAt > 2f)
                {
                    GameObject cameraGo = new GameObject("Main Camera");
                    cameraGo.tag = "MainCamera";
                    Camera camera = cameraGo.AddComponent<Camera>();
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.backgroundColor = new Color(0.18f, 0.28f, 0.34f, 1f);
                    cameraGo.AddComponent<AudioListener>();
                    cameraGo.transform.position = new Vector3(0f, 6f, -10f);
                    Write("RECOVERY", "Created missing Match Main Camera");
                }

                if (canvases == 0 && Time.realtimeSinceStartup - sceneEnteredAt > 5f)
                    BuildRecovery("Match HUD did not appear", "Camera=" + (Camera.main != null));

                if (primitiveLike >= 8 && Time.realtimeSinceStartup - sceneEnteredAt > 5f)
                    Write("MATCH_VISUAL_WARNING", "High prototype geometry count=" + primitiveLike);
            }
        }

        private IEnumerator CheckRenderedFrame(string scene)
        {
            yield return new WaitForEndOfFrame();
            Texture2D sample = null;
            try
            {
                int w = Mathf.Max(32, Mathf.Min(160, Screen.width / 8));
                int h = Mathf.Max(18, Mathf.Min(90, Screen.height / 8));
                sample = new Texture2D(w, h, TextureFormat.RGB24, false);
                float sx = Screen.width / (float)w;
                float sy = Screen.height / (float)h;
                int nearWhite = 0;
                int dark = 0;
                int total = 0;

                // Sample the rendered screen sparsely without storing a full screenshot.
                Texture2D full = ScreenCapture.CaptureScreenshotAsTexture();
                if (full == null) { Write("VISUAL", "Screen capture returned null"); yield break; }

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        Color c = full.GetPixel(Mathf.Clamp((int)(x * sx), 0, full.width - 1), Mathf.Clamp((int)(y * sy), 0, full.height - 1));
                        float lum = (c.r + c.g + c.b) / 3f;
                        if (lum > 0.93f && Mathf.Abs(c.r - c.g) < 0.06f && Mathf.Abs(c.g - c.b) < 0.06f) nearWhite++;
                        if (lum < 0.04f) dark++;
                        total++;
                    }
                }
                Destroy(full);

                float whiteRatio = total > 0 ? nearWhite / (float)total : 0f;
                float darkRatio = total > 0 ? dark / (float)total : 0f;
                Write("VISUAL", "scene=" + scene + " whiteRatio=" + whiteRatio.ToString("0.000") + " darkRatio=" + darkRatio.ToString("0.000") + " samples=" + total);

                if (scene.Equals("Match", StringComparison.OrdinalIgnoreCase) && whiteRatio > 0.72f)
                    BuildRecovery("Match render is washed out / nearly white", "whiteRatio=" + whiteRatio.ToString("0.000") + " Camera=" + (Camera.main != null));
                else if (darkRatio > 0.92f)
                    BuildRecovery("Rendered frame is almost completely black", "darkRatio=" + darkRatio.ToString("0.000") + " scene=" + scene);
            }
            catch (Exception ex)
            {
                Write("VISUAL_CHECK_ERROR", ex.ToString());
            }
            finally
            {
                if (sample != null) Destroy(sample);
            }
        }

        private static void EnsureAlwaysAvailableLogButton()
        {
            if (GameObject.Find("FSP_DiagnosticLogButton") != null) return;

            GameObject canvasGo = new GameObject("FSP_DiagnosticLogButton", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(canvasGo);
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 32761;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            GameObject buttonGo = new GameObject("LOG", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(canvasGo.transform, false);
            RectTransform rt = buttonGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.005f, 0.005f);
            rt.anchorMax = new Vector2(0.075f, 0.065f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            buttonGo.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.10f, 0.82f);

            Font font = null;
            try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            Text label = CreateText(buttonGo.transform, font, "LOG", 18, Vector2.zero, Vector2.one);
            label.color = new Color(1f, 0.55f, 0.12f, 1f);
            buttonGo.GetComponent<Button>().onClick.AddListener(CopyLogToClipboard);
        }

        private static void CopyLogToClipboard()
        {
            try
            {
                GUIUtility.systemCopyBuffer = File.Exists(logPath) ? File.ReadAllText(logPath) : "Diagnostic log not found";
                Write("USER", "Diagnostic log copied to clipboard");
            }
            catch (Exception ex) { Write("COPY_ERROR", ex.ToString()); }
        }

        private static void BuildRecovery(string reason, string details)
        {
            if (GameObject.Find("FSP_DiagnosticRecovery") != null) return;
            Write("RECOVERY", reason + " | " + details);

            GameObject canvasGo = new GameObject("FSP_DiagnosticRecovery", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(canvasGo);
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 32760;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasGo.transform, false);
            RectTransform pr = panel.GetComponent<RectTransform>();
            pr.anchorMin = Vector2.zero; pr.anchorMax = Vector2.one; pr.offsetMin = Vector2.zero; pr.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.015f, 0.03f, 0.05f, 1f);

            Font font = null;
            try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }

            Text title = CreateText(panel.transform, font, "SUNSCAR DIAGNOSTIC RECOVERY", 42, new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.82f));
            title.color = new Color(1f, 0.48f, 0.08f, 1f);
            Text body = CreateText(panel.transform, font, reason + "\n" + details + "\n\nA diagnostic log was saved on this device.", 26, new Vector2(0.10f, 0.30f), new Vector2(0.90f, 0.60f));
            body.color = Color.white;

            GameObject copy = new GameObject("CopyLog", typeof(RectTransform), typeof(Image), typeof(Button));
            copy.transform.SetParent(panel.transform, false);
            RectTransform cr = copy.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(0.35f, 0.12f); cr.anchorMax = new Vector2(0.65f, 0.22f); cr.offsetMin = Vector2.zero; cr.offsetMax = Vector2.zero;
            copy.GetComponent<Image>().color = new Color(0.92f, 0.36f, 0.04f, 1f);
            CreateText(copy.transform, font, "COPY DIAGNOSTIC LOG", 24, Vector2.zero, Vector2.one);
            copy.GetComponent<Button>().onClick.AddListener(CopyLogToClipboard);
        }

        private static Text CreateText(Transform parent, Font font, string value, int size, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            Text text = go.GetComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = size;
            return text;
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert || type == LogType.Warning)
                Write(type.ToString().ToUpperInvariant(), condition + "\n" + stackTrace);
        }

        private static string DeviceSnapshot()
        {
            StringBuilder b = new StringBuilder(512);
            b.Append("app=").Append(Application.version);
            b.Append(" unity=").Append(Application.unityVersion);
            b.Append(" platform=").Append(Application.platform);
            b.Append(" deviceModel=").Append(SystemInfo.deviceModel);
            b.Append(" os=").Append(SystemInfo.operatingSystem);
            b.Append(" ramMB=").Append(SystemInfo.systemMemorySize);
            b.Append(" gpu=").Append(SystemInfo.graphicsDeviceName);
            b.Append(" gpuMemMB=").Append(SystemInfo.graphicsMemorySize);
            b.Append(" gfxApi=").Append(SystemInfo.graphicsDeviceType);
            b.Append(" screen=").Append(Screen.width).Append('x').Append(Screen.height);
            b.Append(" path=").Append(Application.persistentDataPath);
            return b.ToString();
        }

        private static void Write(string tag, string message)
        {
            try
            {
                lock (Gate)
                {
                    if (string.IsNullOrEmpty(logPath)) return;
                    string line = DateTime.UtcNow.ToString("O") + " [" + tag + "] " + message + Environment.NewLine;
                    File.AppendAllText(logPath, line);
                    FileInfo info = new FileInfo(logPath);
                    if (info.Exists && info.Length > 512 * 1024)
                    {
                        string all = File.ReadAllText(logPath);
                        int keep = Math.Min(all.Length, 220000);
                        File.WriteAllText(logPath, "[TRIMMED]\n" + all.Substring(all.Length - keep));
                    }
                }
            }
            catch { }
        }
    }
}
