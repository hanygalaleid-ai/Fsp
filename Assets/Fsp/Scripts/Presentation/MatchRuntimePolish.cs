using Fsp.BattleRoyale;
using Fsp.Combat;
using Fsp.Vehicles;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>
    /// Lightweight visual safety net for cloud-generated Match scenes.
    /// Keeps presentation helpers alive without fighting FixedWorldArtRuntime lighting/materials or ADS FOV.
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
            Scan();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 1.25f;
            ApplySafeCameraAndLighting();
            Scan();
        }

        private static void ApplySafeCameraAndLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.30f, 0.27f, 0.23f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.47f, 0.52f, 0.55f, 1f);
            RenderSettings.fogStartDistance = 450f;
            RenderSettings.fogEndDistance = 1450f;

            Light sun = RenderSettings.sun;
            if (sun == null)
            {
                foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
                {
                    if (light != null && light.type == LightType.Directional)
                    {
                        sun = light;
                        break;
                    }
                }
            }

            if (sun == null)
            {
                GameObject sunObject = new GameObject("FSP_Sun");
                sun = sunObject.AddComponent<Light>();
                sun.type = LightType.Directional;
                RenderSettings.sun = sun;
            }

            sun.color = new Color(1f, 0.83f, 0.66f, 1f);
            sun.intensity = 0.85f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.allowHDR = false;
                camera.allowMSAA = true;
                // Do not write fieldOfView here: AimDownSightsController owns gameplay FOV.
                camera.nearClipPlane = 0.08f;
                camera.farClipPlane = 1600f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.36f, 0.54f, 0.68f, 1f);
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
                if (participant.GetComponent<ParachuteController>() != null && participant.GetComponent<StarterParachuteVisual>() == null)
                    participant.gameObject.AddComponent<StarterParachuteVisual>();
            }

            SimpleVehicleController[] vehicles = FindObjectsByType<SimpleVehicleController>(FindObjectsSortMode.None);
            foreach (SimpleVehicleController vehicle in vehicles)
            {
                if (vehicle == null) continue;
                if (vehicle.GetComponent<StarterProceduralVehicleVisual>() == null)
                    vehicle.gameObject.AddComponent<StarterProceduralVehicleVisual>();
            }

            DropPlaneController[] planes = FindObjectsByType<DropPlaneController>(FindObjectsSortMode.None);
            foreach (DropPlaneController plane in planes)
            {
                if (plane == null) continue;
                if (plane.GetComponent<StarterPlaneVisual>() == null)
                    plane.gameObject.AddComponent<StarterPlaneVisual>();
            }

            HitscanWeapon[] weapons = FindObjectsByType<HitscanWeapon>(FindObjectsSortMode.None);
            foreach (HitscanWeapon weapon in weapons)
            {
                if (weapon == null) continue;
                if (weapon.GetComponent<StarterProceduralWeaponVisual>() == null)
                    weapon.gameObject.AddComponent<StarterProceduralWeaponVisual>();
            }

            SafeZoneController[] zones = FindObjectsByType<SafeZoneController>(FindObjectsSortMode.None);
            foreach (SafeZoneController zone in zones)
            {
                if (zone == null) continue;
                foreach (Renderer renderer in zone.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer != null && renderer.gameObject.name.Contains("Placeholder"))
                        renderer.enabled = false;
                }
            }
        }
    }
}
