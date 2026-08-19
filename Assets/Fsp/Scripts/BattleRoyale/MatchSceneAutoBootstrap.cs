using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.BattleRoyale
{
    /// <summary>
    /// Logic-only bootstrap for the checked-in Match scene.
    /// The scene itself is source-controlled; this only guarantees gameplay systems exist.
    /// </summary>
    public static class MatchSceneAutoBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (UnityEngine.Object.FindFirstObjectByType<MatchSceneAssembler>() == null)
                new GameObject("MatchSceneAssembler").AddComponent<MatchSceneAssembler>();
        }
    }
}
