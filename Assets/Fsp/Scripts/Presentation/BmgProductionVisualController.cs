using System;
using System.Collections;
using Fsp.BattleRoyale;
using Fsp.Combat;
using Fsp.Lobby;
using Fsp.Vehicles;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>
    /// Production-only visual authority for Match. Compatibility gameplay/colliders may exist,
    /// but only assets under Resources/Models/BMG/Production are allowed to render.
    /// </summary>
    public sealed class BmgProductionVisualController : MonoBehaviour
    {
        private const string ProductionRoot = "BMG_PRODUCTION_";
        private const string EnvironmentPath = "Models/BMG/Production/bmg_sunscar_environment";
        private const string PlanePath = "Models/BMG/Production/bmg_transport_plane";
        private const string ParachutePath = "Models/BMG/Production/bmg_parachute";
        private const string BuggyPath = "Models/BMG/Production/bmg_buggy";
        private const string RiflePath = "Models/BMG/Production/bmg_assault_rifle";
        private const string SmgPath = "Models/BMG/Production/bmg_smg";

        private static readonly string[] LegacyWorldRoots =
        {
            "OldCrown_Interiors", "OldCrown_Doors", "CopperPort_Prototype", "Dryfield_Prototype",
            "WhiteQuarry_Prototype", "RedlineAirstrip_Prototype", "Saltworks_Prototype", "LanternCoast_Prototype",
            "OldCrown_CopperPort_Road", "Dryfield_Road_Links", "WhiteQuarry_Road_Links",
            "RedlineAirstrip_Road_Links", "Saltworks_Road_Links", "LanternCoast_Road_Links",
            "SunscarIsland_Polish", "SunscarRuntimeWorld"
        };

        private static BmgProductionVisualController instance;
        private static Material environmentMaterial;
        private static Material characterMaterial;
        private static Material weaponMaterial;
        private static Material planeMaterial;
        private static Material vehicleMaterial;
        private static Material parachuteMaterial;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            GameObject host = new("BMG_ProductionVisualController");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BmgProductionVisualController>();
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
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", StringComparison.OrdinalIgnoreCase)) yield break;
            for (int pass = 0; pass < 12; pass++)
            {
                yield return pass == 0 ? null : new WaitForSecondsRealtime(.25f);
                Apply();
            }
        }

        private static void Apply()
        {
            HideLegacyWorldRenderers();
            EnsureProductionEnvironment();

            foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
            {
                ApplyCharacter(participant);
                ApplyParachute(participant);
            }

            foreach (DropPlaneController plane in FindObjectsByType<DropPlaneController>(FindObjectsSortMode.None))
                ApplyModel(plane.transform, PlanePath, "BMG_PRODUCTION_PLANE", Vector3.one);

            foreach (SimpleVehicleController vehicle in FindObjectsByType<SimpleVehicleController>(FindObjectsSortMode.None))
                ApplyModel(vehicle.transform, BuggyPath, "BMG_PRODUCTION_VEHICLE", Vector3.one);

            foreach (HitscanWeapon weapon in FindObjectsByType<HitscanWeapon>(FindObjectsSortMode.None))
            {
                string path = weapon.Config != null && weapon.Config.weaponClass == WeaponClass.SMG ? SmgPath : RiflePath;
                ApplyModel(weapon.transform, path, "BMG_PRODUCTION_WEAPON", Vector3.one);
            }
        }

        private static void ApplyCharacter(MatchParticipant participant)
        {
            if (participant == null) return;
            string id = LobbyState.Instance != null ? LobbyState.Instance.SelectedCharacterId : "soldier_01";
            int index = 1;
            if (!string.IsNullOrWhiteSpace(id) && id.StartsWith("soldier_", StringComparison.OrdinalIgnoreCase))
                int.TryParse(id.Substring("soldier_".Length), out index);
            index = Mathf.Clamp(index, 1, 6);
            ApplyModel(participant.transform, $"Models/BMG/Production/bmg_character_{index:00}", "BMG_PRODUCTION_CHARACTER", Vector3.one);
        }

        private static void ApplyParachute(MatchParticipant participant)
        {
            if (participant == null) return;
            ParachuteController controller = participant.GetComponent<ParachuteController>();
            if (controller == null) return;
            Transform marker = participant.transform.Find("BMG_PRODUCTION_PARACHUTE");
            if (marker == null)
            {
                GameObject prefab = Resources.Load<GameObject>(ParachutePath);
                if (prefab == null) return;
                GameObject model = Instantiate(prefab, participant.transform, false);
                model.name = "BMG_PRODUCTION_PARACHUTE";
                model.transform.localPosition = new Vector3(0f, 1.65f, 0f);
                model.transform.localRotation = Quaternion.identity;
                ApplyProductionMaterial(model.transform, "BMG_PRODUCTION_PARACHUTE");
                marker = model.transform;
            }
            controller.ConfigureVisual(marker.gameObject);
        }

        private static void EnsureProductionEnvironment()
        {
            if (GameObject.Find("BMG_PRODUCTION_ENVIRONMENT") != null) return;
            GameObject prefab = Resources.Load<GameObject>(EnvironmentPath);
            if (prefab == null) return;
            GameObject environment = Instantiate(prefab);
            environment.name = "BMG_PRODUCTION_ENVIRONMENT";
            environment.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            ApplyProductionMaterial(environment.transform, environment.name);
        }

        private static void HideLegacyWorldRenderers()
        {
            foreach (string rootName in LegacyWorldRoots)
            {
                GameObject root = GameObject.Find(rootName);
                if (root != null) HideLegacyRenderers(root.transform);
            }
            GameObject safetyGround = GameObject.Find("RuntimeSafetyGround");
            if (safetyGround != null) HideLegacyRenderers(safetyGround.transform);
        }

        private static void ApplyModel(Transform owner, string resourcePath, string markerName, Vector3 scale)
        {
            if (owner == null) return;
            Transform marker = owner.Find(markerName);
            HideLegacyRenderers(owner);
            if (marker != null)
            {
                SetProductionRenderers(marker, true);
                ApplyProductionMaterial(marker, markerName);
                return;
            }
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null) return;
            GameObject model = Instantiate(prefab, owner, false);
            model.name = markerName;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = scale;
            ApplyProductionMaterial(model.transform, markerName);
            SetProductionRenderers(model.transform, true);
        }

        private static void ApplyProductionMaterial(Transform root, string markerName)
        {
            Material material = MaterialFor(markerName);
            if (material == null || root == null) return;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                if (renderer != null) renderer.sharedMaterial = material;
        }

        private static Material MaterialFor(string markerName)
        {
            if (markerName.Contains("ENVIRONMENT"))
                return environmentMaterial ??= CreateMaterial("BMG_Environment", new Color(.44f, .36f, .24f, 1f), .22f, .08f);
            if (markerName.Contains("CHARACTER"))
                return characterMaterial ??= CreateMaterial("BMG_TacticalCharacter", new Color(.16f, .20f, .13f, 1f), .36f, .10f);
            if (markerName.Contains("WEAPON"))
                return weaponMaterial ??= CreateMaterial("BMG_Gunmetal", new Color(.075f, .085f, .09f, 1f), .72f, .38f);
            if (markerName.Contains("PLANE"))
                return planeMaterial ??= CreateMaterial("BMG_MilitaryPlane", new Color(.19f, .23f, .23f, 1f), .42f, .18f);
            if (markerName.Contains("VEHICLE"))
                return vehicleMaterial ??= CreateMaterial("BMG_DesertVehicle", new Color(.31f, .28f, .20f, 1f), .48f, .16f);
            if (markerName.Contains("PARACHUTE"))
                return parachuteMaterial ??= CreateMaterial("BMG_ParachuteOrange", new Color(.80f, .24f, .025f, 1f), .18f, .04f);
            return null;
        }

        private static Material CreateMaterial(string name, Color color, float smoothness, float metallic)
        {
            Shader shader = Resources.Load<Shader>("Shaders/FspMobileSafe");
            if (shader == null) shader = Shader.Find("Fsp/MobileSafeLit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) return null;

            Material material = new(shader) { name = name, color = color, hideFlags = HideFlags.DontSave };
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            return material;
        }

        private static void HideLegacyRenderers(Transform root)
        {
            if (root == null) return;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                if (renderer != null && !IsProductionRenderer(renderer.transform)) renderer.enabled = false;
        }

        private static bool IsProductionRenderer(Transform transform)
        {
            for (Transform current = transform; current != null; current = current.parent)
                if (current.name.StartsWith(ProductionRoot, StringComparison.Ordinal)) return true;
            return false;
        }

        private static void SetProductionRenderers(Transform root, bool enabled)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                if (renderer != null) renderer.enabled = enabled;
        }
    }
}
