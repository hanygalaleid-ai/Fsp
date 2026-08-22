using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    /// <summary>Strict BMG lobby background. Keeps only the approved full-screen background and purges obsolete runtime overlays.</summary>
    public sealed class BmgCleanLobbyBackgroundRuntime : MonoBehaviour
    {
        private static BmgCleanLobbyBackgroundRuntime instance;
        private Texture2D modernLobby;
        private RawImage runtimeBackground;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            var host = new GameObject("BMG_CleanLobbyBackgroundRuntime");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BmgCleanLobbyBackgroundRuntime>();
            instance.modernLobby = Resources.Load<Texture2D>("BMG/UI/bmg_lobby_modern");
            SceneManager.sceneLoaded += instance.OnSceneLoaded;
            instance.StartCoroutine(instance.ApplyDelayed());
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            runtimeBackground = null;
            StartCoroutine(ApplyDelayed());
        }

        private IEnumerator ApplyDelayed()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", System.StringComparison.OrdinalIgnoreCase)) yield break;
            for (int i = 0; i < 12; i++)
            {
                yield return null;
                Apply();
            }
            yield return new WaitForSecondsRealtime(.75f);
            Apply();
        }

        private static RectTransform FindLobbyRoot()
        {
            GameObject safe = GameObject.Find("SafeRoot");
            if (safe != null)
            {
                RectTransform rt = safe.GetComponent<RectTransform>();
                if (rt != null) return rt;
            }

            GameObject canvas = GameObject.Find("ProductionLobbyCanvas");
            if (canvas != null)
            {
                RectTransform rt = canvas.GetComponent<RectTransform>();
                if (rt != null) return rt;
            }

            Canvas anyCanvas = FindFirstObjectByType<Canvas>();
            return anyCanvas != null ? anyCanvas.GetComponent<RectTransform>() : null;
        }

        private static void PurgeObsoleteOverlay(string objectName)
        {
            GameObject go = GameObject.Find(objectName);
            if (go != null) Destroy(go);
        }

        private void Apply()
        {
            PurgeObsoleteOverlay("BMG_RealisticLogo");
            PurgeObsoleteOverlay("BMG_RealisticCharacterPreview");

            GameObject legacy = GameObject.Find("FSP_FIXED_LOBBY_ART");
            if (legacy != null) legacy.SetActive(false);

            if (modernLobby == null)
                modernLobby = Resources.Load<Texture2D>("BMG/UI/bmg_lobby_modern");
            if (modernLobby == null) return;

            RectTransform root = FindLobbyRoot();
            if (root == null) return;

            if (runtimeBackground == null)
            {
                GameObject existing = GameObject.Find("BMG_ModernLobbyBackground");
                if (existing != null) runtimeBackground = existing.GetComponent<RawImage>();
            }

            if (runtimeBackground == null)
            {
                GameObject bgObject = new("BMG_ModernLobbyBackground", typeof(RectTransform), typeof(RawImage));
                bgObject.transform.SetParent(root, false);
                RectTransform rt = bgObject.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                runtimeBackground = bgObject.GetComponent<RawImage>();
            }

            runtimeBackground.texture = modernLobby;
            runtimeBackground.color = Color.white;
            runtimeBackground.uvRect = new Rect(0f, 0f, 1f, 1f);
            runtimeBackground.raycastTarget = false;
            runtimeBackground.transform.SetAsFirstSibling();

            GameObject oldBackground = GameObject.Find("LobbyBackground");
            if (oldBackground != null && oldBackground != runtimeBackground.gameObject)
            {
                Image image = oldBackground.GetComponent<Image>();
                if (image != null) image.enabled = false;
                RawImage raw = oldBackground.GetComponent<RawImage>();
                if (raw != null) raw.enabled = false;
            }
        }
    }
}
