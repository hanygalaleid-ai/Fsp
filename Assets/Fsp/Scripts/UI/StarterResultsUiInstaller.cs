using Fsp.BattleRoyale;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.UI
{
    public static class StarterResultsUiInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            bool isMatchScene = SceneManager.GetActiveScene().name == "Match";
            if (!isMatchScene && Object.FindObjectOfType<MatchManager>() == null) return;
            if (Object.FindObjectOfType<MatchResultsController>() != null) return;

            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasObject = new GameObject("ResultsCanvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            var host = new GameObject("MatchResultsController");
            var controller = host.AddComponent<MatchResultsController>();

            GameObject panel = new GameObject("ResultsPanel");
            panel.transform.SetParent(canvas.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(430f, 360f);
            var image = panel.AddComponent<Image>();
            image.color = new Color(0.035f, 0.07f, 0.11f, 0.96f);

            Text title = MakeText(panel.transform, "Title", "MATCH COMPLETE", 34, new Vector2(0f, 120f));
            Text placement = MakeText(panel.transform, "Placement", "PLACE #--", 28, new Vector2(0f, 55f));
            Text kills = MakeText(panel.transform, "Kills", "KILLS 0", 22, new Vector2(0f, 5f));
            Text xp = MakeText(panel.transform, "Xp", "XP +0", 22, new Vector2(0f, -40f));
            Button returnButton = MakeButton(panel.transform, "ReturnToLobby", "RETURN TO LOBBY", new Vector2(0f, -120f));

            controller.Configure(panel, title, placement, kills, xp, returnButton);
        }

        private static Text MakeText(Transform parent, string name, string value, int size, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(380f, 48f);
            Text text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = size;
            text.text = value;
            text.color = new Color(0.94f, 0.92f, 0.86f, 1f);
            return text;
        }

        private static Button MakeButton(Transform parent, string name, string label, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(250f, 58f);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.72f, 0.47f, 0.22f, 1f);
            Button button = go.AddComponent<Button>();
            Text text = MakeText(go.transform, "Label", label, 18, Vector2.zero);
            RectTransform tr = text.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            return button;
        }
    }
}
