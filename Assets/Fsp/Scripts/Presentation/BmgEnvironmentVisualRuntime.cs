using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>Build 149 one-shot environment visual upgrade. Existing colliders/gameplay objects remain authoritative.</summary>
    public sealed class BmgEnvironmentVisualRuntime : MonoBehaviour
    {
        private const string BarricadePath = "Models/BMG/bmg_barricade_mk1";
        private const string CratePath = "Models/BMG/bmg_loot_crate_mk1";
        private const string RockPath = "Models/BMG/bmg_rock_cluster_mk1";
        private const string WarehousePath = "Models/BMG/bmg_warehouse_mk1";
        private static BmgEnvironmentVisualRuntime instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            GameObject host = new("BMG_EnvironmentVisualRuntime");
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
            yield return null;
            yield return null;
            yield return null;
            ApplyNamed("Barrier", BarricadePath, "BMG_Barricade_Authored", Vector3.one, new Color(.28f,.25f,.19f));
            ApplyNamed("Crate", CratePath, "BMG_Crate_Authored", Vector3.one * .85f, new Color(.32f,.20f,.12f));
            ApplyNamed("QuarryRock", RockPath, "BMG_Rock_Authored", Vector3.one, new Color(.42f,.40f,.36f));
            ApplyNamed("Office", WarehousePath, "BMG_QuarryOffice_Authored", Vector3.one * .58f, new Color(.29f,.31f,.30f));
        }

        private static void ApplyNamed(string objectName, string resourcePath, string authoredName, Vector3 scale, Color color)
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null) return;
            Transform[] all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                Transform target = all[i];
                if (target == null || target.name != objectName || target.Find(authoredName) != null) continue;
                Renderer renderer = target.GetComponent<Renderer>();
                if (renderer != null) renderer.enabled = false;
                GameObject model = Instantiate(prefab, target, false);
                model.name = authoredName;
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = scale;
                ApplyMaterial(model, color);
            }
        }

        private static void ApplyMaterial(GameObject root, Color color)
        {
            Shader shader = Resources.Load<Shader>("Shaders/FspMobileSafe");
            if (shader == null) shader = Shader.Find("Fsp/MobileSafeLit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) return;
            Material material = new(shader) { color = color, hideFlags = HideFlags.DontSave };
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++) if (renderers[i] != null) renderers[i].sharedMaterial = material;
        }
    }
}
