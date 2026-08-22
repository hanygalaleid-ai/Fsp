using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    /// <summary>Strict BMG lobby background: legacy FSP art is disabled and the approved modern BMG art is used.</summary>
    public sealed class BmgCleanLobbyBackgroundRuntime : MonoBehaviour
    {
        private static BmgCleanLobbyBackgroundRuntime instance;
        private Texture2D modernLobby;

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

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StartCoroutine(ApplyDelayed());

        private IEnumerator ApplyDelayed()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", System.StringComparison.OrdinalIgnoreCase)) yield break;
            for (int i = 0; i < 10; i++)
            {
                yield return null;
                Apply();
            }
            yield return new WaitForSecondsRealtime(.5f);
            Apply();
        }

        private void Apply()
        {
            GameObject legacy = GameObject.Find("FSP_FIXED_LOBBY_ART");
            if (legacy != null)
            {
                foreach (var renderer in legacy.GetComponentsInChildren<Renderer>(true))
                    if (renderer != null) renderer.enabled = false;
                legacy.SetActive(false);
            }

            if (modernLobby == null)
                modernLobby = Resources.Load<Texture2D>("BMG/UI/bmg_lobby_modern");

            GameObject bg = GameObject.Find("LobbyBackground");
            if (bg == null || modernLobby == null) return;

            var raw = bg.GetComponent<RawImage>();
            if (raw == null) raw = bg.AddComponent<RawImage>();
            raw.texture = modernLobby;
            raw.color = Color.white;
            raw.uvRect = new Rect(0f, 0f, 1f, 1f);
            raw.raycastTarget = false;

            var image = bg.GetComponent<Image>();
            if (image != null) image.enabled = false;

            bg.transform.SetAsFirstSibling();
        }
    }
}
