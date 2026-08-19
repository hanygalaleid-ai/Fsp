#if UNITY_EDITOR
using System;
using System.IO;
using Fsp.BattleRoyale;
using Fsp.Lobby;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.EditorTools
{
    [InitializeOnLoad]
    public static class FspProjectBootstrap
    {
        private const string ScenesFolder = "Assets/Fsp/Scenes";
        private const string LobbyScene = ScenesFolder + "/Lobby.unity";
        private const string MatchScene = ScenesFolder + "/Match.unity";
        private static bool initializing;

        static FspProjectBootstrap()
        {
            if (Application.isBatchMode) EnsureProject();
            else EditorApplication.delayCall += EnsureProject;
        }

        [InitializeOnLoadMethod]
        private static void InitializeAfterDomainReload() { if (Application.isBatchMode) EnsureProject(); }

        [MenuItem("Fsp/Project/Rebuild Fixed Release Scenes")]
        public static void RebuildStarterScenes()
        {
            EnsureFolder(ScenesFolder);
            CreateLobbyScene(true);
            CreateMatchScene(true);
            ApplyBuildSettings();
            ApplyPlayerDefaults();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void EnsureProjectForBuild()
        {
            EnsureProject();
        }

        private static void EnsureProject()
        {
            if (initializing || EditorApplication.isPlayingOrWillChangePlaymode) return;
            initializing = true;
            try
            {
                EnsureFolder(ScenesFolder);
                // Scenes are baked at editor/build time. No runtime UI/style generator is required.
                if (!File.Exists(LobbyScene)) CreateLobbyScene(false);
                if (!File.Exists(MatchScene)) CreateMatchScene(false);
                ApplyBuildSettings();
                ApplyPlayerDefaults();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                if (!File.Exists(LobbyScene) || !File.Exists(MatchScene))
                    throw new InvalidOperationException("Required fixed release scenes are missing.");
                Debug.Log("FSP FIXED RELEASE SCENES READY: Lobby UI is baked; runtime style generation disabled.");
            }
            finally { initializing = false; }
        }

        private static void CreateLobbyScene(bool overwrite)
        {
            if (!overwrite && File.Exists(LobbyScene)) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Lobby";

            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.008f, 0.018f, 0.032f, 1f);
            cameraGo.AddComponent<AudioListener>();

            var state = new GameObject("LobbyState");
            state.AddComponent<LobbyState>();

            var runtime = new GameObject("LobbyRuntime");
            runtime.AddComponent<LobbyController>();
            runtime.AddComponent<LobbyMatchLauncher>();

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            BuildFixedLobbyUi();

            if (!EditorSceneManager.SaveScene(scene, LobbyScene))
                throw new IOException("Failed to save Lobby scene at " + LobbyScene);
        }

        private static void BuildFixedLobbyUi()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject canvasGo = new GameObject("LobbyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Color navy = new Color(0.018f,0.045f,0.078f,0.98f);
            Color navySoft = new Color(0.025f,0.060f,0.105f,0.92f);
            Color orange = new Color(0.95f,0.43f,0.06f,1f);
            Color line = new Color(1f,0.61f,0.16f,1f);
            Color white = new Color(0.97f,0.95f,0.91f,1f);
            Color sand = new Color(0.76f,0.70f,0.61f,1f);

            Panel(canvasGo.transform,"FixedBackdrop",Vector2.zero,Vector2.one,new Color(0.008f,0.018f,0.032f,1f));
            GameObject top = Panel(canvasGo.transform,"TopBar",new Vector2(0f,0.86f),Vector2.one,navy);
            Label(top.transform,"Brand","SUNSCAR",new Vector2(0.035f,0.18f),new Vector2(0.35f,0.92f),48,white,TextAnchor.MiddleLeft,font);
            Label(top.transform,"Subtitle","BATTLE ROYALE  •  MOBILE",new Vector2(0.36f,0.22f),new Vector2(0.72f,0.86f),20,sand,TextAnchor.MiddleLeft,font);
            Label(top.transform,"Status","ONLINE  •  READY",new Vector2(0.73f,0.22f),new Vector2(0.965f,0.86f),20,line,TextAnchor.MiddleRight,font);

            GameObject left = Framed(canvasGo.transform,"PlayerPanel",new Vector2(0.035f,0.18f),new Vector2(0.32f,0.81f),navySoft,line);
            Label(left.transform,"Operative","OPERATIVE",new Vector2(0.07f,0.82f),new Vector2(0.93f,0.95f),30,white,TextAnchor.MiddleLeft,font);
            Label(left.transform,"Name","PLAYER",new Vector2(0.07f,0.68f),new Vector2(0.93f,0.80f),25,line,TextAnchor.MiddleLeft,font);
            Label(left.transform,"Rank","RANK 01  •  ROOKIE",new Vector2(0.07f,0.57f),new Vector2(0.93f,0.68f),18,sand,TextAnchor.MiddleLeft,font);
            GameObject hero = Framed(left.transform,"OperativeCard",new Vector2(0.07f,0.18f),new Vector2(0.93f,0.54f),new Color(0.012f,0.028f,0.05f,1f),line);
            Label(hero.transform,"HeroMark","01",new Vector2(0.08f,0.20f),new Vector2(0.40f,0.82f),72,line,TextAnchor.MiddleCenter,font);
            Label(hero.transform,"HeroName","SOLDIER_01",new Vector2(0.42f,0.38f),new Vector2(0.94f,0.70f),24,white,TextAnchor.MiddleLeft,font);
            Label(left.transform,"Loadout","LOADOUT  •  APPEARANCE  •  CAREER",new Vector2(0.07f,0.04f),new Vector2(0.93f,0.15f),17,sand,TextAnchor.MiddleCenter,font);

            GameObject center = Framed(canvasGo.transform,"DeploymentPanel",new Vector2(0.35f,0.18f),new Vector2(0.64f,0.81f),new Color(0.012f,0.028f,0.05f,0.90f),line);
            Label(center.transform,"MapTitle","SUNSCAR ISLAND",new Vector2(0.08f,0.78f),new Vector2(0.92f,0.92f),31,white,TextAnchor.MiddleCenter,font);
            Label(center.transform,"MapSub","DESERT COAST  •  LAST SQUAD STANDING",new Vector2(0.08f,0.68f),new Vector2(0.92f,0.78f),17,sand,TextAnchor.MiddleCenter,font);
            Label(center.transform,"MapGlyph","◢  SUNSCAR  ◣",new Vector2(0.08f,0.30f),new Vector2(0.92f,0.66f),38,line,TextAnchor.MiddleCenter,font);
            Label(center.transform,"Mode","SOLO / SQUAD",new Vector2(0.08f,0.12f),new Vector2(0.92f,0.26f),23,white,TextAnchor.MiddleCenter,font);

            GameObject right = Framed(canvasGo.transform,"SquadPanel",new Vector2(0.67f,0.31f),new Vector2(0.965f,0.81f),navySoft,line);
            Label(right.transform,"SquadTitle","SQUAD",new Vector2(0.07f,0.80f),new Vector2(0.93f,0.94f),28,white,TextAnchor.MiddleLeft,font);
            Label(right.transform,"Slot1","01   PLAYER                 READY",new Vector2(0.07f,0.61f),new Vector2(0.93f,0.76f),19,line,TextAnchor.MiddleLeft,font);
            Label(right.transform,"Slot2","02   EMPTY SLOT",new Vector2(0.07f,0.43f),new Vector2(0.93f,0.58f),19,sand,TextAnchor.MiddleLeft,font);
            Label(right.transform,"Slot3","03   EMPTY SLOT",new Vector2(0.07f,0.25f),new Vector2(0.93f,0.40f),19,sand,TextAnchor.MiddleLeft,font);
            Label(right.transform,"Voice","VOICE  •  AUTO FILL",new Vector2(0.07f,0.06f),new Vector2(0.93f,0.18f),17,sand,TextAnchor.MiddleLeft,font);

            GameObject start = Framed(canvasGo.transform,"Start",new Vector2(0.67f,0.075f),new Vector2(0.965f,0.25f),orange,line);
            Button button = start.AddComponent<Button>();
            start.AddComponent<LobbyStartButton>();
            Label(start.transform,"Text","START",Vector2.zero,Vector2.one,40,white,TextAnchor.MiddleCenter,font);
            button.targetGraphic = start.GetComponent<Image>();
        }

        private static GameObject Panel(Transform parent,string name,Vector2 min,Vector2 max,Color color)
        {
            GameObject go = new GameObject(name,typeof(RectTransform),typeof(Image));
            go.transform.SetParent(parent,false);
            RectTransform rt = go.GetComponent<RectTransform>(); rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
            go.GetComponent<Image>().color=color;
            return go;
        }

        private static GameObject Framed(Transform parent,string name,Vector2 min,Vector2 max,Color color,Color line)
        {
            GameObject go=Panel(parent,name,min,max,color);
            Outline outline=go.AddComponent<Outline>(); outline.effectColor=line; outline.effectDistance=new Vector2(2f,-2f);
            return go;
        }

        private static Text Label(Transform parent,string name,string value,Vector2 min,Vector2 max,int size,Color color,TextAnchor align,Font font)
        {
            GameObject go=new GameObject(name,typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false);
            RectTransform rt=go.GetComponent<RectTransform>(); rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
            Text t=go.GetComponent<Text>(); t.font=font; t.text=value; t.fontSize=size; t.color=color; t.alignment=align; t.resizeTextForBestFit=true; t.resizeTextMinSize=12; t.resizeTextMaxSize=size; t.raycastTarget=false;
            return t;
        }

        private static void CreateMatchScene(bool overwrite)
        {
            if (!overwrite && File.Exists(MatchScene)) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Match";
            CreateCamera(new Vector3(0f,8f,-12f),new Vector3(24f,0f,0f));
            CreateSun();
            var systems=new GameObject("MatchSystems"); systems.AddComponent<MatchManager>(); systems.AddComponent<MatchSceneAssembler>();
            var ground=GameObject.CreatePrimitive(PrimitiveType.Plane); ground.name="Ground_Base"; ground.transform.localScale=new Vector3(240f,1f,240f);
            if (!EditorSceneManager.SaveScene(scene,MatchScene)) throw new IOException("Failed to save Match scene at "+MatchScene);
        }

        private static void CreateCamera(Vector3 position,Vector3 euler)
        {
            var go=new GameObject("Main Camera"); go.tag="MainCamera"; var camera=go.AddComponent<Camera>(); camera.fieldOfView=65f; camera.nearClipPlane=0.1f; camera.farClipPlane=2500f; go.transform.position=position; go.transform.eulerAngles=euler; go.AddComponent<AudioListener>();
        }

        private static void CreateSun()
        {
            var go=new GameObject("Sun"); var light=go.AddComponent<Light>(); light.type=LightType.Directional; light.intensity=1.05f; light.color=new Color(1f,0.86f,0.68f); light.shadows=LightShadows.Soft; go.transform.rotation=Quaternion.Euler(42f,-28f,0f);
        }

        private static void ApplyBuildSettings()
        {
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(LobbyScene,true),new EditorBuildSettingsScene(MatchScene,true)};
        }

        private static void ApplyPlayerDefaults()
        {
            PlayerSettings.companyName="Fsp Studio"; PlayerSettings.productName="Fsp"; PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft; PlayerSettings.runInBackground=false; PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts=path.Split('/'); string current=parts[0];
            for(int i=1;i<parts.Length;i++){string next=current+"/"+parts[i];if(!AssetDatabase.IsValidFolder(next))AssetDatabase.CreateFolder(current,parts[i]);current=next;}
        }
    }
}
#endif
