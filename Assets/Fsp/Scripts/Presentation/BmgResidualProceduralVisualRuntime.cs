using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>
    /// Strict cleanup for residual procedural visuals. Gameplay colliders, loot logic and door pivots
    /// remain authoritative, while old renderers are completely hidden and authored materials are preserved.
    /// </summary>
    public sealed class BmgResidualProceduralVisualRuntime : MonoBehaviour
    {
        private static BmgResidualProceduralVisualRuntime instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            var host = new GameObject("BMG_StrictResidualVisualRuntime");
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
            for (int pass = 0; pass < 8; pass++)
            {
                yield return pass == 0 ? null : new WaitForSeconds(.25f);
                ReplaceOldCrownDoors();
                ReplaceOldCrownWindows();
                ReplaceNamedSibling("Pump", "Models/BMG/bmg_salt_pump_mk1", "BMG_SaltPump_Authored", new Vector3(0f, -.55f, 0f), Quaternion.identity, Vector3.one);
                ReplaceNamedSibling("Pipe", "Models/BMG/bmg_salt_pipe_mk1", "BMG_SaltPipe_Authored", Vector3.zero, Quaternion.Euler(0f, 0f, 90f), Vector3.one);
                ReplaceLootPrefixes();
            }
        }

        private static void ReplaceOldCrownDoors()
        {
            var prefab = Resources.Load<GameObject>("Models/BMG/bmg_old_crown_door_mk1");
            if (prefab == null) return;
            foreach (var slab in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (slab == null || slab.name != "EntryDoor" || slab.parent == null) continue;
                var pivot = slab.parent;
                DisableRenderers(slab);
                if (pivot.Find("BMG_OldCrownDoor_Authored") != null) continue;
                var model = Instantiate(prefab, pivot, false);
                model.name = "BMG_OldCrownDoor_Authored";
                model.transform.localPosition = new Vector3(.675f, 0f, 0f);
            }
        }

        private static void ReplaceOldCrownWindows()
        {
            var prefab = Resources.Load<GameObject>("Models/BMG/bmg_old_crown_window_mk1");
            if (prefab == null) return;
            int index = 0;
            foreach (var sill in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (sill == null || sill.name != "WindowSill" || sill.parent == null) continue;
                var house = sill.parent;
                string marker = "BMG_OldCrownWindow_Authored_" + index++;
                DisableRenderers(sill);
                if (house.Find(marker) != null) continue;
                var model = Instantiate(prefab, house, false);
                model.name = marker;
                model.transform.localPosition = sill.localPosition + new Vector3(0f, .68f, 0f);
                model.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            }
        }

        private static void ReplaceLootPrefixes()
        {
            var prefab = Resources.Load<GameObject>("Models/BMG/bmg_supply_crate_mk1");
            if (prefab == null) return;
            foreach (var target in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (target == null || target.parent == null) continue;
                string n = target.name;
                bool isLoot = n.StartsWith("quarry_loot_") || n.StartsWith("saltworks_loot_") || n.StartsWith("oldcrown_loot_");
                if (!isLoot) continue;
                DisableRenderers(target);
                if (target.Find("BMG_LootVisual_Authored") != null) continue;
                var model = Instantiate(prefab, target, false);
                model.name = "BMG_LootVisual_Authored";
                model.transform.localScale = Vector3.one * 1.65f;
            }
        }

        private static void ReplaceNamedSibling(string targetName, string resourcePath, string authoredName, Vector3 offset, Quaternion rotation, Vector3 scale)
        {
            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null) return;
            int index = 0;
            foreach (var target in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (target == null || target.name != targetName || target.parent == null) continue;
                var parent = target.parent;
                string marker = authoredName + "_" + index++;
                DisableRenderers(target);
                if (parent.Find(marker) != null) continue;
                var model = Instantiate(prefab, parent, false);
                model.name = marker;
                model.transform.localPosition = target.localPosition + offset;
                model.transform.localRotation = rotation;
                model.transform.localScale = scale;
            }
        }

        private static void DisableRenderers(Transform root)
        {
            if (root == null) return;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
                if (renderer != null) renderer.enabled = false;
        }
    }
}
