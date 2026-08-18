using Fsp.BattleRoyale;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>
    /// Conservative device-safe presentation guard for the Match scene.
    /// It fixes camera/lighting state only and never injects prototype geometry.
    /// </summary>
    public sealed class MatchRuntimePolish : MonoBehaviour
    {
        private float nextScan;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", System.StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<MatchRuntimePolish>() == null)
                new GameObject("Fsp_MatchRuntimePolish").AddComponent<MatchRuntimePolish>();
        }

        private void Awake()
        {
            ApplySafeCameraAndLighting();
            HidePrototypeZoneRenderers();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 1.25f;
            ApplySafeCameraAndLighting();
            HidePrototypeZoneRenderers();
        }

        private static void ApplySafeCameraAndLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.16f, 0.15f, 0.14f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.30f, 0.37f, 0.40f, 1f);
            RenderSettings.fogStartDistance = 350f;
            RenderSettings.fogEndDistance = 1350f;

            Light sun = RenderSettings.sun;
            if (sun == null)
            {
                foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
                {
                    if (light != null && light.type == LightType.Directional) { sun = light; break; }
                }
            }
            if (sun == null)
            {
                GameObject sunObject = new GameObject("FSP_Sun");
                sun = sunObject.AddComponent<Light>();
                sun.type = LightType.Directional;
                RenderSettings.sun = sun;
            }
            sun.color = new Color(1f, 0.90f, 0.78f, 1f);
            sun.intensity = 0.62f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            foreach (Camera camera in FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (camera == null || !camera.enabled) continue;
                camera.allowHDR = false;
                camera.allowMSAA = true;
                camera.nearClipPlane = Mathf.Max(0.08f, camera.nearClipPlane);
                camera.farClipPlane = Mathf.Max(1600f, camera.farClipPlane);
                if (camera == Camera.main)
                {
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.backgroundColor = new Color(0.25f, 0.40f, 0.52f, 1f);
                }
            }
        }

        private static void HidePrototypeZoneRenderers()
        {
            SafeZoneController[] zones = FindObjectsByType<SafeZoneController>(FindObjectsSortMode.None);
            foreach (SafeZoneController zone in zones)
            {
                if (zone == null) continue;
                foreach (Renderer renderer in zone.GetComponentsInChildren<Renderer>(true))
                    if (renderer != null && renderer.gameObject.name.Contains("Placeholder")) renderer.enabled = false;
            }
        }
    }
}
