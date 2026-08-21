using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>
    /// Build 149 residual procedural cleanup. Swaps only render geometry; gameplay colliders,
    /// loot components and Old Crown door pivots remain authoritative.
    /// </summary>
    public sealed class BmgResidualProceduralVisualRuntime : MonoBehaviour
    {
        private static BmgResidualProceduralVisualRuntime instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            GameObject host = new("BMG_ResidualProceduralVisualRuntime");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BmgResidualProceduralVisualRuntime>();
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
            // World generators and door installer build over several frames.
            for (int i = 0; i < 8; i++) yield return null;
            ReplaceOldCrownDoors();
            ReplaceOldCrownWindows();
            ReplaceNamedSibling("Pump", "Models/BMG/bmg_salt_pump_mk1", "BMG_SaltPump_Authored", new Vector3(0f, -0.55f, 0f), Quaternion.identity, Vector3.one, new Color(.28f,.31f,.30f));
            ReplaceNamedSibling("Pipe", "Models/BMG/bmg_salt_pipe_mk1", "BMG_SaltPipe_Authored", Vector3.zero, Quaternion.Euler(0f, 0f, 90f), Vector3.one, new Color(.30f,.33f,.31f));
            ReplaceLootPrefixes();
        }

        private static void ReplaceOldCrownDoors()
        {
            GameObject prefab = Resources.Load<GameObject>("Models/BMG/bmg_old_crown_door_mk1");
            if (prefab == null) return;
            Transform[] all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                Transform slab = all[i];
                if (slab == null || slab.name != "EntryDoor" || slab.parent == null) continue;
                Transform pivot = slab.parent;
                if (pivot.Find("BMG_OldCrownDoor_Authored") != null) continue;
                HideRenderer(slab);
                GameObject model = Instantiate(prefab, pivot, false);
                model.name = "BMG_OldCrownDoor_Authored";
                model.transform.localPosition = new Vector3(.675f, 0f, 0f);
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;
                ApplyMaterial(model, new Color(.30f,.19f,.11f));
            }
        }

        private static void ReplaceOldCrownWindows()
        {
            GameObject prefab = Resources.Load<GameObject>("Models/BMG/bmg_old_crown_window_mk1");
            if (prefab == null) return;
            Transform[] all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            int index = 0;
            for (int i = 0; i < all.Length; i++)
            {
                Transform sill = all[i];
                if (sill == null || sill.name != "WindowSill" || sill.parent == null) continue;
                Transform house = sill.parent;
                string marker = "BMG_OldCrownWindow_Authored_" + index++;
                if (house.Find(marker) != null) continue;
                HideRenderer(sill);
                GameObject model = Instantiate(prefab, house, false);
                model.name = marker;
                model.transform.localPosition = sill.localPosition + new Vector3(0f, .68f, 0f);
                model.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                model.transform.localScale = Vector3.one;
                ApplyMaterial(model, new Color(.24f,.20f,.15f));
            }
        }

        private static void ReplaceLootPrefixes()
        {
            GameObject prefab = Resources.Load<GameObject>("Models/BMG/bmg_supply_crate_mk1");
            if (prefab == null) return;
            Transform[] all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                Transform target = all[i];
                if (target == null || target.parent == null) continue;
                string n = target.name;
                bool isLoot = n.StartsWith("quarry_loot_") || n.StartsWith("saltworks_loot_") || n.StartsWith("oldcrown_loot_");
                if (!isLoot || target.Find("BMG_LootVisual_Authored") != null) continue;
                HideRenderer(target);
                GameObject model = Instantiate(prefab, target, false);
                model.name = "BMG_LootVisual_Authored";
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one * 1.65f;
                ApplyMaterial(model, new Color(.34f,.23f,.13f));
            }
        }

        private static void ReplaceNamedSibling(string targetName, string resourcePath, string authoredName, Vector3 offset, Quaternion rotation, Vector3 scale, Color color)
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null) return;
            Transform[] all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            int index = 0;
            for (int i = 0; i < all.Length; i++)
            {
                Transform target = all[i];
                if (target == null || target.name != targetName || target.parent == null) continue;
                Transform parent = target.parent;
                string marker = authoredName + "_" + index++;
                if (parent.Find(marker) != null) continue;
                HideRenderer(target);
                GameObject model = Instantiate(prefab, parent, false);
                model.name = marker;
                model.transform.localPosition = target.localPosition + offset;
                model.transform.localRotation = rotation;
                model.transform.localScale = scale;
                ApplyMaterial(model, color);
            }
        }

        private static void HideRenderer(Transform target)
        {
            Renderer renderer = target != null ? target.GetComponent<Renderer>() : null;
            if (renderer != null) renderer.enabled = false;
        }

        private static void ApplyMaterial(GameObject root, Color color)
        {
            Shader shader = Resources.Load<Shader>("Shaders/FspMobileSafe");
            if (shader == null) shader = Shader.Find("Fsp/MobileSafeLit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) return;
            Material material = new(shader) { color = color, hideFlags = HideFlags.DontSave };
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null) renderers[i].sharedMaterial = material;
        }
    }
}
