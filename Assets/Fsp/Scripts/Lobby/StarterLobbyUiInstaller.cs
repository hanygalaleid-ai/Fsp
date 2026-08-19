using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fsp.Lobby
{
    /// <summary>Release Sunscar lobby. Uses Resources art directly and never prototype geometry.</summary>
    public sealed class StarterLobbyUiInstaller : MonoBehaviour
    {
        private Font font;
        private LobbyState state;
        private Text modeText, characterText, statusText;
        private InputField nameInput;

        private static readonly Color Navy = new Color(0.018f,0.045f,0.078f,0.97f);
        private static readonly Color NavySoft = new Color(0.025f,0.060f,0.105f,0.90f);
        private static readonly Color Bronze = new Color(0.95f,0.43f,0.06f,1f);
        private static readonly Color BronzeLine = new Color(1.00f,0.61f,0.16f,1f);
        private static readonly Color WarmWhite = new Color(0.97f,0.95f,0.91f,1f);
        private static readonly Color Sand = new Color(0.76f,0.70f,0.61f,1f);

        private void Awake()
        {
            try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            EnsureEventSystem();
            BuildUi();
        }

        private void Start()
        {
            state = LobbyState.Instance;
            if (state == null) { if (statusText != null) statusText.text = "READY"; return; }
            if (nameInput != null)
            {
                nameInput.text = string.IsNullOrWhiteSpace(state.DisplayName) ? "Player" : state.DisplayName;
                state.SetDisplayName(nameInput.text);
                nameInput.onValueChanged.AddListener(state.SetDisplayName);
            }
            state.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy() { if (state != null) state.Changed -= Refresh; }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
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
            scaler.referenceResolution = new Vector2(1920f,1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject baseBg = Panel(canvasGo.transform,"BaseBackground",Vector2.zero,Vector2.one,new Color(0.018f,0.025f,0.035f,1f));
            baseBg.GetComponent<Image>().raycastTarget = false;

            Texture2D art = Resources.Load<Texture2D>("Lobby/lobby_reference");
            if (art != null)
            {
                GameObject artGo = new GameObject("SunscarBackdrop", typeof(RectTransform), typeof(RawImage));
                artGo.transform.SetParent(canvasGo.transform,false);
                RectTransform artRt = artGo.GetComponent<RectTransform>();
                artRt.anchorMin = Vector2.zero; artRt.anchorMax = Vector2.one; artRt.offsetMin = Vector2.zero; artRt.offsetMax = Vector2.zero;
                RawImage raw = artGo.GetComponent<RawImage>();
                raw.texture = art; raw.color = Color.white; raw.raycastTarget = false;
            }
            else Debug.LogError("SUNSCAR LOBBY ART MISSING AT RUNTIME: Lobby/lobby_reference");

            GameObject shade = Panel(canvasGo.transform,"CinematicShade",Vector2.zero,Vector2.one,new Color(0f,0f,0f,0.24f));
            shade.GetComponent<Image>().raycastTarget = false;

            Label(shade.transform,"Brand","SUNSCAR",new Vector2(0.035f,0.91f),new Vector2(0.42f,0.985f),46,WarmWhite,TextAnchor.MiddleLeft);
            Label(shade.transform,"Tag","BATTLE ROYALE",new Vector2(0.035f,0.865f),new Vector2(0.30f,0.915f),18,BronzeLine,TextAnchor.MiddleLeft);

            GameObject profile = FramedPanel(shade.transform,"Profile",new Vector2(0.035f,0.68f),new Vector2(0.32f,0.84f));
            Label(profile.transform,"Title","OPERATIVE",new Vector2(0.055f,0.53f),new Vector2(0.95f,0.92f),30,WarmWhite,TextAnchor.MiddleLeft);
            Label(profile.transform,"Rank","RANK 01",new Vector2(0.055f,0.12f),new Vector2(0.95f,0.48f),19,Sand,TextAnchor.MiddleLeft);

            GameObject namePanel = FramedPanel(shade.transform,"NamePanel",new Vector2(0.035f,0.56f),new Vector2(0.32f,0.655f));
            nameInput = namePanel.AddComponent<InputField>();
            Text nameText = Label(namePanel.transform,"NameText","Player",new Vector2(0.06f,0.08f),new Vector2(0.94f,0.92f),24,WarmWhite,TextAnchor.MiddleLeft);
            nameInput.textComponent = nameText; nameInput.targetGraphic = namePanel.GetComponent<Image>(); nameInput.characterLimit = 18;

            GameObject hero = FramedPanel(shade.transform,"HeroCard",new Vector2(0.035f,0.30f),new Vector2(0.32f,0.53f));
            characterText = Label(hero.transform,"Character","SOLDIER_01",new Vector2(0.18f,0.54f),new Vector2(0.82f,0.90f),23,WarmWhite,TextAnchor.MiddleCenter);
            MakeButton(hero.transform,"Prev","<",new Vector2(0.04f,0.10f),new Vector2(0.30f,0.44f),Navy,PreviousCharacter,22);
            MakeButton(hero.transform,"Next",">",new Vector2(0.70f,0.10f),new Vector2(0.96f,0.44f),Navy,NextCharacter,22);

            GameObject match = FramedPanel(shade.transform,"MatchCard",new Vector2(0.66f,0.20f),new Vector2(0.965f,0.47f));
            Label(match.transform,"Map","SUNSCAR ISLAND",new Vector2(0.06f,0.66f),new Vector2(0.94f,0.93f),28,WarmWhite,TextAnchor.MiddleLeft);
            Label(match.transform,"MapSub","BATTLE ROYALE",new Vector2(0.06f,0.45f),new Vector2(0.94f,0.65f),18,Sand,TextAnchor.MiddleLeft);
            modeText = Label(match.transform,"Mode","SOLO",new Vector2(0.06f,0.08f),new Vector2(0.34f,0.36f),22,WarmWhite,TextAnchor.MiddleCenter);
            MakeButton(match.transform,"Solo","SOLO",new Vector2(0.38f,0.07f),new Vector2(0.64f,0.38f),Navy,()=>state?.SetMode(MatchMode.Solo));
            MakeButton(match.transform,"Squad","SQUAD",new Vector2(0.67f,0.07f),new Vector2(0.94f,0.38f),Navy,()=>state?.SetMode(MatchMode.Squad));

            MakeButton(shade.transform,"Start","START",new Vector2(0.66f,0.055f),new Vector2(0.965f,0.18f),Bronze,StartMatch,34);
            statusText = Label(shade.transform,"Status","READY",new Vector2(0.035f,0.04f),new Vector2(0.30f,0.095f),18,Sand,TextAnchor.MiddleLeft);
        }

        private GameObject FramedPanel(Transform parent,string name,Vector2 min,Vector2 max,bool strong=true)
        {
            GameObject panel=Panel(parent,name,min,max,strong?NavySoft:new Color(0.025f,0.06f,0.105f,0.78f));
            Outline o=panel.AddComponent<Outline>(); o.effectColor=strong?BronzeLine:new Color(BronzeLine.r,BronzeLine.g,BronzeLine.b,0.55f); o.effectDistance=new Vector2(2f,-2f); return panel;
        }
        private GameObject Panel(Transform parent,string name,Vector2 min,Vector2 max,Color color)
        {
            GameObject go=new GameObject(name,typeof(RectTransform),typeof(Image)); go.transform.SetParent(parent,false); RectTransform rt=go.GetComponent<RectTransform>(); rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; go.GetComponent<Image>().color=color; return go;
        }
        private Text Label(Transform parent,string name,string value,Vector2 min,Vector2 max,int size,Color color,TextAnchor align)
        {
            GameObject go=new GameObject(name,typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false); RectTransform rt=go.GetComponent<RectTransform>(); rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; Text t=go.GetComponent<Text>(); t.font=font; t.text=value; t.fontSize=size; t.color=color; t.alignment=align; t.resizeTextForBestFit=true; t.resizeTextMinSize=12; t.resizeTextMaxSize=size; t.raycastTarget=false; return t;
        }
        private Button MakeButton(Transform parent,string name,string text,Vector2 min,Vector2 max,Color color,UnityEngine.Events.UnityAction action,int fontSize=22)
        {
            GameObject go=Panel(parent,name,min,max,color); Outline o=go.AddComponent<Outline>(); o.effectColor=new Color(BronzeLine.r,BronzeLine.g,BronzeLine.b,0.75f); o.effectDistance=new Vector2(1.5f,-1.5f); Button b=go.AddComponent<Button>(); if(action!=null)b.onClick.AddListener(action); Label(go.transform,"Text",text,Vector2.zero,Vector2.one,fontSize,WarmWhite,TextAnchor.MiddleCenter); return b;
        }
        private void PreviousCharacter(){if(state==null)return; string id=state.SelectedCharacterId=="soldier_01"?"soldier_03":state.SelectedCharacterId=="soldier_02"?"soldier_01":"soldier_02";state.SetCharacter(id);}
        private void NextCharacter(){if(state==null)return; string id=state.SelectedCharacterId=="soldier_01"?"soldier_02":state.SelectedCharacterId=="soldier_02"?"soldier_03":"soldier_01";state.SetCharacter(id);}
        private void StartMatch(){if(state==null){UnityEngine.SceneManagement.SceneManager.LoadScene("Match");return;} if(string.IsNullOrWhiteSpace(state.DisplayName))state.SetDisplayName("Player");state.RequestStartMatch();}
        private void Refresh(){if(state==null)return; if(modeText!=null)modeText.text=state.SelectedMode==MatchMode.Solo?"SOLO":"SQUAD";if(characterText!=null)characterText.text=state.SelectedCharacterId.ToUpperInvariant();if(statusText!=null)statusText.text="READY  •  "+(state.SelectedMode==MatchMode.Solo?"SOLO":"SQUAD");}
    }
}
