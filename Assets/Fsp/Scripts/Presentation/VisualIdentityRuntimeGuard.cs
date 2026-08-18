using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    /// <summary>
    /// Final visual safety net for generated scenes. It never creates gameplay; it only normalizes
    /// colours/fonts/camera presentation so fallback canvases keep the same shipped identity.
    /// </summary>
    public static class VisualIdentityRuntimeGuard
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ApplyAfterSceneLoad()
        {
            string scene = SceneManager.GetActiveScene().name;
            if (scene != "Lobby" && scene != "Match") return;

            foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                ApplyCanvas(canvas, scene == "Match");

            Camera main = Camera.main;
            if (main != null)
            {
                main.allowHDR = false;
                main.fieldOfView = scene == "Lobby" ? FspFixedTheme.LobbyFieldOfView : FspFixedTheme.MatchFieldOfView;
            }
        }

        private static void ApplyCanvas(Canvas canvas, bool match)
        {
            if (canvas == null) return;

            foreach (Text text in canvas.GetComponentsInChildren<Text>(true))
            {
                if (text == null) continue;
                string n = text.gameObject.name.ToLowerInvariant();
                text.color = n.Contains("sub") || n.Contains("status") || n.Contains("rank")
                    ? FspFixedTheme.MutedText
                    : FspFixedTheme.Text;
            }

            foreach (Image image in canvas.GetComponentsInChildren<Image>(true))
            {
                if (image == null || image.sprite != null) continue;
                string n = image.gameObject.name.ToLowerInvariant();

                if (n.Contains("start") || n.Contains("fire") || n.Contains("primary"))
                    image.color = FspFixedTheme.Accent;
                else if (n.Contains("danger") || n.Contains("warning"))
                    image.color = FspFixedTheme.Danger;
                else if (n.Contains("panel") || n.Contains("card") || n.Contains("button") || n.Contains("hud"))
                    image.color = FspFixedTheme.Panel;
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = match ? new Vector2(1920f, 1080f) : new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }
        }
    }
}
