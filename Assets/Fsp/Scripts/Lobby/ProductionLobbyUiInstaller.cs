using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fsp.UI;
using Fsp.Localization;
using Fsp.Backend;
using Fsp.Presentation;

namespace Fsp.Lobby
{
    /// <summary>Responsive release lobby UI built at runtime for every Android aspect ratio.</summary>
    public sealed class ProductionLobbyUiInstaller : MonoBehaviour
    {
        private static readonly Color Navy = new(0.018f, 0.035f, 0.055f, 0.92f);
        private static readonly Color NavySoft = new(0.025f, 0.055f, 0.085f, 0.82f);
        private static readonly Color Orange = new(1f, 0.36f, 0.015f, 0.98f);
        private static readonly Color White = new(0.97f, 0.96f, 0.92f, 1f);

        private RectTransform safeRoot;
        private GameObject settingsPanel;
        private GameObject sectionPanel;
        private GameObject authPanel;
        private Text modeText;
        private Text characterText;
        private Text profileNameText;
        private Text connectionText;
        private Text musicText;
        private Text sfxText;
        private Text qualityText;
        private Text languageText;
        private Text deleteButtonText;
        private Text sectionBodyText;
        private GameObject sectionActionButton;
        private GameObject sectionDeleteButton;
        private GameObject teamControls;
        private GameObject wardrobeControls;
        private Text wardrobeSlotText;
        private Text wardrobeItemText;
        private Text wardrobeStatusText;
        private Image wardrobeSwatch;
        private CosmeticLoadout wardrobeLoadout;
        private CosmeticSlot wardrobeSlot = CosmeticSlot.Torso;
        private int wardrobeItemIndex;
        private InputField teamInviteInput;
        private Text teamStatusText;
        private SquadLobbyController boundSquadController;
        private InputField accountEmailInput;
        private InputField accountPasswordInput;
        private InputField accountPlayerNameInput;
        private Text accountStatusText;
        private Button accountSignInButton;
        private Button accountCreateButton;
        private Button accountSignOutButton;
        private Button accountGoogleButton;
        private bool deleteConfirmArmed;
        private float nextConnectionRefresh;
        private Canvas productionCanvas;

        public static bool EnsureInstalled()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", System.StringComparison.OrdinalIgnoreCase)) return false;
            ProductionLobbyUiInstaller installer = FindFirstObjectByType<ProductionLobbyUiInstaller>();
            if (installer == null)
                installer = new GameObject("FSP_ProductionLobbyUI").AddComponent<ProductionLobbyUiInstaller>();
            return installer != null && installer.EnsureCanvasReady();
        }

        private void Awake()
        {
            EnsureCanvasReady();
            ApplySavedSettings();
            RefreshLobbyState();
            if (LobbyState.Instance != null) LobbyState.Instance.Changed += RefreshLobbyState;
            FspLocalizationRuntime.LanguageChanged += HandleLanguageChanged;
        }

        private void OnDestroy()
        {
            if (LobbyState.Instance != null) LobbyState.Instance.Changed -= RefreshLobbyState;
            FspLocalizationRuntime.LanguageChanged -= HandleLanguageChanged;
            if (boundSquadController != null) boundSquadController.StatusChanged -= HandleSquadStatus;
        }

        private void Update()
        {
            if (productionCanvas == null) EnsureCanvasReady();
            if (Time.unscaledTime < nextConnectionRefresh) return;
            nextConnectionRefresh = Time.unscaledTime + 1f;
            RefreshConnectionState();
        }

        private bool EnsureCanvasReady()
        {
            EnsureEventSystem();
            if (productionCanvas != null) return true;
            GameObject existing = GameObject.Find("ProductionLobbyCanvas");
            if (existing != null) productionCanvas = existing.GetComponent<Canvas>();
            if (productionCanvas != null) return true;

            try
            {
                Build();
                return productionCanvas != null;
            }
            catch (System.Exception exception)
            {
                GameObject partial = GameObject.Find("ProductionLobbyCanvas");
                if (partial != null)
                {
                    partial.SetActive(false);
                    Destroy(partial);
                }
                productionCanvas = null;
                Debug.LogError("FSP responsive lobby UI could not be created: " + exception);
                return false;
            }
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (eventSystem == null)
            {
                GameObject inputObject = new("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                DontDestroyOnLoad(inputObject);
                return;
            }

            if (!eventSystem.gameObject.activeSelf)
                eventSystem.gameObject.SetActive(true);
            if (!eventSystem.enabled)
                eventSystem.enabled = true;
            if (eventSystem.GetComponent<BaseInputModule>() == null)
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }

        private void Build()
        {
            GameObject canvasObject = new("ProductionLobbyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 20;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRoot = canvasObject.GetComponent<RectTransform>();
            GameObject backdrop = new("LobbyBackdrop", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(canvasRoot, false);
            RectTransform backdropRt = backdrop.GetComponent<RectTransform>();
            backdropRt.anchorMin = Vector2.zero;
            backdropRt.anchorMax = Vector2.one;
            backdropRt.offsetMin = Vector2.zero;
            backdropRt.offsetMax = Vector2.zero;
            backdrop.GetComponent<Image>().color = new Color(0.008f, 0.015f, 0.023f, 1f);
            backdrop.GetComponent<Image>().raycastTarget = false;

            GameObject safeViewport = new("LobbySafeViewport", typeof(RectTransform), typeof(MobileSafeArea));
            safeViewport.transform.SetParent(canvasRoot, false);
            RectTransform safeViewportRt = safeViewport.GetComponent<RectTransform>();
            safeViewportRt.anchorMin = Vector2.zero;
            safeViewportRt.anchorMax = Vector2.one;
            safeViewportRt.offsetMin = Vector2.zero;
            safeViewportRt.offsetMax = Vector2.zero;

            GameObject contentFrame = new("LobbyContentFrame", typeof(RectTransform), typeof(AspectRatioFitter));
            contentFrame.transform.SetParent(safeViewportRt, false);
            RectTransform frameRt = contentFrame.GetComponent<RectTransform>();
            frameRt.anchorMin = Vector2.zero;
            frameRt.anchorMax = Vector2.one;
            frameRt.offsetMin = Vector2.zero;
            frameRt.offsetMax = Vector2.zero;
            AspectRatioFitter frameFitter = contentFrame.GetComponent<AspectRatioFitter>();
            frameFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            frameFitter.aspectRatio = 16f / 9f;
            CreateBackground(frameRt);

            GameObject safe = new("SafeRoot", typeof(RectTransform));
            safe.transform.SetParent(frameRt, false);
            safeRoot = safe.GetComponent<RectTransform>();
            safeRoot.anchorMin = Vector2.zero;
            safeRoot.anchorMax = Vector2.one;
            safeRoot.offsetMin = new Vector2(34f, 24f);
            safeRoot.offsetMax = new Vector2(-34f, -24f);

            CreateTopBar();
            CreateModePanel();
            CreateBottomActions();
            CreateSettingsPanel();
            CreateSectionPanel();
            CreateAuthPanel();
            productionCanvas = canvas;
        }

        private static void CreateBackground(RectTransform parent)
        {
            GameObject oldArtwork = GameObject.Find("FSP_FIXED_LOBBY_ART");
            if (oldArtwork != null)
            {
                SpriteRenderer sr = oldArtwork.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = false;
            }

            Sprite sprite = Resources.Load<Sprite>("Lobby/fsp_lobby_final");
            GameObject background = new("LobbyBackground", typeof(RectTransform), typeof(RawImage));
            background.transform.SetParent(parent, false);
            RectTransform rt = background.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            RawImage image = background.GetComponent<RawImage>();
            image.texture = sprite != null ? sprite.texture : null;
            image.color = Color.white;
            image.raycastTarget = false;
        }

        private void CreateTopBar()
        {
            RectTransform profile = Panel(safeRoot, "Profile", new Vector2(0.018f, 0.855f), new Vector2(0.27f, 0.982f), NavySoft);
            profileNameText = Label(profile, "PLAYER", 25, new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.92f), TextAnchor.MiddleLeft);
            connectionText = Label(profile, "GUEST • OFFLINE MATCH", 15, new Vector2(0.05f, 0.10f), new Vector2(0.95f, 0.52f), TextAnchor.MiddleLeft);

            RectTransform wallet = Panel(safeRoot, "Wallet", new Vector2(0.70f, 0.89f), new Vector2(0.91f, 0.982f), NavySoft);
            Label(wallet, "GOLD  0     COINS  0", 20, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
            Button(safeRoot, "Settings", "SETTINGS", new Vector2(0.918f, 0.89f), new Vector2(0.995f, 0.982f), Navy, ToggleSettings, 16);
        }

        private void CreateModePanel()
        {
            RectTransform panel = Panel(safeRoot, "ModePanel", new Vector2(0.018f, 0.18f), new Vector2(0.255f, 0.55f), NavySoft);
            Label(panel, "BATTLE MODE", 22, new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.96f), TextAnchor.MiddleCenter);
            modeText = Label(panel, "SOLO", 29, new Vector2(0.06f, 0.55f), new Vector2(0.94f, 0.78f), TextAnchor.MiddleCenter);
            Button(panel, "Solo", "SOLO", new Vector2(0.06f, 0.31f), new Vector2(0.47f, 0.53f), Orange, () => LobbyState.Instance?.SetMode(MatchMode.Solo), 19);
            Button(panel, "Squad", "SQUAD", new Vector2(0.53f, 0.31f), new Vector2(0.94f, 0.53f), Navy, () => LobbyState.Instance?.SetMode(MatchMode.Squad), 19);
            Button(panel, "Prev", "<", new Vector2(0.06f, 0.07f), new Vector2(0.22f, 0.27f), Navy, PreviousCharacter, 24);
            characterText = Label(panel, "SOLDIER 01", 17, new Vector2(0.23f, 0.07f), new Vector2(0.77f, 0.27f), TextAnchor.MiddleCenter);
            Button(panel, "Next", ">", new Vector2(0.78f, 0.07f), new Vector2(0.94f, 0.27f), Navy, NextCharacter, 24);
        }

        private void CreateBottomActions()
        {
            Button(safeRoot, "Team", "TEAM", new Vector2(0.018f, 0.055f), new Vector2(0.115f, 0.145f), Navy, OpenTeam, 18);
            Button(safeRoot, "Loadout", "LOADOUT", new Vector2(0.123f, 0.055f), new Vector2(0.235f, 0.145f), Navy, OpenLoadout, 18);
            Button(safeRoot, "Missions", "MISSIONS", new Vector2(0.243f, 0.055f), new Vector2(0.355f, 0.145f), Navy, OpenMissions, 18);
            Button(safeRoot, "Account", "ACCOUNT", new Vector2(0.363f, 0.055f), new Vector2(0.465f, 0.145f), Navy, OpenAccountPanel, 18);

            // The only START control in the final lobby artwork, aligned inside the 16:9 frame.
            Button(safeRoot, "Start", "START", new Vector2(0.805f, 0.025f), new Vector2(0.995f, 0.145f), Orange, StartMatch, 34);
        }

        private void CreateSettingsPanel()
        {
            settingsPanel = new GameObject("SettingsModal", typeof(RectTransform), typeof(Image));
            settingsPanel.transform.SetParent(safeRoot, false);
            RectTransform panel = settingsPanel.GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(0.29f, 0.10f);
            panel.anchorMax = new Vector2(0.71f, 0.90f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            settingsPanel.GetComponent<Image>().color = new Color(0.012f, 0.025f, 0.045f, 0.97f);

            Label(panel, "SETTINGS", 32, new Vector2(0.08f, 0.86f), new Vector2(0.92f, 0.97f), TextAnchor.MiddleCenter);
            musicText = Label(panel, "MUSIC: ON", 22, new Vector2(0.10f, 0.72f), new Vector2(0.58f, 0.82f), TextAnchor.MiddleLeft);
            Button(panel, "MusicToggle", "TOGGLE", new Vector2(0.63f, 0.72f), new Vector2(0.90f, 0.82f), NavySoft, ToggleMusic, 17);
            sfxText = Label(panel, "SFX: ON", 22, new Vector2(0.10f, 0.59f), new Vector2(0.58f, 0.69f), TextAnchor.MiddleLeft);
            Button(panel, "SfxToggle", "TOGGLE", new Vector2(0.63f, 0.59f), new Vector2(0.90f, 0.69f), NavySoft, ToggleSfx, 17);
            qualityText = Label(panel, "GRAPHICS: MEDIUM", 21, new Vector2(0.10f, 0.46f), new Vector2(0.58f, 0.56f), TextAnchor.MiddleLeft);
            Button(panel, "Quality", "CHANGE", new Vector2(0.63f, 0.46f), new Vector2(0.90f, 0.56f), NavySoft, CycleQuality, 17);
            languageText = Label(panel, "LANGUAGE", 21, new Vector2(0.10f, 0.33f), new Vector2(0.58f, 0.43f), TextAnchor.MiddleLeft);
            Button(panel, "Language", "CHANGE", new Vector2(0.63f, 0.33f), new Vector2(0.90f, 0.43f), NavySoft, CycleLanguage, 17);
            Button(panel, "AccountPrivacy", "ACCOUNT & PRIVACY", new Vector2(0.18f, 0.19f), new Vector2(0.82f, 0.29f), NavySoft, OpenAccountPrivacy, 17);
            Button(panel, "Close", "SAVE & CLOSE", new Vector2(0.22f, 0.04f), new Vector2(0.78f, 0.14f), Orange, ToggleSettings, 20);
            settingsPanel.SetActive(false);
        }

        private void CreateSectionPanel()
        {
            sectionPanel = new GameObject("LobbySectionModal", typeof(RectTransform), typeof(Image));
            sectionPanel.transform.SetParent(safeRoot, false);
            RectTransform panel = sectionPanel.GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(0.27f, 0.20f);
            panel.anchorMax = new Vector2(0.73f, 0.80f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            sectionPanel.GetComponent<Image>().color = new Color(0.012f, 0.025f, 0.045f, 0.97f);
            Label(panel, "SECTION", 32, new Vector2(0.08f, 0.76f), new Vector2(0.92f, 0.94f), TextAnchor.MiddleCenter).name = "SectionTitle";
            sectionBodyText = Label(panel, "", 21, new Vector2(0.10f, 0.24f), new Vector2(0.90f, 0.75f), TextAnchor.MiddleCenter);
            sectionBodyText.name = "SectionBody";
            sectionActionButton = Button(panel, "PrivacyPolicy", "OPEN FULL POLICY", new Vector2(0.05f, 0.06f), new Vector2(0.35f, 0.18f), NavySoft, OpenFullPrivacyPolicy, 16).gameObject;
            Button deleteButton = Button(panel, "DeleteAccount", "DELETE ACCOUNT", new Vector2(0.37f, 0.06f), new Vector2(0.67f, 0.18f), new Color(0.55f, 0.08f, 0.06f, 0.95f), DeleteAccount, 15);
            sectionDeleteButton = deleteButton.gameObject;
            deleteButtonText = deleteButton.GetComponentInChildren<Text>();
            Button(panel, "Close", "CLOSE", new Vector2(0.69f, 0.06f), new Vector2(0.95f, 0.18f), Orange, CloseSection, 18);
            sectionActionButton.SetActive(false);
            sectionDeleteButton.SetActive(false);
            CreateTeamControls(panel);
            CreateWardrobeControls(panel);
            sectionPanel.SetActive(false);
        }

        private void CreateAuthPanel()
        {
            authPanel = new GameObject("AccountModal", typeof(RectTransform), typeof(Image));
            authPanel.transform.SetParent(safeRoot, false);
            RectTransform panel = authPanel.GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(0.31f, 0.20f);
            panel.anchorMax = new Vector2(0.69f, 0.80f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            authPanel.GetComponent<Image>().color = new Color(0.012f, 0.025f, 0.045f, 0.98f);

            Label(panel, "ACCOUNT", 30, new Vector2(0.08f, 0.83f), new Vector2(0.92f, 0.96f), TextAnchor.MiddleCenter);
            accountPlayerNameInput = CreateInput(panel, "PlayerName", "PLAYER NAME", new Vector2(0.10f, 0.70f), new Vector2(0.90f, 0.81f), InputField.ContentType.Standard, 18);
            accountEmailInput = CreateInput(panel, "Email", "EMAIL", new Vector2(0.10f, 0.57f), new Vector2(0.90f, 0.68f), InputField.ContentType.EmailAddress, 160);
            accountPasswordInput = CreateInput(panel, "Password", "PASSWORD", new Vector2(0.10f, 0.44f), new Vector2(0.90f, 0.55f), InputField.ContentType.Password, 72);
            accountStatusText = Label(panel, "", 14, new Vector2(0.10f, 0.37f), new Vector2(0.90f, 0.43f), TextAnchor.MiddleCenter);
            accountGoogleButton = Button(panel, "GoogleSignIn", "CONTINUE WITH GOOGLE", new Vector2(0.18f, 0.27f), new Vector2(0.82f, 0.36f), Color.white, SignInWithGoogle, 16);
            Text googleText = accountGoogleButton.GetComponentInChildren<Text>();
            if (googleText != null) googleText.color = new Color(.08f, .10f, .12f, 1f);
            accountSignInButton = Button(panel, "SignIn", "SIGN IN", new Vector2(0.08f, 0.14f), new Vector2(0.36f, 0.24f), Orange, SignInAccount, 17);
            accountCreateButton = Button(panel, "Create", "CREATE ACCOUNT", new Vector2(0.38f, 0.14f), new Vector2(0.68f, 0.24f), NavySoft, CreateAccount, 15);
            accountSignOutButton = Button(panel, "SignOut", "SIGN OUT", new Vector2(0.70f, 0.14f), new Vector2(0.92f, 0.24f), new Color(0.55f, 0.08f, 0.06f, 0.95f), SignOutAccount, 15);
            Button(panel, "Close", "SAVE & CLOSE", new Vector2(0.31f, 0.02f), new Vector2(0.69f, 0.11f), NavySoft, CloseAccountPanel, 17);
            authPanel.SetActive(false);
        }

        private static InputField CreateInput(Transform parent, string name, string placeholderValue, Vector2 min, Vector2 max, InputField.ContentType contentType, int limit)
        {
            GameObject inputObject = new(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            inputObject.transform.SetParent(parent, false);
            RectTransform rect = inputObject.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            inputObject.GetComponent<Image>().color = new Color(0.03f, 0.07f, 0.11f, 1f);
            Text inputText = Label(inputObject.transform, "", 18, new Vector2(0.04f, 0f), new Vector2(0.96f, 1f), TextAnchor.MiddleLeft);
            inputText.raycastTarget = true;
            Text placeholder = Label(inputObject.transform, placeholderValue, 18, new Vector2(0.04f, 0f), new Vector2(0.96f, 1f), TextAnchor.MiddleLeft);
            placeholder.color = new Color(1f, 1f, 1f, 0.45f);
            InputField input = inputObject.GetComponent<InputField>();
            input.textComponent = inputText;
            input.placeholder = placeholder;
            input.characterLimit = Mathf.Max(1, limit);
            input.contentType = contentType;
            return input;
        }

        private void CreateTeamControls(RectTransform panel)
        {
            teamControls = new GameObject("TeamControls", typeof(RectTransform));
            teamControls.transform.SetParent(panel, false);
            RectTransform root = teamControls.GetComponent<RectTransform>();
            root.anchorMin = new Vector2(0.08f, 0.17f);
            root.anchorMax = new Vector2(0.92f, 0.50f);
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            GameObject inputObject = new("InviteName", typeof(RectTransform), typeof(Image), typeof(InputField));
            inputObject.transform.SetParent(root, false);
            RectTransform inputRect = inputObject.GetComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0f, 0.68f);
            inputRect.anchorMax = new Vector2(1f, 1f);
            inputRect.offsetMin = Vector2.zero;
            inputRect.offsetMax = Vector2.zero;
            inputObject.GetComponent<Image>().color = new Color(0.03f, 0.07f, 0.11f, 1f);
            Text inputText = Label(inputObject.transform, "", 18, new Vector2(0.04f, 0f), new Vector2(0.96f, 1f), TextAnchor.MiddleLeft);
            inputText.raycastTarget = true;
            Text placeholder = Label(inputObject.transform, "INVITE PLAYER", 18, new Vector2(0.04f, 0f), new Vector2(0.96f, 1f), TextAnchor.MiddleLeft);
            placeholder.color = new Color(1f, 1f, 1f, 0.45f);
            teamInviteInput = inputObject.GetComponent<InputField>();
            teamInviteInput.textComponent = inputText;
            teamInviteInput.placeholder = placeholder;
            teamInviteInput.characterLimit = 18;

            teamStatusText = Label(root, "", 14, new Vector2(0f, 0.34f), new Vector2(1f, 0.65f), TextAnchor.MiddleCenter);
            Button(root, "Invite", "CREATE / INVITE", new Vector2(0f, 0f), new Vector2(0.31f, 0.30f), NavySoft, CreateOrInviteSquad, 14);
            Button(root, "Ready", "READY", new Vector2(0.33f, 0f), new Vector2(0.52f, 0.30f), Orange, MarkSquadReady, 14);
            Button(root, "Invites", "CHECK INVITES", new Vector2(0.54f, 0f), new Vector2(0.79f, 0.30f), NavySoft, AcceptLatestSquadInvite, 12);
            Button(root, "Leave", "LEAVE", new Vector2(0.81f, 0f), new Vector2(1f, 0.30f), new Color(0.55f, 0.08f, 0.06f, 0.95f), LeaveSquad, 14);
            teamControls.SetActive(false);
        }

        private void CreateWardrobeControls(RectTransform panel)
        {
            wardrobeControls = new GameObject("WardrobeControls", typeof(RectTransform));
            wardrobeControls.transform.SetParent(panel, false);
            RectTransform root = wardrobeControls.GetComponent<RectTransform>();
            root.anchorMin = new Vector2(0.08f, 0.18f);
            root.anchorMax = new Vector2(0.92f, 0.67f);
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            wardrobeSlotText = Label(root, "TORSO", 20, new Vector2(0.18f, 0.72f), new Vector2(0.82f, 0.98f), TextAnchor.MiddleCenter);
            Button(root, "PrevSlot", "<", new Vector2(0f, 0.72f), new Vector2(0.15f, 0.98f), NavySoft, PreviousWardrobeSlot, 22);
            Button(root, "NextSlot", ">", new Vector2(0.85f, 0.72f), new Vector2(1f, 0.98f), NavySoft, NextWardrobeSlot, 22);

            GameObject swatchObject = new("ColorPreview", typeof(RectTransform), typeof(Image));
            swatchObject.transform.SetParent(root, false);
            RectTransform swatchRt = swatchObject.GetComponent<RectTransform>();
            swatchRt.anchorMin = new Vector2(0.40f, 0.40f);
            swatchRt.anchorMax = new Vector2(0.60f, 0.68f);
            swatchRt.offsetMin = Vector2.zero;
            swatchRt.offsetMax = Vector2.zero;
            wardrobeSwatch = swatchObject.GetComponent<Image>();

            wardrobeItemText = Label(root, "FOREST UNIFORM", 17, new Vector2(0.18f, 0.13f), new Vector2(0.82f, 0.39f), TextAnchor.MiddleCenter);
            Button(root, "PrevItem", "<", new Vector2(0f, 0.13f), new Vector2(0.15f, 0.39f), NavySoft, PreviousWardrobeItem, 22);
            Button(root, "NextItem", ">", new Vector2(0.85f, 0.13f), new Vector2(1f, 0.39f), NavySoft, NextWardrobeItem, 22);
            Button(root, "Equip", "EQUIP & SAVE", new Vector2(0.28f, 0f), new Vector2(0.72f, 0.12f), Orange, EquipWardrobeItem, 15);
            wardrobeStatusText = Label(root, "", 13, new Vector2(0f, -0.14f), new Vector2(1f, -0.01f), TextAnchor.MiddleCenter);
            wardrobeControls.SetActive(false);
        }

        private void OpenSection(string title, string body)
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (sectionPanel == null) return;
            if (sectionActionButton != null) sectionActionButton.SetActive(false);
            if (sectionDeleteButton != null) sectionDeleteButton.SetActive(false);
            if (teamControls != null) teamControls.SetActive(false);
            if (wardrobeControls != null) wardrobeControls.SetActive(false);
            if (sectionBodyText != null)
            {
                RectTransform bodyRt = sectionBodyText.rectTransform;
                bodyRt.anchorMin = new Vector2(0.10f, 0.24f);
                bodyRt.anchorMax = new Vector2(0.90f, 0.75f);
            }
            deleteConfirmArmed = false;
            if (deleteButtonText != null) deleteButtonText.text = FspLocalizationRuntime.T("DELETE ACCOUNT");
            Text[] labels = sectionPanel.GetComponentsInChildren<Text>(true);
            foreach (Text label in labels)
            {
                if (label.name == "SectionTitle") label.text = FspLocalizationRuntime.T(title);
                else if (label.name == "SectionBody") label.text = FspLocalizationRuntime.T(body);
            }
            sectionPanel.SetActive(true);
        }

        private void CloseSection() => sectionPanel?.SetActive(false);

        private void OpenTeam()
        {
            SquadLobbyState squad = SquadLobbyState.Instance;
            string body = !SupabaseSession.IsSignedIn
                ? FspLocalizationRuntime.T("Sign in first to create a squad and invite players.")
                : squad != null && squad.HasSquad
                    ? FspLocalizationRuntime.T("YOU") + "   " + FspLocalizationRuntime.T("READY") + "\n" + (squad.Members != null ? squad.Members.Length : 1) + " / 4"
                    : FspLocalizationRuntime.T("Invite players and manage your squad before deployment.");
            OpenSection("TEAM", body);
            if (teamControls != null) teamControls.SetActive(SupabaseSession.IsSignedIn);
            if (sectionBodyText != null)
            {
                sectionBodyText.rectTransform.anchorMin = new Vector2(0.10f, 0.52f);
                sectionBodyText.rectTransform.anchorMax = new Vector2(0.90f, 0.75f);
            }
            SquadLobbyController controller = FindFirstObjectByType<SquadLobbyController>();
            BindSquadController(controller);
            if (controller != null && squad != null && squad.HasSquad) controller.RefreshMembers();
        }

        private void CreateOrInviteSquad()
        {
            SquadLobbyController controller = FindFirstObjectByType<SquadLobbyController>();
            if (controller == null) return;
            SquadLobbyState squad = SquadLobbyState.Instance;
            string inviteName = teamInviteInput != null ? teamInviteInput.text.Trim() : string.Empty;
            if (squad == null || !squad.HasSquad)
            {
                if (string.IsNullOrWhiteSpace(inviteName)) controller.CreateSquad();
                else controller.InviteName(inviteName);
                return;
            }
            controller.InviteName(inviteName);
        }

        private void MarkSquadReady() => FindFirstObjectByType<SquadLobbyController>()?.SetReady(true);

        private void AcceptLatestSquadInvite() => FindFirstObjectByType<SquadLobbyController>()?.AcceptLatestInvite();

        private void BindSquadController(SquadLobbyController controller)
        {
            if (boundSquadController == controller) return;
            if (boundSquadController != null) boundSquadController.StatusChanged -= HandleSquadStatus;
            boundSquadController = controller;
            if (boundSquadController != null)
            {
                boundSquadController.StatusChanged += HandleSquadStatus;
                HandleSquadStatus(boundSquadController.LastStatus);
            }
        }

        private void HandleSquadStatus(string status)
        {
            if (teamStatusText != null) teamStatusText.text = FspLocalizationRuntime.T(status ?? string.Empty);
        }

        private void LeaveSquad()
        {
            FindFirstObjectByType<SquadLobbyController>()?.LeaveSquad();
            CloseSection();
        }

        private void OpenLoadout()
        {
            string selected = LobbyGameplayProgress.LoadoutName;
            string localizedSelected = FspLocalizationRuntime.T(selected);
            OpenSection("LOADOUT",
                FspLocalizationRuntime.T("SELECTED") + ": " + localizedSelected + "\n" +
                FspLocalizationRuntime.T("PRIMARY") + "  •  " + localizedSelected + " " + FspLocalizationRuntime.T("RIFLE"));
            if (wardrobeControls != null) wardrobeControls.SetActive(true);
            if (sectionBodyText != null)
            {
                sectionBodyText.rectTransform.anchorMin = new Vector2(0.10f, 0.68f);
                sectionBodyText.rectTransform.anchorMax = new Vector2(0.90f, 0.75f);
            }
            RefreshWardrobeAsync();
            if (sectionActionButton != null)
            {
                Button button = sectionActionButton.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Fsp.Audio.FspAudioRuntime.PlayUiClick);
                button.onClick.AddListener(CycleLoadoutAndRefresh);
                Text label = sectionActionButton.GetComponentInChildren<Text>();
                if (label != null) label.text = FspLocalizationRuntime.T("NEXT LOADOUT");
                sectionActionButton.SetActive(true);
            }
        }

        private void CycleLoadoutAndRefresh()
        {
            LobbyGameplayProgress.CycleLoadout();
            OpenLoadout();
        }

        private async void RefreshWardrobeAsync()
        {
            try
            {
                SupabaseCosmeticsClient backend = FindFirstObjectByType<SupabaseCosmeticsClient>();
                wardrobeLoadout = backend != null ? await backend.LoadEquippedAsync() : StarterWardrobeRuntime.LoadLocal();
                StarterWardrobeRuntime.SaveLocal(wardrobeLoadout);
                SetWardrobeStatus(string.Empty);
            }
            catch (System.Exception ex)
            {
                wardrobeLoadout = StarterWardrobeRuntime.LoadLocal();
                SetWardrobeStatus("Offline selection loaded");
                Debug.LogWarning("FSP wardrobe load failed: " + ex.Message);
            }
            SelectEquippedWardrobeIndex();
            RefreshWardrobeLabels();
        }

        private void PreviousWardrobeSlot() => ChangeWardrobeSlot(-1);
        private void NextWardrobeSlot() => ChangeWardrobeSlot(1);

        private void ChangeWardrobeSlot(int direction)
        {
            int count = System.Enum.GetValues(typeof(CosmeticSlot)).Length;
            wardrobeSlot = (CosmeticSlot)(((int)wardrobeSlot + direction + count) % count);
            SelectEquippedWardrobeIndex();
            RefreshWardrobeLabels();
        }

        private void PreviousWardrobeItem() => ChangeWardrobeItem(-1);
        private void NextWardrobeItem() => ChangeWardrobeItem(1);

        private void ChangeWardrobeItem(int direction)
        {
            var items = StarterCosmeticCatalog.GetItems(wardrobeSlot);
            if (items.Count == 0) return;
            wardrobeItemIndex = (wardrobeItemIndex + direction + items.Count) % items.Count;
            RefreshWardrobeLabels();
        }

        private async void EquipWardrobeItem()
        {
            var items = StarterCosmeticCatalog.GetItems(wardrobeSlot);
            if (items.Count == 0) return;
            wardrobeLoadout ??= StarterWardrobeRuntime.LoadLocal();
            StarterCosmeticCatalog.Item item = items[Mathf.Clamp(wardrobeItemIndex, 0, items.Count - 1)];
            wardrobeLoadout.Set(wardrobeSlot, item.Id);
            StarterWardrobeRuntime.SaveLocal(wardrobeLoadout);
            SetWardrobeStatus("Saved on this device");
            try
            {
                SupabaseCosmeticsClient backend = FindFirstObjectByType<SupabaseCosmeticsClient>();
                if (backend != null && SupabaseSession.IsSignedIn)
                {
                    await backend.SaveEquippedAsync(wardrobeLoadout);
                    SetWardrobeStatus("Saved to account");
                }
            }
            catch (System.Exception ex)
            {
                SetWardrobeStatus("Saved locally; account sync pending");
                Debug.LogWarning("FSP wardrobe save failed: " + ex.Message);
            }
            RefreshWardrobeLabels();
        }

        private void SelectEquippedWardrobeIndex()
        {
            var items = StarterCosmeticCatalog.GetItems(wardrobeSlot);
            wardrobeItemIndex = 0;
            string equippedId = wardrobeLoadout != null ? wardrobeLoadout.Get(wardrobeSlot) : string.Empty;
            for (int i = 0; i < items.Count; i++)
                if (items[i].Id == equippedId) { wardrobeItemIndex = i; break; }
        }

        private void RefreshWardrobeLabels()
        {
            var items = StarterCosmeticCatalog.GetItems(wardrobeSlot);
            if (items.Count == 0) return;
            StarterCosmeticCatalog.Item item = items[Mathf.Clamp(wardrobeItemIndex, 0, items.Count - 1)];
            if (wardrobeSlotText != null) wardrobeSlotText.text = FspLocalizationRuntime.T(wardrobeSlot.ToString().ToUpperInvariant());
            if (wardrobeItemText != null) wardrobeItemText.text = FspLocalizationRuntime.T(item.Label);
            if (wardrobeSwatch != null) wardrobeSwatch.color = item.Color;
        }

        private void SetWardrobeStatus(string key)
        {
            if (wardrobeStatusText != null) wardrobeStatusText.text = FspLocalizationRuntime.T(key);
        }

        private void OpenMissions() => OpenSection("MISSIONS", LobbyGameplayProgress.MissionSummary);

        private void OpenAccountPrivacy()
        {
            OpenSection("ACCOUNT & PRIVACY", "FSP stores your profile, match progress and squad data when you sign in. Voice chat uses the microphone only after permission. You can play offline without an account. Account deletion permanently removes your account and associated game data.");
            if (sectionActionButton != null)
            {
                Button button = sectionActionButton.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Fsp.Audio.FspAudioRuntime.PlayUiClick);
                button.onClick.AddListener(OpenFullPrivacyPolicy);
                Text label = sectionActionButton.GetComponentInChildren<Text>();
                if (label != null) label.text = FspLocalizationRuntime.T("OPEN FULL POLICY");
                sectionActionButton.SetActive(true);
            }
            if (sectionDeleteButton != null) sectionDeleteButton.SetActive(true);
        }

        private void OpenAccountPanel()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (sectionPanel != null) sectionPanel.SetActive(false);
            if (authPanel == null) return;
            authPanel.SetActive(true);
            RefreshAccountPanel();
        }

        private void CloseAccountPanel()
        {
            ApplyPlayerName();
            PersistProfileSelection();
            authPanel?.SetActive(false);
        }

        private bool ValidateAccountInput()
        {
            string email = accountEmailInput != null ? accountEmailInput.text.Trim() : string.Empty;
            string password = accountPasswordInput != null ? accountPasswordInput.text : string.Empty;
            if (!email.Contains("@") || password.Length < 6)
            {
                SetAccountStatus("Enter a valid email and a password of at least 6 characters");
                return false;
            }
            return true;
        }

        private void SignInAccount()
        {
            if (!ValidateAccountInput()) return;
            ApplyPlayerName();
            SupabaseAuthClient auth = FindFirstObjectByType<SupabaseAuthClient>();
            if (auth == null) { SetAccountStatus("Sign in failed"); return; }
            SetAccountStatus("Signing in...");
            StartCoroutine(auth.SignIn(accountEmailInput.text.Trim(), accountPasswordInput.text, (ok, error) =>
            {
                if (!ok)
                {
                    SetAccountStatus("Sign in failed");
                    if (!string.IsNullOrWhiteSpace(error)) Debug.LogWarning("FSP sign in failed: " + error);
                    return;
                }
                SetAccountStatus("Signed in");
                LobbyBackendRuntimeInstaller.EnsureInstalled();
                CompleteSignIn();
            }));
        }

        private void SignInWithGoogle()
        {
            ApplyPlayerName();
            SupabaseAuthClient auth = FindFirstObjectByType<SupabaseAuthClient>();
            if (auth == null) { SetAccountStatus("Google sign in failed"); return; }
            SetAccountStatus("Opening Google sign in...");
            auth.BeginGoogleSignIn((ok, error) =>
            {
                if (!ok)
                {
                    SetAccountStatus("Google sign in failed");
                    if (!string.IsNullOrWhiteSpace(error)) Debug.LogWarning("FSP Google sign in failed: " + error);
                    return;
                }
                SetAccountStatus("Signed in");
                LobbyBackendRuntimeInstaller.EnsureInstalled();
                CompleteSignIn();
            });
        }

        private async void CompleteSignIn()
        {
            LobbyProfileSync profile = FindFirstObjectByType<LobbyProfileSync>();
            try
            {
                if (profile != null) await profile.LoadIntoLobbyAsync();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("FSP profile load failed after sign in: " + ex.Message);
                SetAccountStatus("Signed in, but profile could not be loaded");
            }

            PersistPlayerDirectory();
            RefreshAccountPanel();
            RefreshLobbyState();
        }

        private void CreateAccount()
        {
            if (!ValidateAccountInput()) return;
            ApplyPlayerName();
            SupabaseAuthClient auth = FindFirstObjectByType<SupabaseAuthClient>();
            if (auth == null) { SetAccountStatus("Could not create account"); return; }
            SetAccountStatus("Creating account...");
            StartCoroutine(auth.SignUp(accountEmailInput.text.Trim(), accountPasswordInput.text, (ok, error) =>
            {
                SetAccountStatus(ok ? "Account created. Verify your email, then sign in." : "Could not create account");
                if (!ok && !string.IsNullOrWhiteSpace(error)) Debug.LogWarning("FSP sign up failed: " + error);
            }));
        }

        private void SignOutAccount()
        {
            SupabaseAuthClient auth = FindFirstObjectByType<SupabaseAuthClient>();
            if (auth == null) { SupabaseSession.Clear(); CompleteLocalSignOut(); return; }
            SetAccountStatus("Signing out...");
            StartCoroutine(auth.SignOut((ok, error) =>
            {
                if (!ok && !string.IsNullOrWhiteSpace(error)) Debug.LogWarning("FSP server sign out failed; local session was cleared: " + error);
                CompleteLocalSignOut();
            }));
        }

        private void CompleteLocalSignOut()
        {
            SquadLobbyState.Instance?.Clear();
            MatchRoomState.Instance?.Clear();
            SetAccountStatus(string.Empty);
            RefreshAccountPanel();
            RefreshConnectionState();
        }

        private void RefreshAccountPanel()
        {
            bool signedIn = SupabaseSession.IsSignedIn;
            if (accountPlayerNameInput != null && !accountPlayerNameInput.isFocused)
                accountPlayerNameInput.text = LobbyState.Instance != null ? LobbyState.Instance.DisplayName : "Player";
            if (accountEmailInput != null) accountEmailInput.interactable = !signedIn;
            if (accountPasswordInput != null) accountPasswordInput.interactable = !signedIn;
            if (accountSignInButton != null) accountSignInButton.interactable = !signedIn;
            if (accountCreateButton != null) accountCreateButton.interactable = !signedIn;
            if (accountGoogleButton != null) accountGoogleButton.interactable = !signedIn;
            if (accountSignOutButton != null) accountSignOutButton.interactable = signedIn;
            if (signedIn) SetAccountStatus("Signed in");
        }

        private void ApplyPlayerName()
        {
            if (accountPlayerNameInput == null || LobbyState.Instance == null) return;
            LobbyState.Instance.SetDisplayName(accountPlayerNameInput.text);
        }

        private async void PersistProfileSelection()
        {
            if (!SupabaseSession.IsSignedIn || LobbyState.Instance == null) return;
            LobbyProfileSync profile = FindFirstObjectByType<LobbyProfileSync>();
            if (profile != null)
            {
                try { await profile.SaveLobbyAsync(); }
                catch (System.Exception ex) { Debug.LogWarning("FSP profile save failed: " + ex.Message); }
            }

            PersistPlayerDirectory();
        }

        private void PersistPlayerDirectory()
        {
            if (!SupabaseSession.IsSignedIn || LobbyState.Instance == null) return;
            SupabaseSquadClient squad = FindFirstObjectByType<SupabaseSquadClient>();
            if (squad != null)
                StartCoroutine(squad.UpsertDirectory(LobbyState.Instance.DisplayName, LobbyState.Instance.SelectedCharacterId,
                    (ok, error) => { if (!ok && !string.IsNullOrWhiteSpace(error)) Debug.LogWarning("FSP player directory save failed: " + error); }));
        }

        private void SetAccountStatus(string key)
        {
            if (accountStatusText != null) accountStatusText.text = FspLocalizationRuntime.T(key);
        }

        private static void OpenFullPrivacyPolicy()
        {
            string language = FspLocalizationRuntime.Current == FspLanguage.Arabic ? "ar" : "en";
            Application.OpenURL(LegalRuntimeConfig.PrivacyPolicyUrl + "?lang=" + language);
        }

        private void DeleteAccount()
        {
            if (!SupabaseSession.IsSignedIn)
            {
                if (deleteButtonText != null) deleteButtonText.text = FspLocalizationRuntime.T("NO ACCOUNT");
                return;
            }
            if (!deleteConfirmArmed)
            {
                deleteConfirmArmed = true;
                if (deleteButtonText != null) deleteButtonText.text = FspLocalizationRuntime.T("PRESS AGAIN TO DELETE");
                return;
            }
            deleteConfirmArmed = false;
            SupabaseAuthClient auth = FindFirstObjectByType<SupabaseAuthClient>();
            if (auth == null)
            {
                if (deleteButtonText != null) deleteButtonText.text = FspLocalizationRuntime.T("DELETE FAILED");
                return;
            }
            StartCoroutine(auth.DeleteAccount((ok, error) =>
            {
                if (deleteButtonText != null)
                    deleteButtonText.text = FspLocalizationRuntime.T(ok ? "ACCOUNT DELETED" : "DELETE FAILED");
                if (ok)
                {
                    SquadLobbyState.Instance?.Clear();
                    MatchRoomState.Instance?.Clear();
                    RefreshConnectionState();
                    RefreshAccountPanel();
                }
                if (!ok && !string.IsNullOrWhiteSpace(error)) Debug.LogWarning("FSP account deletion failed: " + error);
            }));
        }

        private void StartMatch()
        {
            LobbyState state = LobbyState.Instance;
            if (state == null) return;
            if (string.IsNullOrWhiteSpace(state.DisplayName)) state.SetDisplayName("Player");

            if (state.SelectedMode == MatchMode.Squad && SupabaseSession.IsSignedIn)
            {
                SquadLobbyController controller = FindFirstObjectByType<SquadLobbyController>();
                if (controller == null) { OpenTeam(); return; }
                controller.StartSquadMatchmaking((ok, status) =>
                {
                    if (ok) state.RequestStartMatch();
                    else OpenTeam();
                });
                return;
            }
            state.RequestStartMatch();
        }

        private void PreviousCharacter()
        {
            string current = LobbyState.Instance != null ? LobbyState.Instance.SelectedCharacterId : "soldier_01";
            LobbyState.Instance?.SetCharacter(current == "soldier_01" ? "soldier_03" : current == "soldier_02" ? "soldier_01" : "soldier_02");
            PersistProfileSelection();
        }

        private void NextCharacter()
        {
            string current = LobbyState.Instance != null ? LobbyState.Instance.SelectedCharacterId : "soldier_01";
            LobbyState.Instance?.SetCharacter(current == "soldier_01" ? "soldier_02" : current == "soldier_02" ? "soldier_03" : "soldier_01");
            PersistProfileSelection();
        }

        private void RefreshLobbyState()
        {
            LobbyState state = LobbyState.Instance;
            if (state == null) return;
            if (modeText != null) modeText.text = FspLocalizationRuntime.T(state.SelectedMode == MatchMode.Solo ? "SOLO" : "SQUAD");
            if (characterText != null) characterText.text = FspLocalizationRuntime.T(state.SelectedCharacterId.Replace('_', ' ').ToUpperInvariant());
            if (profileNameText != null) profileNameText.text = string.IsNullOrWhiteSpace(state.DisplayName) ? FspLocalizationRuntime.T("PLAYER") : state.DisplayName;
            RefreshConnectionState();
        }

        private void RefreshConnectionState()
        {
            if (connectionText == null) return;
            bool reachable = Application.internetReachability != NetworkReachability.NotReachable;
            if (SupabaseSession.IsSignedIn && reachable)
                connectionText.text = FspLocalizationRuntime.T("ACCOUNT CONNECTED");
            else if (SupabaseSession.IsSignedIn)
                connectionText.text = FspLocalizationRuntime.T("ACCOUNT OFFLINE");
            else
                connectionText.text = FspLocalizationRuntime.T("GUEST • OFFLINE MATCH");
        }

        private void ToggleSettings()
        {
            if (settingsPanel == null) return;
            if (!settingsPanel.activeSelf && sectionPanel != null) sectionPanel.SetActive(false);
            if (!settingsPanel.activeSelf && authPanel != null) authPanel.SetActive(false);
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }

        private void ToggleMusic()
        {
            bool enabled = PlayerPrefs.GetInt("fsp_music", 1) == 1;
            PlayerPrefs.SetInt("fsp_music", enabled ? 0 : 1);
            ApplySavedSettings();
        }

        private void ToggleSfx()
        {
            bool enabled = PlayerPrefs.GetInt("fsp_sfx", 1) == 1;
            PlayerPrefs.SetInt("fsp_sfx", enabled ? 0 : 1);
            ApplySavedSettings();
        }

        private void CycleQuality()
        {
            int quality = (PlayerPrefs.GetInt("fsp_quality", 1) + 1) % 3;
            PlayerPrefs.SetInt("fsp_quality", quality);
            ApplySavedSettings();
        }

        private void CycleLanguage() => FspLocalizationRuntime.SelectNextLanguage();

        private void HandleLanguageChanged(FspLanguage language)
        {
            ApplySavedSettings();
            RefreshLobbyState();
        }

        private void ApplySavedSettings()
        {
            bool music = PlayerPrefs.GetInt("fsp_music", 1) == 1;
            bool sfx = PlayerPrefs.GetInt("fsp_sfx", 1) == 1;
            int quality = Mathf.Clamp(PlayerPrefs.GetInt("fsp_quality", 1), 0, 2);
            Fsp.Audio.FspAudioRuntime.ApplySettings();
            Fsp.Core.DeviceGraphicsConfigurator.ApplyPreset(quality);
            if (musicText != null) musicText.text = FspLocalizationRuntime.T("MUSIC") + ": " + FspLocalizationRuntime.T(music ? "ON" : "OFF");
            if (sfxText != null) sfxText.text = FspLocalizationRuntime.T("SFX") + ": " + FspLocalizationRuntime.T(sfx ? "ON" : "OFF");
            if (qualityText != null) qualityText.text = FspLocalizationRuntime.T("GRAPHICS") + ": " + FspLocalizationRuntime.T(quality == 0 ? "LOW" : quality == 1 ? "MEDIUM" : "HIGH");
            if (languageText != null) languageText.text = FspLocalizationRuntime.T("LANGUAGE") + ": " + FspLocalizationRuntime.CurrentLabel;
            PlayerPrefs.Save();
        }

        private static RectTransform Panel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return rt;
        }

        private static Text Label(Transform parent, string value, int size, Vector2 min, Vector2 max, TextAnchor anchor)
        {
            GameObject go = new("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            Text text = go.GetComponent<Text>();
            try { text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            text.text = FspLocalizationRuntime.T(value); text.fontSize = size; text.alignment = anchor; text.color = White;
            text.resizeTextForBestFit = true; text.resizeTextMinSize = 10; text.resizeTextMaxSize = size;
            text.raycastTarget = false;
            return text;
        }

        private static Button Button(Transform parent, string name, string value, Vector2 min, Vector2 max, Color color, UnityEngine.Events.UnityAction action, int size)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Fsp.Audio.FspUiClickAudio));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>(); image.color = color;
            Button button = go.GetComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(action);
            Label(go.transform, value, size, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
            return button;
        }
    }
}
