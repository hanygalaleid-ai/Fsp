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
    /// Runtime-safe Android controls using the approved BMG 3D combat UI only.
    /// </summary>
    public static class MobileMatchControlsInstaller
    {
        private static readonly Color Panel = new Color(0.025f, 0.055f, 0.09f, 0.72f);
        private static readonly Color PanelStrong = new Color(0.025f, 0.055f, 0.09f, 0.90f);
        private static readonly Color Accent = new Color(0.90f, 0.43f, 0.05f, 0.96f);
        private static readonly Color TextColor = new Color(0.96f, 0.94f, 0.89f, 1f);
        private static Texture2D actionAtlas;
        private static Texture2D joystickTexture;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallAfterSceneLoad()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", System.StringComparison.OrdinalIgnoreCase)) return;
            if (Object.FindFirstObjectByType<MobileControlsInstallRetry>() == null)
                new GameObject("BMG_MobileControlsInstallRetry").AddComponent<MobileControlsInstallRetry>();
        }

        public static bool Install()
        {
            EnsureEventSystem();
            EnsureBridge();

            MatchParticipant local = FindLocalParticipant();
            if (local == null) return false;

            StarterThirdPersonRig desktopRig = local.GetComponent<StarterThirdPersonRig>();
            if (desktopRig != null) desktopRig.enabled = false;

            MobileGameplayAdapter adapter = local.GetComponent<MobileGameplayAdapter>();
            if (adapter == null) adapter = local.gameObject.AddComponent<MobileGameplayAdapter>();
            adapter.enabled = true;

            if (GameObject.Find("MobileCombatHUD") != null) return true;

            GameObject canvasObject = new GameObject("MobileCombatHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MobileSafeArea));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform root = canvasObject.GetComponent<RectTransform>();

            CreateLookArea(root);
            CreateJoystick(root);

            CreateActionButton(root, "Fire", "FIRE", new Vector2(0.875f, 0.13f), new Vector2(150f, 150f), Accent, MobileButtonActionType.Fire, 28, 0);
            CreateActionButton(root, "Aim", "AIM", new Vector2(0.765f, 0.34f), new Vector2(118f, 82f), PanelStrong, MobileButtonActionType.Aim, 18, 1);
            CreateActionButton(root, "Jump", "JUMP", new Vector2(0.755f, 0.20f), new Vector2(112f, 112f), PanelStrong, MobileButtonActionType.Jump, 19, 2);
            CreateActionButton(root, "Reload", "RELOAD", new Vector2(0.895f, 0.30f), new Vector2(118f, 78f), PanelStrong, MobileButtonActionType.Reload, 17, 3);
            CreateActionButton(root, "Heal", "HEAL", new Vector2(0.735f, 0.075f), new Vector2(110f, 74f), PanelStrong, MobileButtonActionType.Heal, 17, 4);
            CreateActionButton(root, "Interact", "USE", new Vector2(0.625f, 0.19f), new Vector2(108f, 76f), PanelStrong, MobileButtonActionType.Interact, 17, 5);
            CreateActionButton(root, "Switch", "SWAP", new Vector2(0.895f, 0.405f), new Vector2(110f, 68f), PanelStrong, MobileButtonActionType.SwitchWeapon, 16, 6);
            CreateActionButton(root, "Sprint", "SPRINT", new Vector2(0.12f, 0.39f), new Vector2(118f, 72f), PanelStrong, MobileButtonActionType.Sprint, 16, 7);

            CreateTopBadge(root, "BMG // BATTLE ROYALE", new Vector2(0.5f, 0.955f));
            canvasObject.AddComponent<MobileMatchHudPhaseVisibility>().Configure(local);
            Debug.Log("BMG mobile combat HUD installed successfully.");
            return true;
        }

        private static void EnsureBridge()
        {
            if (MobileInputBridge.Instance != null) return;
            new GameObject("MobileInputBridge").AddComponent<MobileInputBridge>();
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (eventSystem == null)
            {
                GameObject go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Object.DontDestroyOnLoad(go);
                return;
            }

            if (!eventSystem.gameObject.activeSelf) eventSystem.gameObject.SetActive(true);
            if (!eventSystem.enabled) eventSystem.enabled = true;
            if (eventSystem.GetComponent<BaseInputModule>() == null)
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
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
            look.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.001f);
        }

        private static void CreateJoystick(RectTransform root)
        {
            GameObject baseObject = new GameObject("MoveJoystick", typeof(RectTransform), typeof(RawImage), typeof(VirtualJoystick));
            baseObject.transform.SetParent(root, false);
            RectTransform rt = baseObject.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.12f, 0.20f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(250f, 250f);
            rt.anchoredPosition = Vector2.zero;
            if (joystickTexture == null) joystickTexture = Resources.Load<Texture2D>("UI/mobile_joystick");
            RawImage baseImage = baseObject.GetComponent<RawImage>();
            baseImage.texture = joystickTexture;
            baseImage.color = new Color(1f, 1f, 1f, .88f);

            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(RawImage));
            handle.transform.SetParent(baseObject.transform, false);
            RectTransform handleRt = handle.GetComponent<RectTransform>();
            handleRt.anchorMin = handleRt.anchorMax = new Vector2(0.5f, 0.5f);
            handleRt.pivot = new Vector2(0.5f, 0.5f);
            handleRt.sizeDelta = new Vector2(105f, 105f);
            handleRt.anchoredPosition = Vector2.zero;
            RawImage handleImage = handle.GetComponent<RawImage>();
            handleImage.texture = joystickTexture;
            handleImage.uvRect = new Rect(.30f, .30f, .40f, .40f);
            handleImage.color = new Color(1f, 1f, 1f, .90f);
            handleImage.raycastTarget = false;
        }

        private static void CreateActionButton(RectTransform root, string name, string label, Vector2 anchor, Vector2 size, Color color, MobileButtonActionType action, int fontSize, int iconIndex)
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
            CreateActionIcon(go.transform, iconIndex);
            CreateLabel(go.transform, label, fontSize);
        }

        private static void CreateActionIcon(Transform parent, int iconIndex)
        {
            if (actionAtlas == null)
                actionAtlas = Resources.Load<Texture2D>("BMG/UI/bmg_action_icons_3d");
            if (actionAtlas == null)
            {
                Debug.LogError("Approved BMG 3D action icon atlas is missing.");
                return;
            }
            if (iconIndex < 0 || iconIndex > 7) return;

            GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(RawImage));
            icon.transform.SetParent(parent, false);
            RectTransform rt = icon.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.18f, 0.20f);
            rt.anchorMax = new Vector2(0.82f, 0.93f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            int column = iconIndex % 4;
            int rowFromTop = iconIndex / 4;
            RawImage image = icon.GetComponent<RawImage>();
            image.texture = actionAtlas;
            image.uvRect = new Rect(column * 0.25f, rowFromTop == 0 ? 0.5f : 0f, 0.25f, 0.5f);
            image.color = Color.white;
            image.raycastTarget = false;
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
            rect.anchorMin = new Vector2(0f, 0.02f);
            rect.anchorMax = new Vector2(1f, 0.42f);
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
            text.resizeTextMinSize = 10;
            text.resizeTextMaxSize = size;
            text.raycastTarget = false;
        }
    }

    public sealed class MobileMatchHudPhaseVisibility : MonoBehaviour
    {
        private DropPlanePassenger passenger;
        private ParachuteController parachute;
        private Transform root;
        private bool? lastDropOnly;
        private bool? lastParachute;

        public void Configure(MatchParticipant participant)
        {
            root = transform;
            passenger = participant != null ? participant.GetComponent<DropPlanePassenger>() : null;
            parachute = participant != null ? participant.GetComponent<ParachuteController>() : null;
            Refresh(true);
        }

        private void Update() => Refresh(false);

        private void Refresh(bool force)
        {
            bool aboard = passenger != null && passenger.IsAboard;
            bool falling = !aboard && parachute != null && parachute.IsActive;
            if (!force && lastDropOnly == aboard && lastParachute == falling) return;
            lastDropOnly = aboard;
            lastParachute = falling;

            Set("MoveJoystick", !aboard);
            Set("Sprint", !aboard && !falling);
            Set("Fire", !aboard && !falling);
            Set("Aim", !aboard && !falling);
            Set("Reload", !aboard && !falling);
            Set("Heal", !aboard && !falling);
            Set("Interact", !aboard && !falling);
            Set("Switch", !aboard && !falling);
            Set("Jump", true);
        }

        private void Set(string childName, bool visible)
        {
            if (root == null) root = transform;
            Transform child = root.Find(childName);
            if (child != null && child.gameObject.activeSelf != visible) child.gameObject.SetActive(visible);
        }
    }

    public sealed class MobileControlsInstallRetry : MonoBehaviour
    {
        private float nextTry;
        private float stopAt;

        private void Awake() => stopAt = Time.unscaledTime + 20f;

        private void Update()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", System.StringComparison.OrdinalIgnoreCase))
            {
                Destroy(gameObject);
                return;
            }

            if (Time.unscaledTime > stopAt)
            {
                Debug.LogError("BMG mobile HUD failed to install: local participant did not become available in time.");
                Destroy(gameObject);
                return;
            }

            if (Time.unscaledTime < nextTry) return;
            nextTry = Time.unscaledTime + 0.2f;
            if (MobileMatchControlsInstaller.Install()) Destroy(gameObject);
        }
    }
}
