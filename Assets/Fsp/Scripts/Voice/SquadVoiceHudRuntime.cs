using System;
using Fsp.Backend;
using Fsp.Lobby;
using Fsp.Localization;
using Fsp.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Voice
{
    public sealed class SquadVoiceHudRuntime : MonoBehaviour
    {
        private Text statusText;
        private Text buttonText;
        private Button talkButton;
        private SquadVoiceState boundState;

        private void Awake() => BuildUi();

        private void OnEnable()
        {
            EnsureStateBinding();
            Refresh();
        }

        private void OnDisable()
        {
            if (boundState != null) boundState.Changed -= Refresh;
            boundState = null;
            SquadVoiceState.Instance?.EndTalking();
        }

        private void Update()
        {
            EnsureStateBinding();
#if ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.GetKeyDown(KeyCode.V)) SquadVoiceState.Instance?.BeginTalking();
            if (UnityEngine.Input.GetKeyUp(KeyCode.V)) SquadVoiceState.Instance?.EndTalking();
#endif
        }

        private void EnsureStateBinding()
        {
            SquadVoiceState current = SquadVoiceState.Instance;
            if (boundState == current) return;
            if (boundState != null) boundState.Changed -= Refresh;
            boundState = current;
            if (boundState != null) boundState.Changed += Refresh;
            Refresh();
        }

        private void BuildUi()
        {
            GameObject canvasObject = GameObject.Find("SquadVoiceCanvas");
            Canvas canvas = canvasObject != null ? canvasObject.GetComponent<Canvas>() : null;
            if (canvas == null)
            {
                canvasObject = new GameObject("SquadVoiceCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MobileSafeArea));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.overrideSorting = true;
                canvas.sortingOrder = 95;
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            GameObject panel = new GameObject("SquadVoicePanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            // Sit below the top-right status card. Bottom-right is reserved for FIRE,
            // AIM and RELOAD on touch devices.
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(-36f, -220f);
            panelRect.sizeDelta = new Vector2(310f, 112f);
            panel.GetComponent<Image>().color = new Color(0.03f, 0.05f, 0.09f, 0.78f);

            GameObject statusObject = new GameObject("VoiceStatus", typeof(RectTransform), typeof(Text));
            statusObject.transform.SetParent(panel.transform, false);
            statusText = statusObject.GetComponent<Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 22;
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.color = Color.white;
            var statusRect = statusObject.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 0.60f);
            statusRect.anchorMax = new Vector2(1f, 1f);
            statusRect.offsetMin = new Vector2(8f, 0f);
            statusRect.offsetMax = new Vector2(-8f, -4f);

            GameObject buttonObject = new GameObject("PushToTalk", typeof(RectTransform), typeof(Image), typeof(Button), typeof(PushToTalkButton));
            buttonObject.transform.SetParent(panel.transform, false);
            var buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.06f, 0.08f);
            buttonRect.anchorMax = new Vector2(0.94f, 0.58f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            buttonObject.GetComponent<Image>().color = new Color(0.08f, 0.33f, 0.70f, 0.95f);
            talkButton = buttonObject.GetComponent<Button>();

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);
            buttonText = labelObject.GetComponent<Text>();
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 24;
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }

        private void Refresh()
        {
            SquadVoiceState state = SquadVoiceState.Instance;
            if (state == null)
            {
                if (statusText != null) statusText.text = FspLocalizationRuntime.T("VOICE OFFLINE");
                if (buttonText != null) buttonText.text = FspLocalizationRuntime.T("VOICE");
                if (talkButton != null) talkButton.interactable = false;
                return;
            }

            if (statusText != null)
                statusText.text = state.Connected
                    ? FspLocalizationRuntime.T(state.MicrophoneMuted ? "SQUAD VOICE • READY" : "SQUAD VOICE • TALKING")
                    : FspLocalizationRuntime.T(state.RuntimeStatus);

            if (buttonText != null)
                buttonText.text = FspLocalizationRuntime.T(state.MicrophoneMuted ? "HOLD TO TALK" : "TALKING...");

            if (talkButton != null)
                talkButton.interactable = state.Connected;
        }
    }

    public static class SquadVoiceHudInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (!SupabaseSession.IsSignedIn) return;
            if (SquadLobbyState.Instance == null || !SquadLobbyState.Instance.HasSquad) return;
            if (UnityEngine.Object.FindFirstObjectByType<SquadVoiceHudRuntime>() != null) return;
            new GameObject("SquadVoiceHudRuntime").AddComponent<SquadVoiceHudRuntime>();
        }
    }
}
