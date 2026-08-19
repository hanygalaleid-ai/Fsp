using System;
using Fsp.Backend;
using Fsp.Lobby;
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

        private void Awake()
        {
            BuildUi();
        }

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
            if (Input.GetKeyDown(KeyCode.V)) SquadVoiceState.Instance?.BeginTalking();
            if (Input.GetKeyUp(KeyCode.V)) SquadVoiceState.Instance?.EndTalking();
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
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("FspRuntimeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }

            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            GameObject panel = new GameObject("SquadVoicePanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(1f, 0f);
            panelRect.anchoredPosition = new Vector2(-36f, 38f);
            panelRect.sizeDelta = new Vector2(310f, 128f);
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
                if (statusText != null) statusText.text = "VOICE OFFLINE";
                if (buttonText != null) buttonText.text = "VOICE";
                if (talkButton != null) talkButton.interactable = false;
                return;
            }

            if (statusText != null)
                statusText.text = state.Connected ? (state.MicrophoneMuted ? "SQUAD VOICE • READY" : "SQUAD VOICE • TALKING") : state.RuntimeStatus;

            if (buttonText != null)
                buttonText.text = state.MicrophoneMuted ? "HOLD TO TALK" : "TALKING...";

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
