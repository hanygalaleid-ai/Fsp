using Fsp.Input;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.UI
{
    /// <summary>Adds the missing AIM control and applies fixed icon art to all mobile combat actions.</summary>
    public sealed class MobileActionArtRuntime : MonoBehaviour
    {
        private static readonly string[] Names = { "Fire", "Aim", "Jump", "Reload", "Heal", "Interact", "Switch", "Sprint" };
        private Texture2D atlas;
        private float retryUntil;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", System.StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<MobileActionArtRuntime>() == null)
                new GameObject("MobileActionArtRuntime").AddComponent<MobileActionArtRuntime>();
        }

        private void Awake()
        {
            atlas = Resources.Load<Texture2D>("UI/action_icons");
            retryUntil = Time.unscaledTime + 12f;
        }

        private void Update()
        {
            GameObject hud = GameObject.Find("MobileCombatHUD");
            if (hud == null)
            {
                if (Time.unscaledTime > retryUntil) enabled = false;
                return;
            }

            EnsureAim(hud.transform);
            ApplyIcons();
            enabled = false;
        }

        private void EnsureAim(Transform hud)
        {
            if (GameObject.Find("Aim") != null) return;

            GameObject go = new GameObject("Aim", typeof(RectTransform), typeof(Image), typeof(MobileButtonAction));
            go.transform.SetParent(hud, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.69f, 0.32f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(112f, 112f);
            rt.anchoredPosition = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0.025f, 0.055f, 0.09f, 0.90f);
            go.GetComponent<MobileButtonAction>().Configure(MobileButtonActionType.Aim);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            RectTransform lr = labelGo.GetComponent<RectTransform>();
            lr.anchorMin = new Vector2(0f, 0f); lr.anchorMax = new Vector2(1f, 0.28f);
            lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;
            Text label = labelGo.GetComponent<Text>();
            label.font = font; label.text = "AIM"; label.fontSize = 14; label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.96f, 0.94f, 0.89f, 1f);
        }

        private void ApplyIcons()
        {
            if (atlas == null || atlas.width < 1024 || atlas.height < 512) return;
            for (int i = 0; i < Names.Length; i++)
            {
                GameObject button = GameObject.Find(Names[i]);
                if (button == null || button.transform.Find("ActionIcon") != null) continue;

                int column = i % 4;
                int row = i / 4;
                float y = row == 0 ? 256f : 0f;
                Sprite sprite = Sprite.Create(atlas, new Rect(column * 256f, y, 256f, 256f), new Vector2(0.5f, 0.5f), 256f, 0, SpriteMeshType.FullRect);
                sprite.name = Names[i] + "Icon";

                GameObject icon = new GameObject("ActionIcon", typeof(RectTransform), typeof(Image));
                icon.transform.SetParent(button.transform, false);
                RectTransform ir = icon.GetComponent<RectTransform>();
                ir.anchorMin = new Vector2(0.18f, 0.28f); ir.anchorMax = new Vector2(0.82f, 0.92f);
                ir.offsetMin = Vector2.zero; ir.offsetMax = Vector2.zero;
                Image image = icon.GetComponent<Image>();
                image.sprite = sprite; image.color = Color.white; image.preserveAspect = true; image.raycastTarget = false;

                Transform labelTransform = button.transform.Find("Label");
                if (labelTransform is RectTransform labelRect)
                {
                    labelRect.anchorMin = new Vector2(0f, 0f); labelRect.anchorMax = new Vector2(1f, 0.28f);
                    labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;
                }
            }
        }
    }
}
