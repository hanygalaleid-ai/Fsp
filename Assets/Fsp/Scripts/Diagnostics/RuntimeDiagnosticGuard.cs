using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Diagnostics
{
    /// <summary>
    /// Lightweight runtime diagnostics that never replace or cover the real game UI.
    /// It only records useful state to disk so release builds remain playable even when
    /// a diagnostic condition is detected.
    /// </summary>
    public sealed class RuntimeDiagnosticGuard : MonoBehaviour
    {
        private static RuntimeDiagnosticGuard instance;
        private static readonly object Gate = new object();
        private static string logPath;
        private float nextProbe;
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
            lastScene = SceneManager.GetActiveScene().name;
            Write("BOOT", DeviceSnapshot());
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
            Write("SCENE", "Loaded=" + scene.name + " mode=" + mode);
            ProbeScene(scene.name);
        }

        private void Update()
        {
            if (Time.realtimeSinceStartup < nextProbe) return;
            nextProbe = Time.realtimeSinceStartup + 2f;

            string scene = SceneManager.GetActiveScene().name;
            if (scene != lastScene)
            {
                lastScene = scene;
                Write("SCENE", "Active=" + scene);
            }

            ProbeScene(scene);
        }

        private static void ProbeScene(string scene)
        {
            Camera main = Camera.main;
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

            if (scene.Equals("Lobby", StringComparison.OrdinalIgnoreCase))
            {
                // The release lobby must be the responsive overlay canvas. The obsolete
                // world-space SpriteRenderer is intentionally disabled on every device.
                Sprite resourceArt = Resources.Load<Sprite>("Lobby/fsp_lobby_final");
                GameObject productionCanvas = GameObject.Find("ProductionLobbyCanvas");
                bool productionUiReady = productionCanvas != null && productionCanvas.GetComponent<Canvas>() != null;

                Write(
                    "LOBBY_VISUAL",
                    "camera=" + (main != null) +
                    " resourceArt=" + (resourceArt != null) +
                    " productionUi=" + productionUiReady +
                    " canvases=" + canvases.Length +
                    " renderers=" + renderers.Length);

                if (!productionUiReady || resourceArt == null)
                    Write("LOBBY_VISUAL_ERROR", "Responsive lobby canvas or its Resources background is unavailable.");

                return;
            }

            if (scene.Equals("Match", StringComparison.OrdinalIgnoreCase))
            {
                int unsafeMaterials = 0;
                foreach (Renderer renderer in renderers)
                {
                    if (renderer == null || renderer is SpriteRenderer || renderer is ParticleSystemRenderer) continue;
                    foreach (Material material in renderer.sharedMaterials)
                    {
                        if (material == null || material.shader == null || !material.shader.isSupported ||
                            material.shader.name.IndexOf("InternalErrorShader", StringComparison.OrdinalIgnoreCase) >= 0)
                            unsafeMaterials++;
                    }
                }
                Write(
                    "MATCH_VISUAL",
                    "camera=" + (main != null) +
                    " canvases=" + canvases.Length +
                    " renderers=" + renderers.Length +
                    " unsafeMaterials=" + unsafeMaterials);
            }
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
            catch
            {
                // Diagnostics must never interfere with gameplay.
            }
        }
    }
}
