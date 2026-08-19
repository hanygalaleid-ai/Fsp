using Fsp.BattleRoyale;
using Fsp.Presentation;
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
            if (!isMatchScene && Object.FindFirstObjectByType<MatchManager>() == null) return;
            if (Object.FindFirstObjectByType<MatchResultsController>() != null) return;

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasObject = new GameObject("ResultsCanvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            var host = new GameObject("MatchResultsController");
            var controller = host.AddComponent<MatchResultsController>();

            GameObject panel = new GameObject("ResultsPanel");
            panel.transform.SetParent(canvas.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(560f, 430f);
            var image = panel.AddComponent<Image>();
            image.color = FspFixedTheme.Panel;

            Text title = MakeText(panel.transform, "Title", "MATCH COMPLETE", 42, new Vector2(0f, 145f), FspFixedTheme.Text);
            Text placement = MakeText(panel.transform, "Placement", "PLACE #--", 34, new Vector2(0f, 72f), FspFixedTheme.Accent);
            Text kills = MakeText(panel.transform, "Kills", "KILLS 0", 24, new Vector2(0f, 12f), FspFixedTheme.Text);
            Text xp = MakeText(panel.transform, "Xp", "XP +0", 24, new Vector2(0f, -40f), FspFixedTheme.Bronze);
            Button returnButton = MakeButton(panel.transform, "ReturnToLobby", "RETURN TO LOBBY", new Vector2(0f, -145f));

            controller.Configure(panel, title, placement, kills, xp, returnButton);
        }

        private static Text MakeText(Transform parent, string name, string value, int size, Vector2 position, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(500f, 58f);
            Text text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = size;
            text.text = value;
            text.color = color;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 14;
            text.resizeTextMaxSize = size;
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
            rect.sizeDelta = new Vector2(330f, 72f);
            var image = go.AddComponent<Image>();
            image.color = FspFixedTheme.Accent;
            Button button = go.AddComponent<Button>();
            Text text = MakeText(go.transform, "Label", label, 20, Vector2.zero, FspFixedTheme.Text);
            RectTransform tr = text.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            return button;
        }
    }
}
