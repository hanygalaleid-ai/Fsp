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
            Scan();
        }

        private static void ApplySceneLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.33f, 0.30f, 0.26f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.52f, 0.56f, 0.57f);
            RenderSettings.fogDensity = 0.0018f;

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.allowHDR = false;
                camera.fieldOfView = 64f;
                camera.nearClipPlane = 0.08f;
                camera.farClipPlane = 1600f;
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
