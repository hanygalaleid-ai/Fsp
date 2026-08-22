using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    /// <summary>Shows the approved realistic parachute render in the lobby appearance/wardrobe panel.</summary>
    public sealed class BmgParachutePreviewRuntime : MonoBehaviour
    {
        private static BmgParachutePreviewRuntime instance;
        private Texture2D texture;
        private RawImage preview;
        private float nextRefresh;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            var host = new GameObject("BMG_ParachutePreviewRuntime");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BmgParachutePreviewRuntime>();
            instance.texture = Resources.Load<Texture2D>("BMG/UI/bmg_parachute");
            SceneManager.sceneLoaded += instance.OnSceneLoaded;
            instance.StartCoroutine(instance.RefreshDelayed());
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            preview = null;
            StartCoroutine(RefreshDelayed());
        }

        private IEnumerator RefreshDelayed()
        {
            for (int i = 0; i < 10; i++) yield return null;
            Refresh();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + .35f;
            Refresh();
        }

        private void Refresh()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", System.StringComparison.OrdinalIgnoreCase)) return;
            if (texture == null) texture = Resources.Load<Texture2D>("BMG/UI/bmg_parachute");
            if (texture == null) return;

            GameObject modal = GameObject.Find("LobbySectionModal");
            bool show = modal != null && modal.activeInHierarchy && ContainsParachuteText(modal);
            if (!show)
            {
                if (preview != null) preview.gameObject.SetActive(false);
                return;
            }

            if (preview == null)
            {
                RectTransform panel = modal.GetComponent<RectTransform>();
                if (panel == null) return;
                GameObject go = new("BMG_ApprovedParachutePreview", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
                go.transform.SetParent(panel, false);
                RectTransform rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(.48f, .20f);
                rt.anchorMax = new Vector2(.94f, .72f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                AspectRatioFitter fit = go.GetComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                fit.aspectRatio = 16f / 9f;
                preview = go.GetComponent<RawImage>();
                preview.texture = texture;
                preview.color = Color.white;
                preview.raycastTarget = false;
            }
            preview.gameObject.SetActive(true);
            preview.transform.SetAsLastSibling();
        }

        private static bool ContainsParachuteText(GameObject root)
        {
            foreach (Text text in root.GetComponentsInChildren<Text>(true))
            {
                string value = (text.text ?? string.Empty).ToLowerInvariant();
                if (value.Contains("parachute") || value.Contains("مظلة") || value.Contains("المظلة")) return true;
            }
            return false;
        }
    }
}
