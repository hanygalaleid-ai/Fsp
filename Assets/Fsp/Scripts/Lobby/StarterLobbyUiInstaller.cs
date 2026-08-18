using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fsp.Lobby
{
    /// <summary>
    /// Builds the starter lobby runtime UI. Checked-in lobby art is the release visual source of truth;
    /// primitive backdrop/hero generation is reserved for emergency recovery when that art is missing.
    /// </summary>
    public sealed class StarterLobbyUiInstaller : MonoBehaviour
    {
        private Font font;
        private LobbyState state;
        private Text modeText;
        private Text characterText;
        private Text statusText;
        private InputField nameInput;
        private GameObject heroRoot;

        private static readonly Color Navy = new Color(0.025f, 0.055f, 0.10f, 0.94f);
        private static readonly Color NavySoft = new Color(0.035f, 0.075f, 0.13f, 0.82f);
        private static readonly Color Bronze = new Color(0.78f, 0.45f, 0.17f, 1f);
        private static readonly Color WarmWhite = new Color(0.96f, 0.93f, 0.86f, 1f);
        private static readonly Color Sand = new Color(0.72f, 0.66f, 0.54f, 1f);

        private void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            EnsureEventSystem();

            // Release rule: never put primitive fort/camp/character geometry in front of shipped lobby art.
            // Procedural visuals exist only so a corrupted/missing Resources asset cannot leave a blank scene.
            bool hasFixedLobbyArt = Resources.Load<Texture2D>("Lobby/lobby_reference") != null;
            if (!hasFixedLobbyArt)
            {
                BuildBackdrop();
                BuildHero();
                Debug.LogWarning("Fsp fixed lobby art is missing; emergency procedural lobby visuals were enabled.");
            }

            BuildUi();
        }

        private void Start()
        {
            state = LobbyState.Instance;
            if (state == null)
            {
                if (statusText != null) statusText.text = "Lobby state unavailable";
                return;
            }

            if (nameInput != null)
            {
                nameInput.text = string.IsNullOrWhiteSpace(state.DisplayName) ? "Player" : state.DisplayName;
                state.SetDisplayName(nameInput.text);
                nameInput.onValueChanged.AddListener(state.SetDisplayName);
            }
            state.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (state != null)
                state.Changed -= Refresh;
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private void BuildBackdrop()
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.22f, 0.22f, 0.22f);
            RenderSettings.fogDensity = 0.006f;
            RenderSettings.ambientLight = new Color(0.34f, 0.30f, 0.25f);

            CreateBlock("FortSilhouette", new Vector3(8f, 2.1f, 10f), new Vector3(10f, 4.2f, 2f), new Color(0.22f, 0.17f, 0.13f));
            CreateBlock("CampLeft", new Vector3(-7f, 1f, 6f), new Vector3(4f, 2f, 3f), new Color(0.34f, 0.25f, 0.17f));
            CreateBlock("CampRight", new Vector3(5.5f, 0.8f, 5f), new Vector3(3f, 1.6f, 2.2f), new Color(0.31f, 0.23f, 0.16f));
            CreateBlock("Antenna", new Vector3(10f, 4.5f, 8f), new Vector3(0.15f, 9f, 0.15f), Bronze * 0.7f);
        }

        private void BuildHero()
        {
            heroRoot = new GameObject("LobbyHero_Procedural");
            heroRoot.transform.position = new Vector3(-1.25f, 0.15f, 0f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(heroRoot.transform, false);
            body.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            body.transform.localScale = new Vector3(0.78f, 1.05f, 0.58f);
            Tint(body, new Color(0.12f, 0.18f, 0.20f));

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(heroRoot.transform, false);
            head.transform.localPosition = new Vector3(0f, 2.65f, 0f);
            head.transform.localScale = Vector3.one * 0.55f;
            Tint(head, new Color(0.55f, 0.40f, 0.29f));

            CreateLimb("ArmL", new Vector3(-0.58f, 1.62f, 0f), new Vector3(0.25f, 0.85f, 0.25f));
            CreateLimb("ArmR", new Vector3(0.58f, 1.62f, 0f), new Vector3(0.25f, 0.85f, 0.25f));
            CreateLimb("LegL", new Vector3(-0.25f, 0.45f, 0f), new Vector3(0.32f, 0.95f, 0.32f));
            CreateLimb("LegR", new Vector3(0.25f, 0.45f, 0f), new Vector3(0.32f, 0.95f, 0.32f));
        }

        private void CreateLimb(string name, Vector3 position, Vector3 scale)
        {
            var limb = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            limb.name = name;
            limb.transform.SetParent(heroRoot.transform, false);
            limb.transform.localPosition = position;
            limb.transform.localScale = scale;
            Tint(limb, new Color(0.10f, 0.15f, 0.17f));
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("LobbyCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.GetComponent<CanvasScaler>().matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var root = Panel(canvasGo.transform, "SafeRoot", Vector2.zero, Vector2.one, NavySoft);
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.18f);

            var profile = Panel(root.transform, "Profile", new Vector2(0.025f, 0.79f), new Vector2(0.31f, 0.96f), NavySoft);
            Label(profile.transform, "TITLE", "FSP // OPERATIVE", new Vector2(0.05f, 0.56f), new Vector2(0.95f, 0.93f), 31, WarmWhite, TextAnchor.MiddleLeft);
            Label(profile.transform, "Rank", "RANK 01   •   SUNSCAR", new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.48f), 20, Sand, TextAnchor.MiddleLeft);

            var namePanel = Panel(root.transform, "NamePanel", new Vector2(0.025f, 0.67f), new Vector2(0.31f, 0.775f), NavySoft);
            nameInput = namePanel.AddComponent<InputField>();
            var nameText = Label(namePanel.transform, "NameText", "Player", new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.92f), 25, WarmWhite, TextAnchor.MiddleLeft);
            nameInput.textComponent = nameText;
            nameInput.targetGraphic = namePanel.GetComponent<Image>();
            nameInput.characterLimit = 18;

            var nav = Panel(root.transform, "Nav", new Vector2(0.025f, 0.22f), new Vector2(0.17f, 0.61f), NavySoft);
            NavButton(nav.transform, "LOADOUT", 0.69f);
            NavButton(nav.transform, "APPEARANCE", 0.40f);
            NavButton(nav.transform, "CAREER", 0.11f);

            var squad = Panel(root.transform, "Squad", new Vector2(0.72f, 0.48f), new Vector2(0.975f, 0.94f), NavySoft);
            Label(squad.transform, "SquadTitle", "SQUAD", new Vector2(0.07f, 0.84f), new Vector2(0.93f, 0.96f), 29, WarmWhite, TextAnchor.MiddleLeft);
            for (int i = 0; i < 4; i++)
            {
                var slot = Panel(squad.transform, "Slot" + i, new Vector2(0.07f, 0.65f - i * 0.16f), new Vector2(0.93f, 0.77f - i * 0.16f), new Color(0.06f, 0.10f, 0.15f, 0.95f));
                Label(slot.transform, "SlotText", i == 0 ? "YOU   READY" : "+ INVITE", new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.95f), 21, i == 0 ? WarmWhite : Sand, TextAnchor.MiddleLeft);
            }

            var match = Panel(root.transform, "MatchCard", new Vector2(0.66f, 0.20f), new Vector2(0.975f, 0.44f), NavySoft);
            Label(match.transform, "Map", "SUNSCAR ISLAND", new Vector2(0.06f, 0.63f), new Vector2(0.94f, 0.93f), 28, WarmWhite, TextAnchor.MiddleLeft);
            Label(match.transform, "MapSub", "BATTLE ROYALE  •  REGION AUTO", new Vector2(0.06f, 0.42f), new Vector2(0.94f, 0.64f), 18, Sand, TextAnchor.MiddleLeft);
            modeText = Label(match.transform, "Mode", "SOLO", new Vector2(0.06f, 0.08f), new Vector2(0.34f, 0.36f), 22, WarmWhite, TextAnchor.MiddleCenter);
            MakeButton(match.transform, "Solo", "SOLO", new Vector2(0.38f, 0.07f), new Vector2(0.64f, 0.38f), Navy, () => state?.SetMode(MatchMode.Solo));
            MakeButton(match.transform, "Squad", "SQUAD", new Vector2(0.67f, 0.07f), new Vector2(0.94f, 0.38f), Navy, () => state?.SetMode(MatchMode.Squad));

            var heroInfo = Panel(root.transform, "HeroInfo", new Vector2(0.20f, 0.12f), new Vector2(0.48f, 0.27f), NavySoft);
            characterText = Label(heroInfo.transform, "Character", "SOLDIER_01", new Vector2(0.12f, 0.48f), new Vector2(0.88f, 0.88f), 23, WarmWhite, TextAnchor.MiddleCenter);
            MakeButton(heroInfo.transform, "Prev", "‹", new Vector2(0.02f, 0.10f), new Vector2(0.22f, 0.47f), Navy, PreviousCharacter);
            MakeButton(heroInfo.transform, "Next", "›", new Vector2(0.78f, 0.10f), new Vector2(0.98f, 0.47f), Navy, NextCharacter);

            MakeButton(root.transform, "Start", "START", new Vector2(0.72f, 0.045f), new Vector2(0.975f, 0.17f), Bronze, StartMatch, 34);
            statusText = Label(root.transform, "Status", "ONLINE  •  PING --", new Vector2(0.025f, 0.04f), new Vector2(0.34f, 0.10f), 18, Sand, TextAnchor.MiddleLeft);
        }

        private void PreviousCharacter()
        {
            if (state == null) return;
            string id = state.SelectedCharacterId == "soldier_01" ? "soldier_03" : state.SelectedCharacterId == "soldier_02" ? "soldier_01" : "soldier_02";
            state.SetCharacter(id);
        }

        private void NextCharacter()
        {
            if (state == null) return;
            string id = state.SelectedCharacterId == "soldier_01" ? "soldier_02" : state.SelectedCharacterId == "soldier_02" ? "soldier_03" : "soldier_01";
            state.SetCharacter(id);
        }

        private void StartMatch()
        {
            if (state == null) return;
            if (string.IsNullOrWhiteSpace(state.DisplayName)) state.SetDisplayName("Player");
            state.RequestStartMatch();
        }

        private void Refresh()
        {
            if (state == null) return;
            if (modeText != null) modeText.text = state.SelectedMode == MatchMode.Solo ? "SOLO" : "SQUAD";
            if (characterText != null) characterText.text = state.SelectedCharacterId.ToUpperInvariant();
            if (statusText != null) statusText.text = "ONLINE  •  " + (state.SelectedMode == MatchMode.Solo ? "SOLO READY" : "SQUAD READY");
            if (heroRoot != null)
            {
                float hue = state.SelectedCharacterId == "soldier_02" ? 0.56f : state.SelectedCharacterId == "soldier_03" ? 0.08f : 0.48f;
                foreach (var r in heroRoot.GetComponentsInChildren<Renderer>())
                    if (r.gameObject.name != "Head") r.material.color = Color.HSVToRGB(hue, 0.55f, 0.55f);
            }
        }

        private GameObject Panel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private Text Label(Transform parent, string name, string text, Vector2 min, Vector2 max, int size, Color color, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var t = go.GetComponent<Text>();
            t.font = font; t.text = text; t.fontSize = size; t.color = color; t.alignment = anchor; t.resizeTextForBestFit = true; t.resizeTextMinSize = 12; t.resizeTextMaxSize = size;
            return t;
        }

        private void NavButton(Transform parent, string text, float y)
        {
            MakeButton(parent, text, text, new Vector2(0.07f, y), new Vector2(0.93f, y + 0.20f), new Color(0.05f, 0.09f, 0.14f, 0.95f), () => { }, 18);
        }

        private Button MakeButton(Transform parent, string name, string text, Vector2 min, Vector2 max, Color color, UnityEngine.Events.UnityAction action, int size = 22)
        {
            var go = Panel(parent, name, min, max, color);
            var button = go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            button.onClick.AddListener(action);
            Label(go.transform, "Label", text, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f), size, WarmWhite, TextAnchor.MiddleCenter);
            return button;
        }

        private void CreateBlock(string name, Vector3 position, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name; go.transform.position = position; go.transform.localScale = scale; Tint(go, color);
        }

        private static void Tint(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = color;
        }
    }
}
