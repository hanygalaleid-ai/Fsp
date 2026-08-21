using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>
    /// Build 149 landmark pass for Copper Port, Dryfield and Lantern Coast.
    /// Keeps generated gameplay colliders authoritative and swaps only visible geometry.
    /// </summary>
    public sealed class BmgPoiLandmarkVisualRuntime : MonoBehaviour
    {
        private static BmgPoiLandmarkVisualRuntime instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            GameObject host = new("BMG_PoiLandmarkVisualRuntime");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BmgPoiLandmarkVisualRuntime>();
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
            yield return null;

            ReplaceGroups("Warehouse", "Models/BMG/bmg_warehouse_mk1", "BMG_PortWarehouse_Authored", Vector3.one * 1.05f, new Color(.33f,.32f,.29f));
            ReplaceObjects("CargoContainer", "Models/BMG/bmg_shipping_container_mk1", "BMG_PortContainer_Authored", Vector3.one, new Color(.35f,.25f,.18f));
            ReplaceGroups("Barn", "Models/BMG/bmg_barn_mk1", "BMG_Barn_Authored", Vector3.one, new Color(.38f,.29f,.20f));
            ReplaceGroups("WaterTower", "Models/BMG/bmg_water_tower_mk1", "BMG_WaterTower_Authored", Vector3.one, new Color(.31f,.34f,.32f));
            ReplaceGroups("Lighthouse", "Models/BMG/bmg_lighthouse_mk1", "BMG_Lighthouse_Authored", Vector3.one, new Color(.62f,.58f,.49f));
            ReplaceGroups("CoastHut", "Models/BMG/bmg_coast_hut_mk1", "BMG_CoastHut_Authored", Vector3.one, new Color(.42f,.37f,.30f));
            ReplaceCranePairs();
        }

        private static void ReplaceCranePairs()
        {
            GameObject prefab = Resources.Load<GameObject>("Models/BMG/bmg_port_crane_mk1");
            if (prefab == null) return;
            Transform[] all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            int index = 0;
            for (int i = 0; i < all.Length; i++)
            {
                Transform post = all[i];
                if (post == null || post.name != "CranePost" || post.parent == null) continue;
                Transform parent = post.parent;
                string markerName = "BMG_PortCrane_Authored_" + index++;
                if (parent.Find(markerName) != null) continue;

                Vector3 localPosition = post.localPosition;
                HideRenderer(post);
                for (int c = 0; c < parent.childCount; c++)
                {
                    Transform child = parent.GetChild(c);
                    if (child != null && child.name == "CraneArm" && Mathf.Abs(child.localPosition.x - (localPosition.x + 4f)) < 0.5f)
                        HideRenderer(child);
                }

                GameObject model = Instantiate(prefab, parent, false);
                model.name = markerName;
                model.transform.localPosition = new Vector3(localPosition.x, 0f, localPosition.z);
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one * 1.25f;
                ApplyMaterial(model, new Color(.73f,.30f,.08f));
            }
        }

        private static void ReplaceGroups(string groupName, string resourcePath, string authoredName, Vector3 scale, Color color)
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null) return;
            Transform[] all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                Transform target = all[i];
                if (target == null || target.name != groupName || target.Find(authoredName) != null) continue;
                HideRenderersRecursive(target);
                GameObject model = Instantiate(prefab, target, false);
                model.name = authoredName;
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = scale;
                ApplyMaterial(model, color);
            }
        }

        private static void ReplaceObjects(string objectName, string resourcePath, string authoredName, Vector3 scale, Color color)
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null) return;
            Transform[] all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                Transform target = all[i];
                if (target == null || target.name != objectName || target.Find(authoredName) != null) continue;
                HideRenderer(target);
                GameObject model = Instantiate(prefab, target, false);
                model.name = authoredName;
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = scale;
                ApplyMaterial(model, color);
            }
        }

        private static void HideRenderersRecursive(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null && !renderers[i].gameObject.name.StartsWith("BMG_")) renderers[i].enabled = false;
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
            for (int i = 0; i < renderers.Length; i++) if (renderers[i] != null) renderers[i].sharedMaterial = material;
        }
    }
}
