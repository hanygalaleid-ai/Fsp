using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    /// <summary>Clean-build guard: removes all legacy FSP lobby artwork and keeps the approved BMG presentation only.</summary>
    public sealed class BmgCleanLobbyBackgroundRuntime : MonoBehaviour
    {
        private static BmgCleanLobbyBackgroundRuntime instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            var host = new GameObject("BMG_CleanLobbyBackgroundRuntime");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BmgCleanLobbyBackgroundRuntime>();
            SceneManager.sceneLoaded += instance.OnSceneLoaded;
            instance.StartCoroutine(instance.ApplyDelayed());
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StartCoroutine(ApplyDelayed());

        private IEnumerator ApplyDelayed()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", System.StringComparison.OrdinalIgnoreCase)) yield break;
            for (int i = 0; i < 8; i++)
            {
                yield return null;
                Apply();
            }
            yield return new WaitForSecondsRealtime(.5f);
            Apply();
        }

        private static void Apply()
        {
            GameObject legacy = GameObject.Find("FSP_FIXED_LOBBY_ART");
            if (legacy != null)
            {
                var sr = legacy.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = false;
                legacy.SetActive(false);
            }

            GameObject bg = GameObject.Find("LobbyBackground");
            if (bg != null)
            {
                var raw = bg.GetComponent<RawImage>();
                if (raw != null)
                {
                    raw.texture = null;
                    raw.color = new Color(.008f, .015f, .023f, 1f);
                    raw.raycastTarget = false;
                }
                var image = bg.GetComponent<Image>();
                if (image != null)
                {
                    image.sprite = null;
                    image.color = new Color(.008f, .015f, .023f, 1f);
                }
            }
        }
    }
}
