using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>
    /// Build 149 landmark pass. Generated gameplay colliders stay authoritative;
    /// only visible primitive geometry is replaced with checked-in authored meshes.
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

            // Copper Port / Dryfield / Lantern Coast.
            ReplaceGroups("Warehouse", "Models/BMG/bmg_warehouse_mk1", "BMG_PortWarehouse_Authored", Vector3.one * 1.05f, new Color(.33f,.32f,.29f));
            ReplaceObjects("CargoContainer", "Models/BMG/bmg_shipping_container_mk1", "BMG_PortContainer_Authored", Vector3.one, new Color(.35f,.25f,.18f));
            ReplaceGroups("Barn", "Models/BMG/bmg_barn_mk1", "BMG_Barn_Authored", Vector3.one, new Color(.38f,.29f,.20f));
            ReplaceGroups("WaterTower", "Models/BMG/bmg_water_tower_mk1", "BMG_WaterTower_Authored", Vector3.one, new Color(.31f,.34f,.32f));
            ReplaceGroups("Lighthouse", "Models/BMG/bmg_lighthouse_mk1", "BMG_Lighthouse_Authored", Vector3.one, new Color(.62f,.58f,.49f));
            ReplaceGroups("CoastHut", "Models/BMG/bmg_coast_hut_mk1", "BMG_CoastHut_Authored", Vector3.one, new Color(.42f,.37f,.30f));
            ReplaceCranePairs();

            // Saltworks / Old Crown.
            ReplaceGroups("PumpHouse", "Models/BMG/bmg_pump_house_mk1", "BMG_PumpHouse_Authored", Vector3.one, new Color(.39f,.27f,.20f));
            ReplaceSaltPiles();
            ReplaceOldCrownHouses();
            ReplaceOldCrownMonument();
            ReplaceOldCrownCover();
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

        private static void ReplaceSaltPiles()
        {
            GameObject prefab = Resources.Load<GameObject>("Models/BMG/bmg_rock_cluster_mk1");
            if (prefab == null) return;
            Transform[] all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                Transform pile = all[i];
                if (pile == null || pile.name != "SaltPile" || pile.Find("BMG_SaltPile_Authored") != null) continue;
                Vector3 original = pile.localScale;
                HideRenderer(pile);
                GameObject model = Instantiate(prefab, pile, false);
                model.name = "BMG_SaltPile_Authored";
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = new Vector3(Mathf.Max(.8f, original.x * .55f), Mathf.Max(.7f, original.y * .75f), Mathf.Max(.8f, original.z * .55f));
                ApplyMaterial(model, new Color(.82f,.79f,.69f));
            }
        }

        private static void ReplaceOldCrownHouses()
        {
            GameObject prefab = Resources.Load<GameObject>("Models/BMG/bmg_old_crown_house_mk1");
            if (prefab == null) return;
            Transform[] all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                Transform house = all[i];
                if (house == null || !house.name.StartsWith("House_") || house.Find("BMG_OldCrownHouse_Authored") != null) continue;
                Transform body = house.Find("Body");
                if (body == null) continue;
                Vector3 bodyScale = body.localScale;
                HideRenderersRecursive(house);
                GameObject model = Instantiate(prefab, house, false);
                model.name = "BMG_OldCrownHouse_Authored";
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = new Vector3(
                    Mathf.Max(.45f, bodyScale.x / 8f),
                    Mathf.Max(.45f, bodyScale.y / 5.6f),
                    Mathf.Max(.45f, bodyScale.z / 8f));
                ApplyMaterial(model, (i & 1) == 0 ? new Color(.55f,.49f,.40f) : new Color(.45f,.41f,.35f));
            }
        }

        private static void ReplaceOldCrownMonument()
        {
            Transform tower = FindNamed("CrownTower");
            if (tower == null || tower.parent == null || tower.parent.Find("BMG_CrownMonument_Authored") != null) return;
            GameObject prefab = Resources.Load<GameObject>("Models/BMG/bmg_crown_monument_mk1");
            if (prefab == null) return;
            Transform parent = tower.parent;
            HideRenderer(tower);
            Transform top = parent.Find("CrownTop");
            HideRenderer(top);
            GameObject model = Instantiate(prefab, parent, false);
            model.name = "BMG_CrownMonument_Authored";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            ApplyMaterial(model, new Color(.48f,.43f,.35f));
        }

        private static void ReplaceOldCrownCover()
        {
            GameObject rock = Resources.Load<GameObject>("Models/BMG/bmg_rock_cluster_mk1");
            GameObject barricade = Resources.Load<GameObject>("Models/BMG/bmg_barricade_mk1");
            if (rock == null || barricade == null) return;
            Transform[] all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                Transform cover = all[i];
                if (cover == null || !cover.name.StartsWith("Cover_") || cover.childCount > 0) continue;
                Renderer renderer = cover.GetComponent<Renderer>();
                if (renderer == null) continue;
                Vector3 oldScale = cover.localScale;
                renderer.enabled = false;
                bool longCover = oldScale.x > oldScale.z * 1.8f;
                GameObject model = Instantiate(longCover ? barricade : rock, cover, false);
                model.name = longCover ? "BMG_CoverBarricade_Authored" : "BMG_CoverRock_Authored";
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = longCover ? new Vector3(Mathf.Max(.7f, oldScale.x / 2.8f), 1.15f, 1f) : Vector3.one * Mathf.Max(.65f, oldScale.x * .75f);
                ApplyMaterial(model, new Color(.46f,.41f,.34f));
            }
        }

        private static Transform FindNamed(string name)
        {
            Transform[] all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++) if (all[i] != null && all[i].name == name) return all[i];
            return null;
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
