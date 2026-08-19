using System;
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
            Write("SCENE", "Loaded=" + scene.name + " mode=" + mode);
            ProbeScene(scene.name, true);
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
            }
            ProbeScene(scene, false);
        }

        private void ProbeScene(string scene, bool immediate)
        {
            Camera main = Camera.main;
            int canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length;
            Texture2D lobbyArt = Resources.Load<Texture2D>("Lobby/lobby_reference");
            Write("PROBE", "scene=" + scene + " mainCamera=" + (main != null) + " canvases=" + canvases + " lobbyArt=" + (lobbyArt != null));

            if (scene.Equals("Lobby", StringComparison.OrdinalIgnoreCase))
            {
                bool lobbyVisible = GameObject.Find("LobbyCanvas") != null || GameObject.Find("FSP_DiagnosticRecovery") != null;
                if (!lobbyVisible && Time.realtimeSinceStartup - sceneEnteredAt > 3.5f)
                    BuildRecovery("Lobby UI did not appear", "LobbyArt=" + (lobbyArt != null) + " Camera=" + (main != null) + " CanvasCount=" + canvases);
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
            }
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
            copy.GetComponent<Button>().onClick.AddListener(() =>
            {
                try
                {
                    GUIUtility.systemCopyBuffer = File.Exists(logPath) ? File.ReadAllText(logPath) : "Diagnostic log not found";
                    Write("USER", "Diagnostic log copied to clipboard");
                }
                catch (Exception ex) { Write("COPY_ERROR", ex.ToString()); }
            });
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
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
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
