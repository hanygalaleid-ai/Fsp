using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Lobby
{
    public sealed class LobbyBootSafetyRuntime : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", System.StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<LobbyBootSafetyRuntime>() == null)
                new GameObject("Fsp_LobbyBootSafety").AddComponent<LobbyBootSafetyRuntime>();
        }

        private IEnumerator Start()
        {
            yield return null;
            yield return null;
            if (GameObject.Find("LobbyCanvas") != null) yield break;
            BuildEmergencyLobby();
        }

        private static void BuildEmergencyLobby()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            GameObject canvasGo = new GameObject("LobbyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f,1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject bg = new GameObject("EmergencyBackground", typeof(RectTransform), typeof(RawImage));
            bg.transform.SetParent(canvasGo.transform,false);
            RectTransform br = bg.GetComponent<RectTransform>(); br.anchorMin=Vector2.zero;br.anchorMax=Vector2.one;br.offsetMin=Vector2.zero;br.offsetMax=Vector2.zero;
            RawImage raw=bg.GetComponent<RawImage>(); raw.texture=Resources.Load<Texture2D>("Lobby/lobby_reference"); raw.color=raw.texture!=null?Color.white:new Color(0.018f,0.025f,0.035f,1f); raw.raycastTarget=false;

            Font font=null; try{font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");}catch{}
            GameObject title=new GameObject("Title",typeof(RectTransform),typeof(Text)); title.transform.SetParent(canvasGo.transform,false);
            RectTransform tr=title.GetComponent<RectTransform>();tr.anchorMin=new Vector2(0.05f,0.78f);tr.anchorMax=new Vector2(0.55f,0.94f);tr.offsetMin=Vector2.zero;tr.offsetMax=Vector2.zero;
            Text tt=title.GetComponent<Text>();tt.font=font;tt.text="SUNSCAR";tt.fontSize=58;tt.alignment=TextAnchor.MiddleLeft;tt.color=new Color(0.97f,0.95f,0.91f,1f);tt.resizeTextForBestFit=true;

            GameObject btn=new GameObject("Start",typeof(RectTransform),typeof(Image),typeof(Button));btn.transform.SetParent(canvasGo.transform,false);
            RectTransform rr=btn.GetComponent<RectTransform>();rr.anchorMin=new Vector2(0.68f,0.07f);rr.anchorMax=new Vector2(0.95f,0.18f);rr.offsetMin=Vector2.zero;rr.offsetMax=Vector2.zero;
            btn.GetComponent<Image>().color=new Color(0.95f,0.43f,0.06f,1f);
            Button b=btn.GetComponent<Button>();b.onClick.AddListener(()=>{ if(Application.CanStreamedLevelBeLoaded("Match"))SceneManager.LoadScene("Match"); });
            GameObject label=new GameObject("Text",typeof(RectTransform),typeof(Text));label.transform.SetParent(btn.transform,false);
            RectTransform lr=label.GetComponent<RectTransform>();lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=Vector2.zero;lr.offsetMax=Vector2.zero;
            Text lt=label.GetComponent<Text>();lt.font=font;lt.text="START";lt.fontSize=34;lt.alignment=TextAnchor.MiddleCenter;lt.color=Color.white;lt.raycastTarget=false;
            Debug.LogError("Primary lobby UI was absent; Sunscar emergency lobby activated.");
        }
    }
}
