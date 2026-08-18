using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Lobby
{
    /// <summary>
    /// Runtime safety net for cloud-generated Lobby scenes.
    /// Ensures the real lobby installer runs even if its serialized scene component was stripped/missed,
    /// and leaves a visible diagnostic UI instead of a blank primitive scene if installer startup fails.
    /// </summary>
    public static class LobbyRuntimeGuard
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureLobbyRuntime()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Lobby", StringComparison.OrdinalIgnoreCase))
                return;

            EnsureState();
            EnsureRuntimeComponents();

            if (GameObject.Find("LobbyCanvas") != null)
                return;

            StarterLobbyUiInstaller existing = UnityEngine.Object.FindFirstObjectByType<StarterLobbyUiInstaller>();
            try
            {
                if (existing == null)
                {
                    GameObject host = GameObject.Find("LobbyRuntime");
                    if (host == null)
                        host = new GameObject("LobbyRuntime");
                    host.AddComponent<StarterLobbyUiInstaller>();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            if (GameObject.Find("LobbyCanvas") == null)
                BuildFallbackUi();
        }

        private static GameObject GetRuntimeHost()
        {
            GameObject host = GameObject.Find("LobbyRuntime");
            return host != null ? host : new GameObject("LobbyRuntime");
        }

        private static void EnsureRuntimeComponents()
        {
            GameObject host = GetRuntimeHost();
            if (host.GetComponent<LobbyController>() == null)
                host.AddComponent<LobbyController>();
            if (host.GetComponent<LobbyMatchLauncher>() == null)
                host.AddComponent<LobbyMatchLauncher>();
        }

        private static void EnsureState()
        {
            if (LobbyState.Instance != null)
                return;

            GameObject stateObject = GameObject.Find("LobbyState");
            if (stateObject == null)
                stateObject = new GameObject("LobbyState");
            if (stateObject.GetComponent<LobbyState>() == null)
                stateObject.AddComponent<LobbyState>();
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
                return;

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static void BuildFallbackUi()
        {
            EnsureEventSystem();

            GameObject canvasObject = new GameObject("LobbyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject background = new GameObject("RuntimeGuardBackground", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(canvasObject.transform, false);
            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            background.GetComponent<Image>().color = new Color(0.025f, 0.055f, 0.10f, 0.96f);

            Font font = null;
            try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            if (font == null)
            {
                try { font = Font.CreateDynamicFontFromOSFont(new[] { "Roboto", "Arial", "sans-serif" }, 32); } catch { }
            }

            CreateLabel(background.transform, font, "FSP", new Vector2(0.08f, 0.68f), new Vector2(0.92f, 0.88f), 72, TextAnchor.MiddleCenter);
            CreateLabel(background.transform, font, "LOBBY READY", new Vector2(0.08f, 0.50f), new Vector2(0.92f, 0.67f), 38, TextAnchor.MiddleCenter);
            CreateLabel(background.transform, font, "Runtime UI recovery active", new Vector2(0.08f, 0.40f), new Vector2(0.92f, 0.50f), 24, TextAnchor.MiddleCenter);

            GameObject start = new GameObject("Start", typeof(RectTransform), typeof(Image), typeof(Button));
            start.transform.SetParent(background.transform, false);
            RectTransform startRect = start.GetComponent<RectTransform>();
            startRect.anchorMin = new Vector2(0.36f, 0.16f);
            startRect.anchorMax = new Vector2(0.64f, 0.30f);
            startRect.offsetMin = Vector2.zero;
            startRect.offsetMax = Vector2.zero;
            start.GetComponent<Image>().color = new Color(0.78f, 0.45f, 0.17f, 1f);
            CreateLabel(start.transform, font, "START", Vector2.zero, Vector2.one, 34, TextAnchor.MiddleCenter);
            start.GetComponent<Button>().onClick.AddListener(() =>
            {
                LobbyState state = LobbyState.Instance;
                if (state != null)
                    state.RequestStartMatch();
            });

            Debug.LogError("Fsp LobbyRuntimeGuard created fallback UI because StarterLobbyUiInstaller did not produce LobbyCanvas.");
        }

        private static void CreateLabel(Transform parent, Font font, string value, Vector2 min, Vector2 max, int size, TextAnchor alignment)
        {
            GameObject labelObject = new GameObject(value, typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Text text = labelObject.GetComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = new Color(0.96f, 0.93f, 0.86f, 1f);
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 14;
            text.resizeTextMaxSize = size;
        }
    }
}
