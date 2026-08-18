using System;
using System.Collections.Generic;
using Fsp.BattleRoyale;
using Fsp.Presentation;
using Fsp.Vehicles;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.World
{
    /// <summary>
    /// Conservative mobile culling for the generated 2400x2400 Sunscar world. It caches static world
    /// components instead of scanning the whole scene every frame and never disables gameplay actors.
    /// </summary>
    public sealed class MobileWorldOptimizer : MonoBehaviour
    {
        [SerializeField] private float highDetailDistance = 180f;
        [SerializeField] private float visibleDistance = 360f;
        [SerializeField] private float colliderDistance = 125f;
        [SerializeField] private float flightVisibleDistance = 900f;
        [SerializeField] private float updateInterval = 0.85f;
        [SerializeField] private float recacheInterval = 8f;

        private readonly List<Renderer> worldRenderers = new();
        private readonly List<Collider> worldColliders = new();
        private Transform target;
        private MatchParticipant localParticipant;
        private float nextUpdate;
        private float nextRecache;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<MobileWorldOptimizer>() == null)
                new GameObject("Fsp_MobileWorldOptimizer").AddComponent<MobileWorldOptimizer>();
        }

        private void Start()
        {
            ApplyQualityTier();
            ResolveTarget();
            RebuildCache();
        }

        private void Update()
        {
            if (Time.unscaledTime >= nextRecache)
            {
                nextRecache = Time.unscaledTime + recacheInterval;
                RebuildCache();
            }

            if (Time.unscaledTime < nextUpdate) return;
            nextUpdate = Time.unscaledTime + updateInterval;
            if (target == null || localParticipant == null) ResolveTarget();
            if (target == null) return;
            OptimizeScene();
        }

        private void ResolveTarget()
        {
            MatchParticipant[] participants = FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None);
            foreach (MatchParticipant participant in participants)
            {
                if (participant == null || !participant.IsLocalPlayer) continue;
                localParticipant = participant;
                target = participant.transform;
                return;
            }

            if (Camera.main != null) target = Camera.main.transform;
        }

        private void ApplyQualityTier()
        {
            int ram = SystemInfo.systemMemorySize;
            if (ram > 0 && ram <= 3500)
            {
                highDetailDistance = 130f;
                visibleDistance = 280f;
                colliderDistance = 95f;
                flightVisibleDistance = 700f;
                updateInterval = 1.05f;
            }
            else if (ram >= 7000)
            {
                highDetailDistance = 240f;
                visibleDistance = 500f;
                colliderDistance = 165f;
                flightVisibleDistance = 1100f;
                updateInterval = 0.65f;
            }
        }

        private void RebuildCache()
        {
            worldRenderers.Clear();
            worldColliders.Clear();

            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || IsDynamicGameplayObject(renderer.transform)) continue;
                renderer.allowOcclusionWhenDynamic = true;
                worldRenderers.Add(renderer);
            }

            Collider[] colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            foreach (Collider collider in colliders)
            {
                if (collider == null || collider is CharacterController || IsDynamicGameplayObject(collider.transform)) continue;
                worldColliders.Add(collider);
            }
        }

        private void OptimizeScene()
        {
            Vector3 p = target.position;
            bool airborne = IsLocalPlayerAirbornePhase();
            float renderRange = airborne ? flightVisibleDistance : visibleDistance;
            float renderRangeSqr = renderRange * renderRange;
            float colliderRangeSqr = colliderDistance * colliderDistance;

            foreach (Renderer renderer in worldRenderers)
            {
                if (renderer == null) continue;
                if (IsAlwaysVisible(renderer.gameObject.name))
                {
                    renderer.enabled = true;
                    continue;
                }

                Vector3 delta = renderer.bounds.center - p;
                renderer.enabled = delta.sqrMagnitude <= renderRangeSqr;
            }

            foreach (Collider collider in worldColliders)
            {
                if (collider == null || collider.isTrigger) continue;
                if (IsAlwaysCollision(collider.gameObject.name))
                {
                    collider.enabled = true;
                    continue;
                }

                Vector3 delta = collider.bounds.center - p;
                collider.enabled = delta.sqrMagnitude <= colliderRangeSqr;
            }
        }

        private bool IsLocalPlayerAirbornePhase()
        {
            if (localParticipant == null) return false;
            DropPlanePassenger passenger = localParticipant.GetComponent<DropPlanePassenger>();
            if (passenger != null && passenger.IsAboard) return true;
            ParachuteController parachute = localParticipant.GetComponent<ParachuteController>();
            return parachute != null && parachute.IsActive;
        }

        private static bool IsDynamicGameplayObject(Transform t)
        {
            if (t == null) return false;
            return t.GetComponentInParent<MatchParticipant>() != null ||
                   t.GetComponentInParent<SimpleVehicleController>() != null ||
                   t.GetComponentInParent<DropPlaneController>() != null ||
                   t.GetComponentInParent<SafeZoneController>() != null;
        }

        private static bool IsAlwaysVisible(string objectName)
        {
            string n = (objectName ?? string.Empty).ToLowerInvariant();
            return n.Contains("ground_base") || n.Contains("sunscarroadnetwork");
        }

        private static bool IsAlwaysCollision(string objectName)
        {
            string n = (objectName ?? string.Empty).ToLowerInvariant();
            return n.Contains("ground") || n.Contains("floor") || n.Contains("road") || n.Contains("bridge");
        }

        public bool IsHighDetail(Vector3 worldPosition)
        {
            return target != null && (target.position - worldPosition).sqrMagnitude <= highDetailDistance * highDetailDistance;
        }
    }
}
