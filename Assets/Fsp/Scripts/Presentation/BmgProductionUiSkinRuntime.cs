using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    /// <summary>
    /// Production BMG/FSP metallic UI skin. Keeps all existing button callbacks and layouts,
    /// but upgrades the presentation at runtime for Lobby + Match scenes.
    /// </summary>
    public sealed class BmgProductionUiSkinRuntime : MonoBehaviour
    {
        private static BmgProductionUiSkinRuntime instance;
        private static Sprite metalPanel;
        private static Sprite metalButton;
        private static Sprite orangeButton;
        private static Sprite roundMetal;
        private static Sprite roundOrange;

        private static readonly Color TextIvory = new(0.94f, 0.89f, 0.78f, 1f);
        private static readonly Color Gold = new(0.92f, 0.57f, 0.19f, 1f);
        private static readonly Color PressedOrange = new(0.88f, 0.26f, 0.015f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            var go = new GameObject("BMG_ProductionUiSkinRuntime");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<BmgProductionUiSkinRuntime>();
            SceneManager.sceneLoaded += instance.OnSceneLoaded;
            instance.StartCoroutine(instance.ApplyDelayed());
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StartCoroutine(ApplyDelayed());

        private IEnumerator ApplyDelayed()
        {
            yield return null;
            yield return null;
            yield return new WaitForSecondsRealtime(0.08f);
            EnsureSprites();
            ApplyAll();
            yield return new WaitForSecondsRealtime(0.35f);
            ApplyAll();
        }

        private static void EnsureSprites()
        {
            if (metalButton != null) return;
            metalPanel = BuildSprite("BMG_MetalPanel", 96, 96, 15, false, false);
            metalButton = BuildSprite("BMG_MetalButton", 96, 64, 13, false, true);
            orangeButton = BuildSprite("BMG_OrangeButton", 96, 64, 13, true, true);
            roundMetal = BuildSprite("BMG_RoundMetal", 96, 96, 46, false, true);
            roundOrange = BuildSprite("BMG_RoundOrange", 96, 96, 46, true, true);
        }

        private static Sprite BuildSprite(string name, int w, int h, int radius, bool orange, bool bevel)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true)
            {
                name = name + "Tex",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave
            };
            var pixels = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = Mathf.Max(radius - x, x - (w - 1 - radius));
                    float dy = Mathf.Max(radius - y, y - (h - 1 - radius));
                    bool corner = dx > 0f && dy > 0f && dx * dx + dy * dy > radius * radius;
                    if (corner) { pixels[y * w + x] = new Color32(0, 0, 0, 0); continue; }

                    int edge = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y));
                    float vertical = y / (float)(h - 1);
                    Color c;
                    if (orange)
                    {
                        c = Color.Lerp(new Color(0.34f, 0.075f, 0.008f, 1f), new Color(0.95f, 0.32f, 0.018f, 1f), 0.28f + vertical * 0.58f);
                        if (edge <= 2) c = new Color(1f, 0.66f, 0.16f, 1f);
                        else if (edge <= 5 && bevel) c *= 1.17f;
                    }
                    else
                    {
                        c = Color.Lerp(new Color(0.028f, 0.032f, 0.036f, 0.98f), new Color(0.11f, 0.105f, 0.09f, 0.98f), vertical);
                        if (edge <= 2) c = new Color(0.52f, 0.39f, 0.23f, 1f);
                        else if (edge <= 5 && bevel) c = new Color(0.20f, 0.18f, 0.145f, 1f);
                    }
                    pixels[y * w + x] = c;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            var border = new Vector4(radius + 3, radius + 3, radius + 3, radius + 3);
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect, border);
            sprite.name = name;
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        private static void ApplyAll()
        {
            foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                StyleButton(button);
            foreach (var text in FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                StyleText(text);
            foreach (var image in FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                StylePanel(image);
        }

        private static void StyleButton(Button button)
        {
            if (button == null) return;
            var image = button.targetGraphic as Image ?? button.GetComponent<Image>();
            if (image == null) return;

            string n = button.gameObject.name.ToLowerInvariant();
            var label = button.GetComponentInChildren<Text>(true);
            string t = label != null ? (label.text ?? string.Empty).Trim().ToLowerInvariant() : string.Empty;
            bool primary = ContainsAny(n, "start", "fire", "shoot", "confirm", "equip", "claim") ||
                           ContainsAny(t, "start", "ابدأ", "إطلاق", "اطلاق", "تجهيز", "موافق", "استلام");
            bool round = IsSquareish(button.GetComponent<RectTransform>()) &&
                         (ContainsAny(n, "fire", "aim", "jump", "reload", "crouch", "heal", "grenade", "swap", "use") ||
                          ContainsAny(t, "إطلاق", "اطلاق", "تصويب", "قفز", "إعادة", "علاج", "قنابل", "تبديل", "استخدم"));

            image.sprite = round ? (primary ? roundOrange : roundMetal) : (primary ? orangeButton : metalButton);
            image.type = Image.Type.Sliced;
            image.color = Color.white;

            var cb = button.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = primary ? new Color(1.12f, 1.05f, 0.92f, 1f) : new Color(1.12f, 1.10f, 1.04f, 1f);
            cb.pressedColor = primary ? PressedOrange : new Color(0.66f, 0.66f, 0.63f, 1f);
            cb.selectedColor = cb.highlightedColor;
            cb.disabledColor = new Color(0.28f, 0.28f, 0.28f, 0.72f);
            cb.colorMultiplier = 1f;
            cb.fadeDuration = .08f;
            button.colors = cb;

            AddShadow(image.gameObject, primary ? new Color(1f, .28f, .02f, .42f) : new Color(0f, 0f, 0f, .68f), primary ? new Vector2(0, -5) : new Vector2(2, -4));
            AddOutline(image.gameObject, primary ? new Color(1f, .64f, .16f, .78f) : new Color(.48f, .36f, .21f, .75f), new Vector2(1.2f, -1.2f));

            if (label != null)
            {
                label.color = primary ? Color.white : TextIvory;
                label.fontStyle = FontStyle.Bold;
                AddShadow(label.gameObject, new Color(0f, 0f, 0f, .86f), new Vector2(1.5f, -2f));
            }
        }

        private static void StyleText(Text text)
        {
            if (text == null || text.transform.parent == null) return;
            if (text.GetComponentInParent<Button>() != null) return;
            text.color = text.color.a < .2f ? text.color : TextIvory;
            if (text.fontSize >= 22) text.fontStyle = FontStyle.Bold;
            if (text.fontSize >= 18) AddShadow(text.gameObject, new Color(0, 0, 0, .70f), new Vector2(1.2f, -1.6f));
        }

        private static void StylePanel(Image image)
        {
            if (image == null || image.GetComponent<Button>() != null) return;
            string n = image.gameObject.name.ToLowerInvariant();
            if (ContainsAny(n, "background", "backdrop", "icon", "portrait", "avatar", "logo", "sprite", "swatch")) return;
            var rt = image.rectTransform;
            if (rt == null) return;
            if (image.sprite != null && image.color.a > .95f && image.color.r > .35f) return;
            if (image.color.a < .28f) return;

            if (ContainsAny(n, "panel", "modal", "profile", "wallet", "bar", "frame", "section"))
            {
                image.sprite = metalPanel;
                image.type = Image.Type.Sliced;
                image.color = Color.white;
                AddOutline(image.gameObject, new Color(.42f, .31f, .18f, .60f), new Vector2(1f, -1f));
            }
        }

        private static bool IsSquareish(RectTransform rt)
        {
            if (rt == null) return false;
            var r = rt.rect;
            float w = Mathf.Abs(r.width), h = Mathf.Abs(r.height);
            if (w < 18f || h < 18f) return false;
            return Mathf.Abs(w - h) / Mathf.Max(w, h) < .28f;
        }

        private static bool ContainsAny(string s, params string[] keys)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (var k in keys) if (s.Contains(k)) return true;
            return false;
        }

        private static void AddShadow(GameObject go, Color color, Vector2 distance)
        {
            var sh = go.GetComponent<Shadow>();
            if (sh == null) sh = go.AddComponent<Shadow>();
            sh.effectColor = color;
            sh.effectDistance = distance;
            sh.useGraphicAlpha = true;
        }

        private static void AddOutline(GameObject go, Color color, Vector2 distance)
        {
            var ol = go.GetComponent<Outline>();
            if (ol == null) ol = go.AddComponent<Outline>();
            ol.effectColor = color;
            ol.effectDistance = distance;
            ol.useGraphicAlpha = true;
        }
    }
}
