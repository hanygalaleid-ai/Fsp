using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Lobby
{
    /// <summary>
    /// Runtime safety net for cloud-generated Lobby scenes. Recovery uses the same shipped art/theme
    /// as the normal lobby so a failed installer never drops the player into a diagnostic-looking prototype.
    /// </summary>
    public static class LobbyRuntimeGuard
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureLobbyRuntime()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Lobby", StringComparison.OrdinalIgnoreCase)) return;

            EnsureState();
            EnsureRuntimeComponents();
            if (GameObject.Find("LobbyCanvas") != null) return;

            StarterLobbyUiInstaller existing = UnityEngine.Object.FindFirstObjectByType<StarterLobbyUiInstaller>();
            try
            {
                if (existing == null)
                {
                    GameObject host = GetRuntimeHost();
                    host.AddComponent<StarterLobbyUiInstaller>();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            if (GameObject.Find("LobbyCanvas") == null) BuildFallbackUi();
        }

        private static GameObject GetRuntimeHost()
        {
            GameObject host = GameObject.Find("LobbyRuntime");
            return host != null ? host : new GameObject("LobbyRuntime");
        }

        private static void EnsureRuntimeComponents()
        {
            GameObject host = GetRuntimeHost();
            if (host.GetComponent<LobbyController>() == null) host.AddComponent<LobbyController>();
            if (host.GetComponent<LobbyMatchLauncher>() == null) host.AddComponent<LobbyMatchLauncher>();
        }

        private static void EnsureState()
        {
            if (LobbyState.Instance != null) return;
            GameObject stateObject = GameObject.Find("LobbyState") ?? new GameObject("LobbyState");
            if (stateObject.GetComponent<LobbyState>() == null) stateObject.AddComponent<LobbyState>();
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return;
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
            canvas.sortingOrder = 20;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject background = new GameObject("LobbyBackdrop", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(canvasObject.transform, false);
            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = background.GetComponent<Image>();
            Texture2D backdrop = Resources.Load<Texture2D>("Lobby/lobby_reference");
            if (backdrop != null)
            {
                bgImage.sprite = Sprite.Create(backdrop, new Rect(0, 0, backdrop.width, backdrop.height), new Vector2(0.5f, 0.5f), 100f);
                bgImage.preserveAspect = false;
                bgImage.color = Color.white;
            }
            else bgImage.color = Fsp.Presentation.FspFixedTheme.Background;

            GameObject shade = new GameObject("RecoveryShade", typeof(RectTransform), typeof(Image));
            shade.transform.SetParent(background.transform, false);
            RectTransform shadeRect = shade.GetComponent<RectTransform>();
            shadeRect.anchorMin = Vector2.zero;
            shadeRect.anchorMax = Vector2.one;
            shadeRect.offsetMin = Vector2.zero;
            shadeRect.offsetMax = Vector2.zero;
            shade.GetComponent<Image>().color = new Color(0.01f, 0.02f, 0.03f, 0.38f);

            Font font = null;
            try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            CreateLabel(shade.transform, font, "FSP // OPERATIVE", new Vector2(0.05f, 0.76f), new Vector2(0.48f, 0.90f), 50, TextAnchor.MiddleLeft);
            CreateLabel(shade.transform, font, "SUNSCAR ISLAND", new Vector2(0.05f, 0.67f), new Vector2(0.48f, 0.76f), 25, TextAnchor.MiddleLeft);

            GameObject panel = new GameObject("RecoveryPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(shade.transform, false);
            RectTransform pr = panel.GetComponent<RectTransform>();
            pr.anchorMin = new Vector2(0.68f, 0.15f);
            pr.anchorMax = new Vector2(0.96f, 0.48f);
            pr.offsetMin = Vector2.zero;
            pr.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = Fsp.Presentation.FspFixedTheme.Panel;
            CreateLabel(panel.transform, font, "BATTLE ROYALE", new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.88f), 27, TextAnchor.MiddleCenter);
            CreateLabel(panel.transform, font, "SOLO READY", new Vector2(0.08f, 0.48f), new Vector2(0.92f, 0.65f), 20, TextAnchor.MiddleCenter);

            GameObject start = new GameObject("Start", typeof(RectTransform), typeof(Image), typeof(Button));
            start.transform.SetParent(panel.transform, false);
            RectTransform startRect = start.GetComponent<RectTransform>();
            startRect.anchorMin = new Vector2(0.10f, 0.10f);
            startRect.anchorMax = new Vector2(0.90f, 0.38f);
            startRect.offsetMin = Vector2.zero;
            startRect.offsetMax = Vector2.zero;
            start.GetComponent<Image>().color = Fsp.Presentation.FspFixedTheme.Accent;
            CreateLabel(start.transform, font, "START", Vector2.zero, Vector2.one, 30, TextAnchor.MiddleCenter);
            start.GetComponent<Button>().onClick.AddListener(() => LobbyState.Instance?.RequestStartMatch());

            Debug.LogWarning("Fsp LobbyRuntimeGuard recovery UI activated with fixed shipped art.");
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
            text.color = Fsp.Presentation.FspFixedTheme.Text;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 13;
            text.resizeTextMaxSize = size;
        }
    }
}
