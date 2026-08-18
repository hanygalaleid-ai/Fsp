using System;
using Fsp.BattleRoyale;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>
    /// Ensures CI/runtime-generated Match scenes always have functional ADS on the local player.
    /// The mobile HUD already drives MobileInputBridge.AimHeld; this installer guarantees a
    /// controller consumes that state even when no serialized player prefab exists.
    /// </summary>
    public sealed class AimDownSightsRuntimeInstaller : MonoBehaviour
    {
        private float stopAt;
        private float nextTry;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<AimDownSightsRuntimeInstaller>() == null)
                new GameObject("Fsp_AimDownSightsRuntimeInstaller").AddComponent<AimDownSightsRuntimeInstaller>();
        }

        private void Awake()
        {
            stopAt = Time.unscaledTime + 20f;
        }

        private void Update()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", StringComparison.OrdinalIgnoreCase))
            {
                Destroy(gameObject);
                return;
            }

            if (Time.unscaledTime > stopAt)
            {
                Debug.LogError("FSP ADS installer timed out waiting for the local participant.");
                Destroy(gameObject);
                return;
            }

            if (Time.unscaledTime < nextTry) return;
            nextTry = Time.unscaledTime + 0.2f;

            MatchParticipant[] participants = FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None);
            foreach (MatchParticipant participant in participants)
            {
                if (participant == null || !participant.IsLocalPlayer) continue;
                if (participant.GetComponent<AimDownSightsController>() == null)
                    participant.gameObject.AddComponent<AimDownSightsController>();
                Destroy(gameObject);
                return;
            }
        }
    }
}
