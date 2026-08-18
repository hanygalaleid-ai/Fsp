using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Lobby
{
    /// <summary>
    /// Release/device safety net: the primitive lobby was useful only during early prototyping.
    /// It must never be visible in an Android build. The real lobby background/UI is provided by
    /// checked-in Resources art; the pre-build art gate now guarantees those assets are importable.
    /// </summary>
    public sealed class LobbyPrimitiveBlockerRuntime : MonoBehaviour
    {
        private static readonly string[] LegacyNames =
        {
            "FortSilhouette",
            "CampLeft",
            "CampRight",
            "Antenna",
            "LobbyHero_Procedural"
        };

        private float nextScan;
        private float stopAt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<LobbyPrimitiveBlockerRuntime>() == null)
                new GameObject("Fsp_LobbyPrimitiveBlocker").AddComponent<LobbyPrimitiveBlockerRuntime>();
        }

        private void Awake()
        {
            stopAt = Time.unscaledTime + 20f;
            HideLegacy();
        }

        private void Update()
        {
            if (Time.unscaledTime > stopAt)
            {
                enabled = false;
                return;
            }

            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 0.2f;
            HideLegacy();
        }

        private static void HideLegacy()
        {
            foreach (string name in LegacyNames)
            {
                GameObject go = GameObject.Find(name);
                if (go != null && go.activeSelf)
                    go.SetActive(false);
            }
        }
    }
}
