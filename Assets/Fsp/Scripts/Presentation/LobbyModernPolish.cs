using Fsp.Lobby;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    /// <summary>
    /// Keeps the generated lobby on the intended modern military presentation even when the scene is rebuilt in CI.
    /// Checked-in art is the primary background; procedural elements remain only as a fallback.
    /// </summary>
    public static class LobbyModernPolish
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", System.StringComparison.OrdinalIgnoreCase)) return;

            bool fixedBackgroundReady = EnsureFixedBackground();

            GameObject hero = GameObject.Find("LobbyHero_Procedural");
            if (hero != null && hero.GetComponent<StarterProceduralCharacterVisual>() == null)
            {
                foreach (Renderer renderer in hero.GetComponentsInChildren<Renderer>(true))
                    renderer.enabled = false;
                hero.AddComponent<StarterProceduralCharacterVisual>();
                hero.transform.localScale = Vector3.one * 1.15f;
            }

            if (fixedBackgroundReady) HideProceduralBackdrop();
            EnsureLobbyLights(hero != null ? hero.transform : null);
            PolishCanvas();
            PolishCamera();

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.20f, 0.20f, 0.18f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.18f, 0.19f, 0.20f);
            RenderSettings.fogDensity = 0.0045f;
        }

        private static bool EnsureFixedBackground()
        {
            GameObject existing = GameObject.Find("Fsp_FixedLobbyArt");
            if (existing != null)
            {
                Canvas existingCanvas = existing.GetComponent<Canvas>();
                if (existingCanvas != null) existingCanvas.sortingOrder = 0;
                return true;
            }

            Texture2D texture = Resources.Load<Texture2D>("Lobby/lobby_reference");
            if (texture == null)
            {
                Debug.LogError("FSP fixed lobby art missing at Resources/Lobby/lobby_reference. Procedural fallback kept visible.");
                return false;
            }

            GameObject canvasObject = new GameObject("Fsp_FixedLobbyArt", typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject background = new GameObject("LobbyArt", typeof(RectTransform), typeof(RawImage));
            background.transform.SetParent(canvasObject.transform, false);
            RectTransform rect = background.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            RawImage raw = background.GetComponent<RawImage>();
            raw.texture = texture;
            raw.color = Color.white;
            raw.raycastTarget = false;

            GameObject shade = new GameObject("ReadabilityShade", typeof(RectTransform), typeof(Image));
            shade.transform.SetParent(canvasObject.transform, false);
            RectTransform shadeRect = shade.GetComponent<RectTransform>();
            shadeRect.anchorMin = Vector2.zero;
            shadeRect.anchorMax = Vector2.one;
            shadeRect.offsetMin = Vector2.zero;
            shadeRect.offsetMax = Vector2.zero;
            Image shadeImage = shade.GetComponent<Image>();
            shadeImage.color = new Color(0.015f, 0.025f, 0.04f, 0.24f);
            shadeImage.raycastTarget = false;

            return true;
        }

        private static void HideProceduralBackdrop()
        {
            // Names from every previous starter/bootstrap version that must never show over shipped lobby art.
            string[] obsoleteBackdrop = { "FortSilhouette", "CampLeft", "CampRight", "Antenna", "CharacterStage" };
            foreach (string objectName in obsoleteBackdrop)
            {
                GameObject go = GameObject.Find(objectName);
                if (go == null) continue;
                foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>(true))
                    renderer.enabled = false;
            }
        }

        private static void EnsureLobbyLights(Transform hero)
        {
            if (GameObject.Find("LobbyKeyLight") == null)
            {
                GameObject key = new GameObject("LobbyKeyLight", typeof(Light));
                Light light = key.GetComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1f, 0.78f, 0.57f);
                light.intensity = 0.9f;
                light.shadows = LightShadows.Soft;
                key.transform.rotation = Quaternion.Euler(42f, -32f, 0f);
            }

            if (hero != null && GameObject.Find("LobbyRimLight") == null)
            {
                GameObject rim = new GameObject("LobbyRimLight", typeof(Light));
                Light light = rim.GetComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(0.95f, 0.42f, 0.14f);
                light.range = 9f;
                light.intensity = 1.6f;
                rim.transform.position = hero.position + new Vector3(1.8f, 2.4f, 1.8f);
            }
        }

        private static void PolishCanvas()
        {
            GameObject canvasObject = GameObject.Find("LobbyCanvas");
            if (canvasObject == null) return;

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = 20;
            }

            foreach (Text text in canvasObject.GetComponentsInChildren<Text>(true))
            {
                if (text == null) continue;
                text.color = text.name.Contains("Rank") || text.name.Contains("Status") || text.name.Contains("MapSub")
                    ? new Color(0.73f, 0.69f, 0.61f, 1f)
                    : new Color(0.96f, 0.94f, 0.89f, 1f);
            }

            foreach (Image image in canvasObject.GetComponentsInChildren<Image>(true))
            {
                if (image == null) continue;
                if (image.gameObject.name == "Start")
                    image.color = new Color(0.90f, 0.42f, 0.04f, 0.98f);
                else if (image.color.a > 0.5f && image.color.r < 0.2f)
                    image.color = new Color(0.025f, 0.050f, 0.082f, Mathf.Clamp(image.color.a, 0.76f, 0.94f));
            }
        }

        private static void PolishCamera()
        {
            Camera camera = Camera.main;
            if (camera == null) return;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.045f, 0.055f, 1f);
            camera.fieldOfView = 58f;
            camera.allowHDR = false;
        }
    }
}
