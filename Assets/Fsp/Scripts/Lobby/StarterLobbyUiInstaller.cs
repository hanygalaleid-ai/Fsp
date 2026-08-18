using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fsp.Lobby
{
    /// <summary>
    /// Release lobby. It always renders the fixed Sunscar art/UI and never creates prototype 3D geometry.
    /// </summary>
    public sealed class StarterLobbyUiInstaller : MonoBehaviour
    {
        private Font font;
        private LobbyState state;
        private Text modeText;
        private Text characterText;
        private Text statusText;
        private InputField nameInput;

        private static readonly Color Navy = new Color(0.018f, 0.045f, 0.078f, 0.97f);
        private static readonly Color NavySoft = new Color(0.025f, 0.060f, 0.105f, 0.90f);
        private static readonly Color Bronze = new Color(0.95f, 0.43f, 0.06f, 1f);
        private static readonly Color BronzeLine = new Color(1.00f, 0.61f, 0.16f, 1f);
        private static readonly Color WarmWhite = new Color(0.97f, 0.95f, 0.91f, 1f);
        private static readonly Color Sand = new Color(0.76f, 0.70f, 0.61f, 1f);

        private void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            EnsureEventSystem();
            BuildUi();
        }

        private void Start()
        {
            state = LobbyState.Instance;
            if (state == null)
            {
                if (statusText != null) statusText.text = "OFFLINE";
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
            if (state != null) state.Changed -= Refresh;
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private void BuildUi()
        {
            GameObject old = GameObject.Find("LobbyCanvas");
            if (old != null) Destroy(old);

            GameObject canvasGo = new GameObject("LobbyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject bg = Panel(canvasGo.transform, "SunscarBackdrop", Vector2.zero, Vector2.one, new Color(0.018f,0.025f,0.035f,1f));
            Texture2D art = Resources.Load<Texture2D>("Lobby/lobby_reference");
            if (art != null)
            {
                Image image = bg.GetComponent<Image>();
                image.sprite = Sprite.Create(art, new Rect(0, 0, art.width, art.height), new Vector2(0.5f,0.5f), 100f);
                image.preserveAspect = false;
                image.color = Color.white;
            }

            GameObject shade = Panel(bg.transform, "CinematicShade", Vector2.zero, Vector2.one, new Color(0f,0f,0f,0.24f));
            GameObject topGlow = Panel(shade.transform, "TopGlow", new Vector2(0f,0.94f), new Vector2(1f,1f), new Color(0.95f,0.35f,0.05f,0.12f));
            topGlow.GetComponent<Image>().raycastTarget = false;

            GameObject profile = FramedPanel(shade.transform, "Profile", new Vector2(0.035f,0.78f), new Vector2(0.32f,0.95f));
            Label(profile.transform,"Title","SUNSCAR // OPERATIVE",new Vector2(0.055f,0.53f),new Vector2(0.95f,0.92f),32,WarmWhite,TextAnchor.MiddleLeft);
            Label(profile.transform,"Rank","RANK 01   •   SUNSCAR",new Vector2(0.055f,0.12f),new Vector2(0.95f,0.48f),19,Sand,TextAnchor.MiddleLeft);

            GameObject namePanel = FramedPanel(shade.transform,"NamePanel",new Vector2(0.035f,0.665f),new Vector2(0.32f,0.765f));
            nameInput = namePanel.AddComponent<InputField>();
            Text nameText = Label(namePanel.transform,"NameText","Player",new Vector2(0.06f,0.08f),new Vector2(0.94f,0.92f),24,WarmWhite,TextAnchor.MiddleLeft);
            nameInput.textComponent = nameText;
            nameInput.targetGraphic = namePanel.GetComponent<Image>();
            nameInput.characterLimit = 18;

            GameObject nav = FramedPanel(shade.transform,"Nav",new Vector2(0.035f,0.22f),new Vector2(0.19f,0.60f));
            NavButton(nav.transform,"LOADOUT",0.69f);
            NavButton(nav.transform,"APPEARANCE",0.40f);
            NavButton(nav.transform,"CAREER",0.11f);

            GameObject heroBadge = FramedPanel(shade.transform,"HeroCard",new Vector2(0.235f,0.10f),new Vector2(0.52f,0.245f));
            characterText = Label(heroBadge.transform,"Character","SOLDIER_01",new Vector2(0.18f,0.48f),new Vector2(0.82f,0.90f),23,WarmWhite,TextAnchor.MiddleCenter);
            MakeButton(heroBadge.transform,"Prev","‹",new Vector2(0.02f,0.10f),new Vector2(0.19f,0.46f),Navy,PreviousCharacter);
            MakeButton(heroBadge.transform,"Next","›",new Vector2(0.81f,0.10f),new Vector2(0.98f,0.46f),Navy,NextCharacter);

            GameObject squad = FramedPanel(shade.transform,"Squad",new Vector2(0.72f,0.48f),new Vector2(0.975f,0.94f));
            Label(squad.transform,"SquadTitle","SQUAD",new Vector2(0.07f,0.84f),new Vector2(0.93f,0.96f),29,WarmWhite,TextAnchor.MiddleLeft);
            for (int i=0;i<4;i++)
            {
                GameObject slot = FramedPanel(squad.transform,"Slot"+i,new Vector2(0.07f,0.65f-i*0.16f),new Vector2(0.93f,0.77f-i*0.16f), false);
                Label(slot.transform,"SlotText",i==0?"YOU   READY":"+ INVITE",new Vector2(0.08f,0.05f),new Vector2(0.92f,0.95f),20,i==0?WarmWhite:Sand,TextAnchor.MiddleLeft);
            }

            GameObject match = FramedPanel(shade.transform,"MatchCard",new Vector2(0.66f,0.19f),new Vector2(0.975f,0.44f));
            Label(match.transform,"Map","SUNSCAR ISLAND",new Vector2(0.06f,0.63f),new Vector2(0.94f,0.93f),28,WarmWhite,TextAnchor.MiddleLeft);
            Label(match.transform,"MapSub","BATTLE ROYALE  •  REGION AUTO",new Vector2(0.06f,0.42f),new Vector2(0.94f,0.64f),18,Sand,TextAnchor.MiddleLeft);
            modeText = Label(match.transform,"Mode","SOLO",new Vector2(0.06f,0.08f),new Vector2(0.34f,0.36f),22,WarmWhite,TextAnchor.MiddleCenter);
            MakeButton(match.transform,"Solo","SOLO",new Vector2(0.38f,0.07f),new Vector2(0.64f,0.38f),Navy,()=>state?.SetMode(MatchMode.Solo));
            MakeButton(match.transform,"Squad","SQUAD",new Vector2(0.67f,0.07f),new Vector2(0.94f,0.38f),Navy,()=>state?.SetMode(MatchMode.Squad));

            MakeButton(shade.transform,"Start","START",new Vector2(0.72f,0.045f),new Vector2(0.975f,0.165f),Bronze,StartMatch,34);
            statusText = Label(shade.transform,"Status","ONLINE  •  READY",new Vector2(0.035f,0.035f),new Vector2(0.34f,0.095f),18,Sand,TextAnchor.MiddleLeft);
        }

        private GameObject FramedPanel(Transform parent,string name,Vector2 min,Vector2 max,bool strong=true)
        {
            GameObject panel = Panel(parent,name,min,max,strong?NavySoft:new Color(0.025f,0.06f,0.105f,0.78f));
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = strong ? BronzeLine : new Color(BronzeLine.r,BronzeLine.g,BronzeLine.b,0.55f);
            outline.effectDistance = new Vector2(2f,-2f);
            return panel;
        }

        private GameObject Panel(Transform parent,string name,Vector2 min,Vector2 max,Color color)
        {
            GameObject go = new GameObject(name,typeof(RectTransform),typeof(Image));
            go.transform.SetParent(parent,false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
            go.GetComponent<Image>().color=color;
            return go;
        }

        private Text Label(Transform parent,string name,string value,Vector2 min,Vector2 max,int size,Color color,TextAnchor align)
        {
            GameObject go = new GameObject(name,typeof(RectTransform),typeof(Text));
            go.transform.SetParent(parent,false);
            RectTransform rt=go.GetComponent<RectTransform>();
            rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
            Text t=go.GetComponent<Text>();
            t.font=font; t.text=value; t.fontSize=size; t.color=color; t.alignment=align;
            t.resizeTextForBestFit=true; t.resizeTextMinSize=12; t.resizeTextMaxSize=size;
            return t;
        }

        private void NavButton(Transform parent,string text,float y)
        {
            MakeButton(parent,text,text,new Vector2(0.07f,y),new Vector2(0.93f,y+0.20f),Navy,null,20);
        }

        private Button MakeButton(Transform parent,string name,string text,Vector2 min,Vector2 max,Color color,UnityEngine.Events.UnityAction action,int fontSize=22)
        {
            GameObject go=Panel(parent,name,min,max,color);
            Outline outline=go.AddComponent<Outline>();
            outline.effectColor=new Color(BronzeLine.r,BronzeLine.g,BronzeLine.b,0.75f);
            outline.effectDistance=new Vector2(1.5f,-1.5f);
            Button button=go.AddComponent<Button>();
            if(action!=null) button.onClick.AddListener(action);
            Label(go.transform,"Text",text,Vector2.zero,Vector2.one,fontSize,WarmWhite,TextAnchor.MiddleCenter);
            return button;
        }

        private void PreviousCharacter()
        {
            if(state==null)return;
            string id=state.SelectedCharacterId=="soldier_01"?"soldier_03":state.SelectedCharacterId=="soldier_02"?"soldier_01":"soldier_02";
            state.SetCharacter(id);
        }

        private void NextCharacter()
        {
            if(state==null)return;
            string id=state.SelectedCharacterId=="soldier_01"?"soldier_02":state.SelectedCharacterId=="soldier_02"?"soldier_03":"soldier_01";
            state.SetCharacter(id);
        }

        private void StartMatch()
        {
            if(state==null)return;
            if(string.IsNullOrWhiteSpace(state.DisplayName)) state.SetDisplayName("Player");
            state.RequestStartMatch();
        }

        private void Refresh()
        {
            if(state==null)return;
            if(modeText!=null) modeText.text=state.SelectedMode==MatchMode.Solo?"SOLO":"SQUAD";
            if(characterText!=null) characterText.text=state.SelectedCharacterId.ToUpperInvariant();
            if(statusText!=null) statusText.text="ONLINE  •  "+(state.SelectedMode==MatchMode.Solo?"SOLO READY":"SQUAD READY");
        }
    }
}
