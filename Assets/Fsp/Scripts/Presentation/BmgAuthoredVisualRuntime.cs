using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>
    /// Build 149 authored-art bridge. Imported 3D assets in Resources/Models/BMG are
    /// preferred over the legacy procedural presentation without changing gameplay.
    /// Procedural visuals remain only as a safe fallback when an authored asset is absent.
    /// </summary>
    public sealed class BmgAuthoredVisualRuntime : MonoBehaviour
    {
        private const string RiflePath = "Models/BMG/bmg_assault_rifle_mk1";
        private const string BackpackPath = "Models/BMG/bmg_backpack_mk1";
        private const string BuggyPath = "Models/BMG/bmg_buggy_mk1";
        private static BmgAuthoredVisualRuntime instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            GameObject host = new("BMG_AuthoredVisualRuntime");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BmgAuthoredVisualRuntime>();
            SceneManager.sceneLoaded += instance.OnSceneLoaded;
            instance.StartCoroutine(instance.ApplyWhenReady());
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(ApplyWhenReady());
        }

        private IEnumerator ApplyWhenReady()
        {
            // Runtime assemblies build their fallback visuals during Awake/Start.
            yield return null;
            yield return null;
            UpgradeCharacters();
            UpgradeVehicles();
        }

        private static void UpgradeCharacters()
        {
            StarterProceduralCharacterVisual[] characters = FindObjectsByType<StarterProceduralCharacterVisual>(FindObjectsSortMode.None);
            for (int i = 0; i < characters.Length; i++)
            {
                StarterProceduralCharacterVisual character = characters[i];
                if (character == null) continue;
                Transform visualRoot = character.transform.Find("FSP_CharacterVisual");
                if (visualRoot == null) continue;

                Transform backpack = FindRecursive(visualRoot, "Backpack");
                ReplaceWithAuthored(backpack, BackpackPath, "BMG_Backpack_Authored", new Vector3(.95f, .95f, .95f));

                Transform rifle = FindRecursive(visualRoot, "RifleVisual");
                ReplaceWithAuthored(rifle, RiflePath, "BMG_Rifle_Authored", new Vector3(.72f, .72f, .72f));
            }
        }

        private static void UpgradeVehicles()
        {
            StarterProceduralVehicleVisual[] vehicles = FindObjectsByType<StarterProceduralVehicleVisual>(FindObjectsSortMode.None);
            GameObject authored = Resources.Load<GameObject>(BuggyPath);
            if (authored == null) return;

            for (int i = 0; i < vehicles.Length; i++)
            {
                StarterProceduralVehicleVisual vehicle = vehicles[i];
                if (vehicle == null || vehicle.transform.Find("BMG_Buggy_Authored") != null) continue;

                Transform old = vehicle.transform.Find("FSP_ScoutVehicleVisual");
                if (old != null) old.gameObject.SetActive(false);

                GameObject model = Instantiate(authored, vehicle.transform, false);
                model.name = "BMG_Buggy_Authored";
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one * .92f;
                ApplyMobileMaterial(model, new Color(.18f, .24f, .13f));
            }
        }

        private static void ReplaceWithAuthored(Transform oldVisual, string resourcePath, string newName, Vector3 scale)
        {
            if (oldVisual == null || oldVisual.parent == null) return;
            if (oldVisual.parent.Find(newName) != null) return;
            GameObject authored = Resources.Load<GameObject>(resourcePath);
            if (authored == null) return;

            Transform parent = oldVisual.parent;
            Vector3 position = oldVisual.localPosition;
            Quaternion rotation = oldVisual.localRotation;
            oldVisual.gameObject.SetActive(false);

            GameObject model = Instantiate(authored, parent, false);
            model.name = newName;
            model.transform.localPosition = position;
            model.transform.localRotation = rotation;
            model.transform.localScale = scale;
            ApplyMobileMaterial(model, resourcePath == RiflePath ? new Color(.10f, .11f, .11f) : new Color(.20f, .24f, .18f));
        }

        private static Transform FindRecursive(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindRecursive(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        private static void ApplyMobileMaterial(GameObject root, Color color)
        {
            Shader shader = Resources.Load<Shader>("Shaders/FspMobileSafe");
            if (shader == null) shader = Shader.Find("Fsp/MobileSafeLit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) return;

            Material material = new(shader)
            {
                color = color,
                hideFlags = HideFlags.DontSave
            };

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) renderers[i].sharedMaterial = material;
            }
        }
    }
}
