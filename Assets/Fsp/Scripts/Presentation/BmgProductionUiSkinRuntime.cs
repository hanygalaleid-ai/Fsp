using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    /// <summary>
    /// Production BMG metallic UI skin. Uses checked-in static BMG assets only; no runtime texture generation.
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
        private static readonly Color PressedOrange = new(0.88f, 0.26f, 0.015f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            GameObject go = new("BMG_ProductionUiSkinRuntime");
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
            yield return new WaitForSecondsRealtime(.08f);
            EnsureSprites();
            ApplyAll();
            yield return new WaitForSecondsRealtime(.35f);
            ApplyAll();
            yield return new WaitForSecondsRealtime(.90f);
            ApplyAll();
        }

        private static void EnsureSprites()
        {
            if (metalButton != null) return;
            metalPanel = LoadSprite("BMG/UI/bmg_ui_panel_static");
            metalButton = LoadSprite("BMG/UI/bmg_ui_button_static");
            orangeButton = LoadSprite("BMG/UI/bmg_ui_button_orange_static");
            roundMetal = LoadSprite("BMG/UI/bmg_ui_round_static");
            roundOrange = LoadSprite("BMG/UI/bmg_ui_round_orange_static");
            if (metalPanel == null || metalButton == null || orangeButton == null || roundMetal == null || roundOrange == null)
                Debug.LogError("BMG production UI static skin assets are missing or not imported as sprites.");
        }

        private static Sprite LoadSprite(string resourcePath)
        {
            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null) return sprite;
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null) return null;
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(12f, 12f, 12f, 12f));
        }

        private static void ApplyAll()
        {
            if (metalButton == null) return;
            foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)) StyleButton(button);
            foreach (Text text in FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None)) StyleText(text);
            foreach (Image image in FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None)) StylePanelOrAction(image);
        }

        private static void StyleButton(Button button)
        {
            if (button == null) return;
            Image image = button.targetGraphic as Image ?? button.GetComponent<Image>();
            if (image == null) return;
            string n = button.gameObject.name.ToLowerInvariant();
            Text label = button.GetComponentInChildren<Text>(true);
            string t = label != null ? (label.text ?? string.Empty).Trim().ToLowerInvariant() : string.Empty;
            bool primary = ContainsAny(n, "start", "fire", "shoot", "confirm", "equip", "claim") || ContainsAny(t, "start", "ابدأ", "إطلاق", "اطلاق", "تجهيز", "موافق", "استلام");
            bool round = IsSquareish(button.GetComponent<RectTransform>()) && IsCombatAction(n, t);
            ApplyButtonSurface(image, primary, round);
            ColorBlock cb = button.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = Color.white;
            cb.pressedColor = primary ? PressedOrange : new Color(.66f, .66f, .63f, 1f);
            cb.selectedColor = cb.highlightedColor;
            cb.disabledColor = new Color(.28f, .28f, .28f, .72f);
            cb.colorMultiplier = 1f;
            cb.fadeDuration = .08f;
            button.colors = cb;
            if (label != null)
            {
                label.color = primary ? Color.white : TextIvory;
                label.fontStyle = FontStyle.Bold;
                AddShadow(label.gameObject, new Color(0f, 0f, 0f, .86f), new Vector2(1.5f, -2f));
            }
        }

        private static void ApplyButtonSurface(Image image, bool primary, bool round)
        {
            Sprite sprite = round ? (primary ? roundOrange : roundMetal) : (primary ? orangeButton : metalButton);
            if (sprite == null) return;
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            AddShadow(image.gameObject, primary ? new Color(1f, .28f, .02f, .42f) : new Color(0f, 0f, 0f, .68f), primary ? new Vector2(0, -5) : new Vector2(2, -4));
            AddOutline(image.gameObject, primary ? new Color(1f, .64f, .16f, .78f) : new Color(.48f, .36f, .21f, .75f), new Vector2(1.2f, -1.2f));
        }

        private static void StyleText(Text text)
        {
            if (text == null || text.transform.parent == null || text.GetComponentInParent<Button>() != null) return;
            text.color = text.color.a < .2f ? text.color : TextIvory;
            if (text.fontSize >= 22) text.fontStyle = FontStyle.Bold;
            if (text.fontSize >= 18) AddShadow(text.gameObject, new Color(0f, 0f, 0f, .70f), new Vector2(1.2f, -1.6f));
        }

        private static void StylePanelOrAction(Image image)
        {
            if (image == null || image.GetComponent<Button>() != null) return;
            string n = image.gameObject.name.ToLowerInvariant();
            if (ContainsAny(n, "background", "backdrop", "icon", "portrait", "avatar", "logo", "sprite", "swatch")) return;
            RectTransform rt = image.rectTransform;
            if (rt == null) return;
            if (IsCombatAction(n, string.Empty))
            {
                bool primary = ContainsAny(n, "fire", "shoot");
                ApplyButtonSurface(image, primary, IsSquareish(rt));
                return;
            }
            if (image.color.a < .28f || metalPanel == null) return;
            if (ContainsAny(n, "panel", "modal", "profile", "wallet", "bar", "frame", "section", "topbadge"))
            {
                image.sprite = metalPanel;
                image.type = Image.Type.Sliced;
                image.color = Color.white;
                AddOutline(image.gameObject, new Color(.42f, .31f, .18f, .60f), new Vector2(1f, -1f));
            }
        }

        private static bool IsCombatAction(string name, string text) =>
            ContainsAny(name, "fire", "shoot", "aim", "jump", "reload", "crouch", "heal", "grenade", "swap", "switch", "interact", "use", "sprint") ||
            ContainsAny(text, "إطلاق", "اطلاق", "تصويب", "قفز", "إعادة", "علاج", "قنابل", "تبديل", "استخدم");

        private static bool IsSquareish(RectTransform rt)
        {
            if (rt == null) return false;
            Rect r = rt.rect;
            float w = Mathf.Abs(r.width), h = Mathf.Abs(r.height);
            return w >= 18f && h >= 18f && Mathf.Abs(w - h) / Mathf.Max(w, h) < .28f;
        }

        private static bool ContainsAny(string s, params string[] keys)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (string key in keys) if (s.Contains(key)) return true;
            return false;
        }

        private static void AddShadow(GameObject go, Color color, Vector2 distance)
        {
            Shadow shadow = go.GetComponent<Shadow>();
            if (shadow == null) shadow = go.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        private static void AddOutline(GameObject go, Color color, Vector2 distance)
        {
            Outline outline = go.GetComponent<Outline>();
            if (outline == null) outline = go.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }
    }
}
