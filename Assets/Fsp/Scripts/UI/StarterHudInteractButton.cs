using Fsp.BattleRoyale;
using Fsp.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Fsp.UI
{
    /// <summary>
    /// Adds one context-sensitive interaction button to the generated starter HUD.
    /// The action becomes Jump / Open Parachute / Enter-Exit Vehicle depending on state.
    /// </summary>
    public sealed class StarterHudInteractButton : MonoBehaviour
    {
        private StarterInteractInput interact;
        private Button button;
        private Text label;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            Canvas canvas = GameObject.Find("FallbackHUD")?.GetComponent<Canvas>();
            if (canvas == null) return;

            MatchParticipant local = null;
            foreach (MatchParticipant participant in Object.FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
            {
                if (participant != null && participant.IsLocalPlayer)
                {
                    local = participant;
                    break;
                }
            }
            if (local == null) return;

            StarterInteractInput interact = local.GetComponent<StarterInteractInput>();
            if (interact == null) interact = local.gameObject.AddComponent<StarterInteractInput>();

            var go = new GameObject("Interact");
            go.transform.SetParent(canvas.transform, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-18f, 140f);
            rect.sizeDelta = new Vector2(132f, 58f);

            Image image = go.AddComponent<Image>();
            image.color = new Color(0.72f, 0.47f, 0.22f, 0.92f);
            Button button = go.AddComponent<Button>();
            go.AddComponent<Fsp.Audio.FspUiClickAudio>();

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            RectTransform textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            Text text = textGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 17;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.94f, 0.92f, 0.86f, 1f);
            text.text = "INTERACT";

            var behaviour = go.AddComponent<StarterHudInteractButton>();
            behaviour.interact = interact;
            behaviour.button = button;
            behaviour.label = text;
            button.onClick.AddListener(interact.Interact);
        }

        private void Update()
        {
            if (interact == null || button == null) return;
            bool available = interact.IsAvailable();
            button.interactable = available;
            if (label != null) label.text = ResolveLabel();
        }

        private string ResolveLabel()
        {
            if (interact == null) return "INTERACT";
            DropPlanePassenger passenger = interact.GetComponent<DropPlanePassenger>();
            if (passenger != null && passenger.IsAboard) return "JUMP";
            ParachuteController parachute = interact.GetComponent<ParachuteController>();
            if (parachute != null && parachute.IsActive && !parachute.IsOpen) return "PARACHUTE";
            return "INTERACT";
        }
    }
}
