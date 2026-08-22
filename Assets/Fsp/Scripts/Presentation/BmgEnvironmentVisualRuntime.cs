using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>Strict BMG environment visuals. Old render geometry is fully hidden; gameplay colliders stay active.</summary>
    public sealed class BmgEnvironmentVisualRuntime : MonoBehaviour
    {
        private const string BarricadePath = "Models/BMG/bmg_barricade_mk1";
        private const string CratePath = "Models/BMG/bmg_loot_crate_mk1";
        private const string RockPath = "Models/BMG/bmg_rock_cluster_mk1";
        private const string WarehousePath = "Models/BMG/bmg_warehouse_mk1";
        private const string CrusherPath = "Models/BMG/bmg_quarry_crusher_mk1";
        private const string WreckPath = "Models/BMG/bmg_aircraft_wreck_mk1";
        private static BmgEnvironmentVisualRuntime instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            var host = new GameObject("BMG_StrictEnvironmentVisualRuntime");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BmgEnvironmentVisualRuntime>();
            SceneManager.sceneLoaded += instance.OnSceneLoaded;
            instance.StartCoroutine(instance.ApplyDelayed());
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StartCoroutine(ApplyDelayed());

        private IEnumerator ApplyDelayed()
        {
            for (int pass = 0; pass < 8; pass++)
            {
                yield return pass == 0 ? null : new WaitForSeconds(.25f);
                ApplyNamed("Barrier", BarricadePath, "BMG_Barricade_Authored", Vector3.one);
                ApplyNamed("Crate", CratePath, "BMG_Crate_Authored", Vector3.one * .85f);
                ApplyNamed("QuarryRock", RockPath, "BMG_Rock_Authored", Vector3.one);
                ApplyNamed("Office", WarehousePath, "BMG_QuarryOffice_Authored", Vector3.one * .58f);
                ApplyNamed("Crusher", CrusherPath, "BMG_Crusher_Authored", Vector3.one * .82f);
                ApplyNamed("AircraftWreck", WreckPath, "BMG_AircraftWreck_Authored", Vector3.one);
            }
        }

        private static void ApplyNamed(string objectName, string resourcePath, string authoredName, Vector3 scale)
        {
            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null) return;

            var all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach (var target in all)
            {
                if (target == null || target.name != objectName) continue;

                var existing = target.Find(authoredName);
                if (existing == null)
                {
                    DisableRenderers(target);
                    var model = Instantiate(prefab, target, false);
                    model.name = authoredName;
                    model.transform.localPosition = Vector3.zero;
                    model.transform.localRotation = Quaternion.identity;
                    model.transform.localScale = scale;
                }
                else
                {
                    // Keep authored model visible, but ensure any procedural siblings/children stay hidden.
                    var renderers = target.GetComponentsInChildren<Renderer>(true);
                    foreach (var renderer in renderers)
                        if (renderer != null && !renderer.transform.IsChildOf(existing)) renderer.enabled = false;
                }
            }
        }

        private static void DisableRenderers(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
                if (renderer != null) renderer.enabled = false;
        }
    }
}
