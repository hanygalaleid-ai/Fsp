using Fsp.BattleRoyale;
using Fsp.Combat;
using Fsp.Vehicles;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>
    /// Release-only visual cleanup for CI/runtime-created gameplay objects.
    /// Keeps fallback gameplay logic alive while preventing raw Unity capsules/cubes from reaching players.
    /// </summary>
    public sealed class ReleasePlaceholderStyler : MonoBehaviour
    {
        private float nextScan;
        private float stopAt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", System.StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<ReleasePlaceholderStyler>() == null)
                new GameObject("Fsp_ReleasePlaceholderStyler").AddComponent<ReleasePlaceholderStyler>();
        }

        private void Awake()
        {
            stopAt = Time.unscaledTime + 30f;
            Apply();
        }

        private void Update()
        {
            if (Time.unscaledTime > stopAt) { enabled = false; return; }
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 0.35f;
            Apply();
        }

        private static void Apply()
        {
            foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
            {
                if (participant == null) continue;
                HideRootRenderer(participant.gameObject);
                if (participant.GetComponent<StarterProceduralCharacterVisual>() == null)
                    participant.gameObject.AddComponent<StarterProceduralCharacterVisual>();
            }

            foreach (DropPlaneController plane in FindObjectsByType<DropPlaneController>(FindObjectsSortMode.None))
            {
                if (plane == null) continue;
                HideRootRenderer(plane.gameObject);
                if (plane.GetComponent<StarterPlaneVisual>() == null)
                    plane.gameObject.AddComponent<StarterPlaneVisual>();
            }

            foreach (SimpleVehicleController vehicle in FindObjectsByType<SimpleVehicleController>(FindObjectsSortMode.None))
            {
                if (vehicle == null) continue;
                HideRootRenderer(vehicle.gameObject);
                if (vehicle.GetComponent<StarterProceduralVehicleVisual>() == null)
                    vehicle.gameObject.AddComponent<StarterProceduralVehicleVisual>();
            }

            foreach (HitscanWeapon weapon in FindObjectsByType<HitscanWeapon>(FindObjectsSortMode.None))
            {
                if (weapon == null) continue;
                HideRootRenderer(weapon.gameObject);
                if (weapon.GetComponent<StarterProceduralWeaponVisual>() == null)
                    weapon.gameObject.AddComponent<StarterProceduralWeaponVisual>();
            }

            foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if (renderer == null) continue;
                string n = renderer.gameObject.name;
                if (n.Contains("SafeZoneVisual_Placeholder")) renderer.enabled = false;
            }
        }

        private static void HideRootRenderer(GameObject go)
        {
            Renderer r = go != null ? go.GetComponent<Renderer>() : null;
            if (r != null) r.enabled = false;
        }
    }
}
