using System;
using Fsp.BattleRoyale;
using Fsp.Input;
using Fsp.Player;
using Fsp.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Core
{
    /// <summary>
    /// Final Android safety guard for the Match scene. Guarantees a tagged gameplay camera,
    /// a visible styled local player, and a spawn above the battle floor without injecting
    /// any raw prototype geometry.
    /// </summary>
    public sealed class MatchReleaseSafetyRuntime : MonoBehaviour
    {
        private float nextScan;
        private float stopAt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<MatchReleaseSafetyRuntime>() == null)
                new GameObject("Fsp_MatchReleaseSafetyRuntime").AddComponent<MatchReleaseSafetyRuntime>();
        }

        private void Awake()
        {
            stopAt = Time.unscaledTime + 25f;
            Repair();
        }

        private void Update()
        {
            if (Time.unscaledTime > stopAt) { enabled = false; return; }
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 0.35f;
            Repair();
        }

        private static void Repair()
        {
            Camera camera = EnsureMainCamera();
            MatchParticipant local = FindLocalParticipant();
            if (local == null) return;

            // Never let a just-created player start below the generated Sunscar floor.
            if (local.transform.position.y < -2f)
            {
                Vector3 p = local.transform.position;
                p.y = 1.15f;
                local.transform.position = p;
            }

            // The raw fallback capsule must never be visible in release.
            Renderer raw = local.GetComponent<Renderer>();
            if (raw != null) raw.enabled = false;

            if (local.GetComponent<StarterProceduralCharacterVisual>() == null)
                local.gameObject.AddComponent<StarterProceduralCharacterVisual>();

            if (local.GetComponent<MobileGameplayAdapter>() == null)
                local.gameObject.AddComponent<MobileGameplayAdapter>();

            if (camera != null)
            {
                ThirdPersonMotor motor = local.GetComponent<ThirdPersonMotor>();
                if (motor != null) motor.SetCamera(camera.transform);
            }
        }

        private static MatchParticipant FindLocalParticipant()
        {
            foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
                if (participant != null && participant.IsLocalPlayer) return participant;
            return null;
        }

        private static Camera EnsureMainCamera()
        {
            Camera main = Camera.main;
            if (main != null)
            {
                Configure(main);
                return main;
            }

            Camera candidate = FindFirstObjectByType<Camera>();
            if (candidate == null)
            {
                GameObject go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                candidate = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
                go.transform.position = new Vector3(0f, 4.2f, -7.2f);
            }
            else
            {
                candidate.gameObject.tag = "MainCamera";
                if (candidate.GetComponent<AudioListener>() == null)
                    candidate.gameObject.AddComponent<AudioListener>();
            }

            Configure(candidate);
            return candidate;
        }

        private static void Configure(Camera camera)
        {
            camera.enabled = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.23f, 0.38f, 0.50f, 1f);
            camera.allowHDR = false;
            camera.allowMSAA = true;
            camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, 55f, 78f);
            camera.nearClipPlane = 0.08f;
            camera.farClipPlane = Mathf.Max(camera.farClipPlane, 1800f);
        }
    }
}
