using Fsp.BattleRoyale;
using Fsp.Input;
using Fsp.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.UI
{
    /// <summary>
    /// Runtime-safe Android controls. It is generated in code so cloud-created Match scenes are playable
    /// even when no serialized mobile HUD prefab is present.
    /// </summary>
    public static class MobileMatchControlsInstaller
    {
        private static readonly Color Panel = new Color(0.025f, 0.055f, 0.09f, 0.72f);
        private static readonly Color PanelStrong = new Color(0.025f, 0.055f, 0.09f, 0.90f);
        private static readonly Color Accent = new Color(0.90f, 0.43f, 0.05f, 0.96f);
        private static readonly Color TextColor = new Color(0.96f, 0.94f, 0.89f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallAfterSceneLoad()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", System.StringComparison.OrdinalIgnoreCase)) return;
            Install();
        }

        public static void Install()
        {
            EnsureEventSystem();
            EnsureBridge();

            MatchParticipant local = FindLocalParticipant();
            if (local == null) return;

            StarterThirdPersonRig desktopRig = local.GetComponent<StarterThirdPersonRig>();
            if (desktopRig != null) desktopRig.enabled = false;

            MobileGameplayAdapter adapter = local.GetComponent<MobileGameplayAdapter>();
            if (adapter == null) adapter = local.gameObject.AddComponent<MobileGameplayAdapter>();
            adapter.enabled = true;

            if (GameObject.Find("MobileCombatHUD") != null) return;

            GameObject canvasObject = new GameObject("MobileCombatHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MobileSafeArea));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform root = canvasObject.GetComponent<RectTransform>();

            CreateLookArea(root);
            CreateJoystick(root);
            CreateActionButton(root, "Fire", "FIRE", new Vector2(0.865f, 0.12f), new Vector2(150f, 150f), Accent, MobileButtonActionType.Fire, 28);
            CreateActionButton(root, "Jump", "JUMP", new Vector2(0.76f, 0.20f), new Vector2(118f, 118f), PanelStrong, MobileButtonActionType.Jump, 20);
            CreateActionButton(root, "Reload", "RELOAD", new Vector2(0.88f, 0.31f), new Vector2(118f, 78f), PanelStrong, MobileButtonActionType.Reload, 17);
            CreateActionButton(root, "Heal", "HEAL", new Vector2(0.73f, 0.08f), new Vector2(112f, 76f), PanelStrong, MobileButtonActionType.Heal, 18);
            CreateActionButton(root, "Interact", "USE", new Vector2(0.62f, 0.19f), new Vector2(110f, 78f), PanelStrong, MobileButtonActionType.Interact, 18);
            CreateActionButton(root, "Switch", "SWAP", new Vector2(0.89f, 0.41f), new Vector2(110f, 70f), PanelStrong, MobileButtonActionType.SwitchWeapon, 16);
            CreateActionButton(root, "Sprint", "SPRINT", new Vector2(0.18f, 0.10f), new Vector2(118f, 72f), PanelStrong, MobileButtonActionType.Sprint, 16);

            CreateTopBadge(root, "FSP // SUNSCAR", new Vector2(0.5f, 0.955f));
        }

        private static void EnsureBridge()
        {
            if (MobileInputBridge.Instance != null) return;
            new GameObject("MobileInputBridge").AddComponent<MobileInputBridge>();
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            GameObject go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Object.DontDestroyOnLoad(go);
        }

        private static MatchParticipant FindLocalParticipant()
        {
            MatchParticipant[] participants = Object.FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None);
            foreach (MatchParticipant participant in participants)
                if (participant != null && participant.IsLocalPlayer) return participant;
            return participants.Length > 0 ? participants[0] : null;
        }

        private static void CreateLookArea(RectTransform root)
        {
            GameObject look = new GameObject("LookArea", typeof(RectTransform), typeof(Image), typeof(LookDragArea));
            look.transform.SetParent(root, false);
            RectTransform rt = look.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.42f, 0.10f);
            rt.anchorMax = new Vector2(0.99f, 0.92f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image image = look.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.001f);
        }

        private static void CreateJoystick(RectTransform root)
        {
            GameObject baseObject = new GameObject("MoveJoystick", typeof(RectTransform), typeof(Image), typeof(VirtualJoystick));
            baseObject.transform.SetParent(root, false);
            RectTransform rt = baseObject.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.12f, 0.20f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(250f, 250f);
            rt.anchoredPosition = Vector2.zero;
            baseObject.GetComponent<Image>().color = Panel;

            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(baseObject.transform, false);
            RectTransform handleRt = handle.GetComponent<RectTransform>();
            handleRt.anchorMin = handleRt.anchorMax = new Vector2(0.5f, 0.5f);
            handleRt.pivot = new Vector2(0.5f, 0.5f);
            handleRt.sizeDelta = new Vector2(105f, 105f);
            handleRt.anchoredPosition = Vector2.zero;
            handle.GetComponent<Image>().color = new Color(0.88f, 0.89f, 0.87f, 0.74f);
        }

        private static void CreateActionButton(RectTransform root, string name, string label, Vector2 anchor, Vector2 size, Color color, MobileButtonActionType action, int fontSize)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(MobileButtonAction));
            go.transform.SetParent(root, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
            go.GetComponent<Image>().color = color;
            go.GetComponent<MobileButtonAction>().Configure(action);
            CreateLabel(go.transform, label, fontSize);
        }

        private static void CreateTopBadge(RectTransform root, string label, Vector2 anchor)
        {
            GameObject go = new GameObject("TopBadge", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(root, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(330f, 58f);
            go.GetComponent<Image>().color = Panel;
            CreateLabel(go.transform, label, 18);
        }

        private static void CreateLabel(Transform parent, string value, int size)
        {
            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Text text = labelObject.GetComponent<Text>();
            Font font = null;
            try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = TextColor;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = size;
        }
    }
}
