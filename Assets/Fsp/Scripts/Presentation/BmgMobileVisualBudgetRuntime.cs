using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>Lightweight distance culling for authored environment props on Android/mobile builds.</summary>
    public sealed class BmgMobileVisualBudgetRuntime : MonoBehaviour
    {
        private static BmgMobileVisualBudgetRuntime instance;
        [SerializeField] private float smallPropDistance = 95f;
        [SerializeField] private float mediumPropDistance = 155f;
        [SerializeField] private float largePropDistance = 240f;

        private static readonly string[] SmallTokens = { "Barrel", "Supply", "Generator", "Sandbag", "Fence" };
        private static readonly string[] MediumTokens = { "Fuel", "Guardhouse", "Barricade", "Crate", "Rock" };
        private static readonly string[] LargeTokens = { "Warehouse", "Hangar", "Watchtower", "Radio" };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            var host = new GameObject("BMG_MobileVisualBudgetRuntime");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BmgMobileVisualBudgetRuntime>();
            SceneManager.sceneLoaded += instance.OnSceneLoaded;
            instance.StartCoroutine(instance.Loop());
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }

        private IEnumerator Loop()
        {
            WaitForSeconds wait = new(0.45f);
            while (true)
            {
                ApplyBudget();
                yield return wait;
            }
        }

        private void ApplyBudget()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            Vector3 origin = cam.transform.position;
            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null || !r.gameObject.name.StartsWith("BMG_", System.StringComparison.Ordinal)) continue;
                if (IsCharacterOrVehicle(r.transform)) continue;
                float limit = DistanceFor(r.gameObject.name);
                if (limit <= 0f) continue;
                bool visible = (r.bounds.center - origin).sqrMagnitude <= limit * limit;
                if (r.enabled != visible) r.enabled = visible;
            }
        }

        private float DistanceFor(string name)
        {
            if (ContainsAny(name, SmallTokens)) return smallPropDistance;
            if (ContainsAny(name, MediumTokens)) return mediumPropDistance;
            if (ContainsAny(name, LargeTokens)) return largePropDistance;
            return 0f;
        }

        private static bool ContainsAny(string value, string[] tokens)
        {
            for (int i = 0; i < tokens.Length; i++)
                if (value.IndexOf(tokens[i], System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private static bool IsCharacterOrVehicle(Transform t)
        {
            for (Transform p = t; p != null; p = p.parent)
            {
                if (p.GetComponent<StarterProceduralCharacterVisual>() != null) return true;
                if (p.GetComponent<StarterProceduralVehicleVisual>() != null) return true;
                if (p.GetComponent<StarterPlaneVisual>() != null) return true;
            }
            return false;
        }
    }
}
