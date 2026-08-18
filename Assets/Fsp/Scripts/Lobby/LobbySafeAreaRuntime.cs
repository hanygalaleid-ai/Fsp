using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Lobby
{
    /// <summary>Keeps edge-anchored lobby controls clear of notches and rounded screen cutouts.</summary>
    public sealed class LobbySafeAreaRuntime : MonoBehaviour
    {
        private RectTransform safeRoot;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;
        private float retryUntil;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", System.StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<LobbySafeAreaRuntime>() == null)
                new GameObject("LobbySafeAreaRuntime").AddComponent<LobbySafeAreaRuntime>();
        }

        private void Awake()
        {
            retryUntil = Time.unscaledTime + 10f;
        }

        private void Update()
        {
            if (safeRoot == null)
            {
                GameObject root = GameObject.Find("SafeRoot");
                if (root != null) safeRoot = root.GetComponent<RectTransform>();
                if (safeRoot == null)
                {
                    if (Time.unscaledTime > retryUntil) enabled = false;
                    return;
                }
            }

            if (Screen.width <= 0 || Screen.height <= 0) return;
            if (Screen.safeArea != lastSafeArea || Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
                Apply();
        }

        private void Apply()
        {
            Rect safe = Screen.safeArea;
            Vector2 min = safe.position;
            Vector2 max = safe.position + safe.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            safeRoot.anchorMin = min;
            safeRoot.anchorMax = max;
            safeRoot.offsetMin = Vector2.zero;
            safeRoot.offsetMax = Vector2.zero;

            lastSafeArea = safe;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
