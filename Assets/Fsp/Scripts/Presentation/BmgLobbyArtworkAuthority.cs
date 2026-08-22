using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    /// <summary>
    /// Single authority for the production BMG lobby backdrop. It intentionally overrides any
    /// legacy/empty lobby background after the responsive lobby canvas has been constructed.
    /// </summary>
    public sealed class BmgLobbyArtworkAuthority : MonoBehaviour
    {
        private const string TexturePath = "BMG/UI/bmg_lobby_modern";
        private static BmgLobbyArtworkAuthority instance;
        private Texture2D lobbyTexture;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            var host = new GameObject("BMG_LobbyArtworkAuthority");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BmgLobbyArtworkAuthority>();
            SceneManager.sceneLoaded += instance.OnSceneLoaded;
            instance.StartCoroutine(instance.ApplyWhenReady());
        }

        private void Awake()
        {
            lobbyTexture = Resources.Load<Texture2D>(TexturePath);
            if (lobbyTexture == null)
                Debug.LogError("BMG lobby artwork authority: missing Resources/" + TexturePath);
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (string.Equals(scene.name, "Lobby", StringComparison.OrdinalIgnoreCase))
                StartCoroutine(ApplyWhenReady());
        }

        private IEnumerator ApplyWhenReady()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", StringComparison.OrdinalIgnoreCase))
                yield break;

            for (int pass = 0; pass < 24; pass++)
            {
                Apply();
                yield return new WaitForSecondsRealtime(0.15f);
            }
        }

        private void Apply()
        {
            if (lobbyTexture == null)
                lobbyTexture = Resources.Load<Texture2D>(TexturePath);
            if (lobbyTexture == null) return;

            foreach (RawImage image in FindObjectsByType<RawImage>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (image == null || !string.Equals(image.name, "LobbyBackground", StringComparison.OrdinalIgnoreCase))
                    continue;

                image.texture = lobbyTexture;
                image.uvRect = new Rect(0f, 0f, 1f, 1f);
                image.color = Color.white;
                image.raycastTarget = false;
                if (!image.gameObject.activeSelf) image.gameObject.SetActive(true);
                image.transform.SetAsFirstSibling();
            }
        }
    }
}
