using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.World
{
    /// <summary>
    /// Release guard for the checked-in Sunscar world.
    /// The Match scene now owns all world art and collision surfaces. Runtime generation of
    /// placeholder ground, roads, rocks, and POIs is intentionally disabled on every platform.
    /// </summary>
    public sealed class SunscarRuntimeWorldInstaller : MonoBehaviour
    {
        private const string RootName = "SunscarRuntimeWorld";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", StringComparison.OrdinalIgnoreCase)) return;

            GameObject legacyRoot = GameObject.Find(RootName);
            if (legacyRoot != null)
            {
                Debug.LogWarning("FSP Match: removing legacy runtime-generated Sunscar world; checked-in scene art is authoritative.");
                Destroy(legacyRoot);
            }
        }
    }
}
