using System;
using Fsp.Bots;
using Fsp.Lobby;
using Fsp.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.BattleRoyale
{
    /// <summary>
    /// Persistent scene lifecycle bootstrap. RuntimeInitializeOnLoadMethod only guarantees startup
    /// initialization, so subscribe to SceneManager.sceneLoaded to rebuild runtime-only systems on
    /// every Lobby <-> Match transition and on the second and later matches.
    /// </summary>
    public static class MatchSceneAutoBootstrap
    {
        private static bool subscribed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLifecycle()
        {
            if (subscribed) return;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            subscribed = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInitialScene()
        {
            HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!scene.IsValid()) return;

            if (string.Equals(scene.name, "Lobby", StringComparison.OrdinalIgnoreCase))
            {
                LobbyRuntimeGuard.EnsureInstalled();
                return;
            }

            if (!string.Equals(scene.name, "Match", StringComparison.OrdinalIgnoreCase)) return;

            MobileInputBridge.Instance?.ResetAll();
            FallbackBotAgent.ResetForNewMatch();

            if (UnityEngine.Object.FindFirstObjectByType<MatchSceneAssembler>() == null)
                new GameObject("MatchSceneAssembler").AddComponent<MatchSceneAssembler>();
        }
    }
}
