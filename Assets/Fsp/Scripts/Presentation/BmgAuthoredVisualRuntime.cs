using System.Collections;
using Fsp.BattleRoyale;
using Fsp.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>Build 149 authored-art bridge. Authored Resources meshes are authoritative; generated art is fallback only.</summary>
    public sealed class BmgAuthoredVisualRuntime : MonoBehaviour
    {
        private const string RiflePath = "Models/BMG/bmg_assault_rifle_mk1";
        private const string SmgPath = "Models/BMG/bmg_smg_mk1";
        private const string SniperPath = "Models/BMG/bmg_sniper_mk1";
        private const string ShotgunPath = "Models/BMG/bmg_shotgun_mk1";
        private const string HelmetPath = "Models/BMG/bmg_helmet_mk1";
        private const string BackpackPath = "Models/BMG/bmg_backpack_mk1";
        private const string BuggyPath = "Models/BMG/bmg_buggy_mk1";
        private const string DesertCarPath = "Models/BMG/bmg_desert_car_mk1";
        private const string PlanePath = "Models/BMG/bmg_transport_plane_mk1";
        private const string MaleTorsoPath = "Models/BMG/bmg_male_torso_mk1";
        private const string FemaleTorsoPath = "Models/BMG/bmg_female_torso_mk1";
        private const string HeadPath = "Models/BMG/bmg_head_mk1";
        private const string ArmPath = "Models/BMG/bmg_arm_mk1";
        private const string LegPath = "Models/BMG/bmg_leg_mk1";
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

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StartCoroutine(ApplyWhenReady());

        private IEnumerator ApplyWhenReady()
        {
            yield return null;
            yield return null;
            UpgradeCharacters();
            UpgradeVehicles();
            UpgradePlanes();
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

                MatchParticipant participant = character.GetComponent<MatchParticipant>();
                bool bot = participant != null && participant.IsBot;
                string selected = Fsp.Lobby.LobbyState.Instance != null ? Fsp.Lobby.LobbyState.Instance.SelectedCharacterId : "soldier_01";
                bool female = bot ? (character.GetInstanceID() & 1) == 0 : selected == "soldier_03";
                string torsoPath = female ? FemaleTorsoPath : MaleTorsoPath;
                Color uniform = female ? new Color(.24f, .34f, .28f) : new Color(.26f, .31f, .23f);
                Color skin = female ? new Color(.58f, .38f, .27f) : new Color(.47f, .31f, .22f);

                ReplaceWithAuthored(FindRecursive(visualRoot, "Torso"), torsoPath, "BMG_Torso_Authored", Vector3.one * .78f, uniform);
                ReplaceWithAuthored(FindRecursive(visualRoot, "Head"), HeadPath, "BMG_Head_Authored", Vector3.one * .58f, skin);
                ReplaceWithAuthored(FindRecursive(visualRoot, "LeftArm"), ArmPath, "BMG_LeftArm_Authored", Vector3.one * 1.65f, uniform);
                ReplaceWithAuthored(FindRecursive(visualRoot, "RightArm"), ArmPath, "BMG_RightArm_Authored", Vector3.one * 1.65f, uniform);
                ReplaceWithAuthored(FindRecursive(visualRoot, "LeftLeg"), LegPath, "BMG_LeftLeg_Authored", Vector3.one * 1.80f, uniform);
                ReplaceWithAuthored(FindRecursive(visualRoot, "RightLeg"), LegPath, "BMG_RightLeg_Authored", Vector3.one * 1.80f, uniform);
                ReplaceWithAuthored(FindRecursive(visualRoot, "Helmet"), HelmetPath, "BMG_Helmet_Authored", Vector3.one * 1.08f, new Color(.16f, .20f, .17f));
                ReplaceWithAuthored(FindRecursive(visualRoot, "Backpack"), BackpackPath, "BMG_Backpack_Authored", Vector3.one * .95f, new Color(.20f, .24f, .18f));

                HitscanWeapon activeWeapon = FindActiveWeapon(character);
                string weaponPath = PathFor(activeWeapon != null ? activeWeapon.Config : null);
                ReplaceWithAuthored(FindRecursive(visualRoot, "RifleVisual"), weaponPath, "BMG_Weapon_Authored", ScaleFor(activeWeapon != null ? activeWeapon.Config : null), new Color(.10f, .11f, .11f));
            }
        }

        private static HitscanWeapon FindActiveWeapon(Component owner)
        {
            HitscanWeapon[] weapons = owner.GetComponentsInChildren<HitscanWeapon>(true);
            for (int i = 0; i < weapons.Length; i++)
                if (weapons[i] != null && weapons[i].gameObject.activeInHierarchy && weapons[i].enabled) return weapons[i];
            return weapons.Length > 0 ? weapons[0] : null;
        }

        private static string PathFor(WeaponConfig config)
        {
            if (config == null) return RiflePath;
            return config.weaponClass switch
            {
                WeaponClass.SMG => SmgPath,
                WeaponClass.Marksman => SniperPath,
                WeaponClass.Shotgun => ShotgunPath,
                _ => RiflePath
            };
        }

        private static Vector3 ScaleFor(WeaponConfig config)
        {
            if (config == null) return Vector3.one * .72f;
            return config.weaponClass switch
            {
                WeaponClass.SMG => Vector3.one * .66f,
                WeaponClass.Marksman => Vector3.one * .76f,
                WeaponClass.Shotgun => Vector3.one * .74f,
                _ => Vector3.one * .72f
            };
        }

        private static void UpgradeVehicles()
        {
            StarterProceduralVehicleVisual[] vehicles = FindObjectsByType<StarterProceduralVehicleVisual>(FindObjectsSortMode.None);
            for (int i = 0; i < vehicles.Length; i++)
            {
                StarterProceduralVehicleVisual vehicle = vehicles[i];
                if (vehicle == null || vehicle.transform.Find("BMG_Vehicle_Authored") != null) continue;
                string path = (i & 1) == 0 ? BuggyPath : DesertCarPath;
                GameObject authored = Resources.Load<GameObject>(path);
                if (authored == null) authored = Resources.Load<GameObject>(BuggyPath);
                if (authored == null) continue;

                Transform old = vehicle.transform.Find("FSP_ScoutVehicleVisual");
                if (old != null) old.gameObject.SetActive(false);
                GameObject model = Instantiate(authored, vehicle.transform, false);
                model.name = "BMG_Vehicle_Authored";
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one * .92f;
                ApplyMobileMaterial(model, (i & 1) == 0 ? new Color(.18f, .24f, .13f) : new Color(.31f, .27f, .19f));
            }
        }

        private static void UpgradePlanes()
        {
            StarterPlaneVisual[] planes = FindObjectsByType<StarterPlaneVisual>(FindObjectsSortMode.None);
            GameObject authored = Resources.Load<GameObject>(PlanePath);
            if (authored == null) return;
            for (int i = 0; i < planes.Length; i++)
            {
                StarterPlaneVisual plane = planes[i];
                if (plane == null || plane.transform.Find("BMG_TransportPlane_Authored") != null) continue;
                Transform old = plane.transform.Find("FSP_TransportPlaneVisual");
                if (old != null) old.gameObject.SetActive(false);
                GameObject model = Instantiate(authored, plane.transform, false);
                model.name = "BMG_TransportPlane_Authored";
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;
                ApplyMobileMaterial(model, new Color(.34f, .40f, .35f));
            }
        }

        private static void ReplaceWithAuthored(Transform oldVisual, string resourcePath, string newName, Vector3 scale, Color color)
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
            ApplyMobileMaterial(model, color);
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
            Material material = new(shader) { color = color, hideFlags = HideFlags.DontSave };
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++) if (renderers[i] != null) renderers[i].sharedMaterial = material;
        }
    }
}
