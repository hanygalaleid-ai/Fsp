using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    /// <summary>
    /// Applies fixed UI art from Resources to runtime-generated Lobby/Match canvases.
    /// Lobby only needs a short bootstrap scan; Match stays active so late-created/late-enabled
    /// UI such as the results screen always receives the shipped visual identity.
    /// </summary>
    public sealed class FixedUiArtRuntime : MonoBehaviour
    {
        private static readonly Dictionary<string, Sprite> Cache = new();
        private float nextApply;
        private float lobbyStopAt;
        private bool isMatch;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            string scene = SceneManager.GetActiveScene().name;
            if (!string.Equals(scene, "Lobby", System.StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(scene, "Match", System.StringComparison.OrdinalIgnoreCase)) return;

            if (FindFirstObjectByType<FixedUiArtRuntime>() == null)
                new GameObject("Fsp_FixedUiArtRuntime").AddComponent<FixedUiArtRuntime>();
        }

        private void Awake()
        {
            isMatch = string.Equals(SceneManager.GetActiveScene().name, "Match", System.StringComparison.OrdinalIgnoreCase);
            lobbyStopAt = Time.unscaledTime + 12f;
            ApplyAll();
        }

        private void Update()
        {
            if (!isMatch && Time.unscaledTime > lobbyStopAt)
            {
                enabled = false;
                return;
            }

            if (Time.unscaledTime < nextApply) return;
            nextApply = Time.unscaledTime + (isMatch ? 1f : 0.35f);
            ApplyAll();
        }

        private static void ApplyAll()
        {
            string scene = SceneManager.GetActiveScene().name;
            if (string.Equals(scene, "Lobby", System.StringComparison.OrdinalIgnoreCase)) ApplyLobby();
            else if (string.Equals(scene, "Match", System.StringComparison.OrdinalIgnoreCase)) ApplyMatch();
        }

        private static void ApplyLobby()
        {
            ApplyNamed("Start", "UI/ui_button_primary", Image.Type.Sliced);
            ApplyNamed("Solo", "UI/ui_button_secondary", Image.Type.Sliced);
            ApplyNamed("Squad", "UI/ui_button_secondary", Image.Type.Sliced);
            ApplyNamed("Loadout", "UI/ui_button_secondary", Image.Type.Sliced);
            ApplyNamed("Appearance", "UI/ui_button_secondary", Image.Type.Sliced);
            ApplyNamed("Career", "UI/ui_button_secondary", Image.Type.Sliced);

            GameObject canvas = GameObject.Find("LobbyCanvas");
            if (canvas == null) return;
            foreach (Image image in canvas.GetComponentsInChildren<Image>(true))
            {
                if (image == null || image.sprite != null) continue;
                string n = image.gameObject.name;
                if (n.Contains("Panel") || n.Contains("Card") || n.Contains("Profile") || n.Contains("Squad"))
                    SetSprite(image, "UI/ui_panel_dark", Image.Type.Sliced);
            }
        }

        private static void ApplyMatch()
        {
            ApplyNamed("MoveJoystick", "UI/joystick_base", Image.Type.Simple);
            ApplyNamed("Fire", "UI/ui_button_primary", Image.Type.Sliced);
            ApplyNamed("Jump", "UI/ui_button_secondary", Image.Type.Sliced);
            ApplyNamed("Reload", "UI/ui_button_secondary", Image.Type.Sliced);
            ApplyNamed("Heal", "UI/ui_button_secondary", Image.Type.Sliced);
            ApplyNamed("Interact", "UI/ui_button_secondary", Image.Type.Sliced);
            ApplyNamed("Switch", "UI/ui_button_secondary", Image.Type.Sliced);
            ApplyNamed("Sprint", "UI/ui_button_secondary", Image.Type.Sliced);
            ApplyNamed("TopBadge", "UI/ui_panel_dark", Image.Type.Sliced);
            ApplyNamed("ResultsPanel", "UI/ui_panel_dark", Image.Type.Sliced);
            ApplyNamed("ReturnToLobby", "UI/ui_button_primary", Image.Type.Sliced);
        }

        private static void ApplyNamed(string objectName, string resourcePath, Image.Type type)
        {
            GameObject go = GameObject.Find(objectName);
            if (go == null) return;
            Image image = go.GetComponent<Image>();
            if (image == null) return;
            SetSprite(image, resourcePath, type);
        }

        private static void SetSprite(Image image, string resourcePath, Image.Type type)
        {
            Sprite sprite = LoadSprite(resourcePath);
            if (sprite == null) return;
            image.sprite = sprite;
            image.type = type;
            image.preserveAspect = type == Image.Type.Simple;
            image.color = Color.white;
        }

        private static Sprite LoadSprite(string resourcePath)
        {
            if (Cache.TryGetValue(resourcePath, out Sprite cached) && cached != null) return cached;
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null) return null;
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.name = resourcePath.Replace('/', '_') + "_RuntimeSprite";
            Cache[resourcePath] = sprite;
            return sprite;
        }
    }
}
