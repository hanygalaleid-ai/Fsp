using Fsp.BattleRoyale;
using Fsp.Vehicles;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>
    /// Lightweight visual safety net for cloud-generated Match scenes.
    /// Replaces bare primitive player/vehicle renderers with the original FSP procedural presentation.
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
            ApplySceneLighting();
            Scan();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 1.25f;
            ApplySceneLighting();
            Scan();
        }

        private static void ApplySceneLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.39f, 0.33f, 0.27f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.58f, 0.53f, 0.46f);
            RenderSettings.fogDensity = 0.00135f;

            Light sun = null;
            foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light != null && light.type == LightType.Directional)
                {
                    sun = light;
                    break;
                }
            }

            if (sun == null)
            {
                GameObject sunObject = new GameObject("FSP_Sun");
                sun = sunObject.AddComponent<Light>();
                sun.type = LightType.Directional;
            }

            sun.color = new Color(1f, 0.79f, 0.58f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(34f, -42f, 0f);

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.allowHDR = false;
                camera.fieldOfView = FspFixedTheme.MatchFieldOfView;
                camera.nearClipPlane = 0.08f;
                camera.farClipPlane = 1600f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.46f, 0.63f, 0.74f, 1f);
            }
        }

        private static void Scan()
        {
            MatchParticipant[] participants = FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None);
            foreach (MatchParticipant participant in participants)
            {
                if (participant == null) continue;
                if (participant.GetComponent<StarterProceduralCharacterVisual>() == null)
                    participant.gameObject.AddComponent<StarterProceduralCharacterVisual>();
            }

            SimpleVehicleController[] vehicles = FindObjectsByType<SimpleVehicleController>(FindObjectsSortMode.None);
            foreach (SimpleVehicleController vehicle in vehicles)
            {
                if (vehicle == null) continue;
                if (vehicle.GetComponent<StarterProceduralVehicleVisual>() == null)
                    vehicle.gameObject.AddComponent<StarterProceduralVehicleVisual>();
            }
        }
    }
}
