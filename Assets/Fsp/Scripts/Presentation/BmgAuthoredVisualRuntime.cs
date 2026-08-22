using System.Collections;
using Fsp.BattleRoyale;
using Fsp.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>
    /// Strict BMG presentation mode. Gameplay objects remain authoritative, but all legacy
    /// procedural render geometry is hidden whenever an authored BMG model is available.
    /// Authored prefab materials are preserved; no flat color material is forced at runtime.
    /// </summary>
    public sealed class BmgAuthoredVisualRuntime : MonoBehaviour
    {
        private const string RiflePath = "Models/BMG/bmg_assault_rifle_mk1";
        private const string SmgPath = "Models/BMG/bmg_smg_mk1";
        private const string SniperPath = "Models/BMG/bmg_sniper_mk1";
        private const string ShotgunPath = "Models/BMG/bmg_shotgun_mk1";
        private const string HelmetPath = "Models/BMG/bmg_helmet_mk1";
        private const string FaceMaskPath = "Models/BMG/bmg_face_mask_mk1";
        private const string BackpackPath = "Models/BMG/bmg_backpack_mk1";
        private const string VestPath = "Models/BMG/bmg_tactical_vest_mk1";
        private const string BootPath = "Models/BMG/bmg_combat_boot_mk1";
        private const string GlovePath = "Models/BMG/bmg_tactical_glove_mk1";
        private const string KneePath = "Models/BMG/bmg_knee_pad_mk1";
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
            var host = new GameObject("BMG_StrictAuthoredVisualRuntime");
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
            // Re-run while procedural scene builders finish so legacy visuals cannot reappear.
            for (int pass = 0; pass < 8; pass++)
            {
                yield return pass == 0 ? null : new WaitForSeconds(.25f);
                UpgradeCharacters();
                UpgradeVehicles();
                UpgradePlanes();
            }
        }

        private static void UpgradeCharacters()
        {
            var characters = FindObjectsByType<StarterProceduralCharacterVisual>(FindObjectsSortMode.None);
            foreach (var character in characters)
            {
                if (character == null) continue;
                var root = character.transform.Find("FSP_CharacterVisual");
                if (root == null) continue;

                var participant = character.GetComponent<MatchParticipant>();
                bool bot = participant != null && participant.IsBot;
                string selected = Fsp.Lobby.LobbyState.Instance != null ? Fsp.Lobby.LobbyState.Instance.SelectedCharacterId : "soldier_01";
                bool female = bot ? (character.GetInstanceID() & 1) == 0 : selected == "soldier_03";

                Replace(FindRecursive(root, "Torso"), female ? FemaleTorsoPath : MaleTorsoPath, "BMG_Torso_Authored", Vector3.one * .78f);
                Replace(FindRecursive(root, "Vest"), VestPath, "BMG_Vest_Authored", Vector3.one * 1.02f);
                Replace(FindRecursive(root, "Head"), HeadPath, "BMG_Head_Authored", Vector3.one * .58f);
                Replace(FindRecursive(root, "FaceMask"), FaceMaskPath, "BMG_FaceMask_Authored", Vector3.one * 1.05f);
                Replace(FindRecursive(root, "LeftArm"), ArmPath, "BMG_LeftArm_Authored", Vector3.one * 1.65f);
                Replace(FindRecursive(root, "RightArm"), ArmPath, "BMG_RightArm_Authored", Vector3.one * 1.65f);
                Replace(FindRecursive(root, "LeftLeg"), LegPath, "BMG_LeftLeg_Authored", Vector3.one * 1.80f);
                Replace(FindRecursive(root, "RightLeg"), LegPath, "BMG_RightLeg_Authored", Vector3.one * 1.80f);
                Replace(FindRecursive(root, "LeftHand"), GlovePath, "BMG_LeftGlove_Authored", Vector3.one * .78f);
                Replace(FindRecursive(root, "RightHand"), GlovePath, "BMG_RightGlove_Authored", Vector3.one * .78f);
                Replace(FindRecursive(root, "LeftKnee"), KneePath, "BMG_LeftKnee_Authored", Vector3.one * .72f);
                Replace(FindRecursive(root, "RightKnee"), KneePath, "BMG_RightKnee_Authored", Vector3.one * .72f);
                Replace(FindRecursive(root, "LeftBoot"), BootPath, "BMG_LeftBoot_Authored", Vector3.one * 1.20f);
                Replace(FindRecursive(root, "RightBoot"), BootPath, "BMG_RightBoot_Authored", Vector3.one * 1.20f);
                Replace(FindRecursive(root, "Helmet"), HelmetPath, "BMG_Helmet_Authored", Vector3.one * 1.08f);
                Replace(FindRecursive(root, "Backpack"), BackpackPath, "BMG_Backpack_Authored", Vector3.one * .95f);

                var active = FindActiveWeapon(character);
                Replace(FindRecursive(root, "RifleVisual"), PathFor(active != null ? active.Config : null), "BMG_Weapon_Authored", ScaleFor(active != null ? active.Config : null));
            }
        }

        private static HitscanWeapon FindActiveWeapon(Component owner)
        {
            var weapons = owner.GetComponentsInChildren<HitscanWeapon>(true);
            foreach (var weapon in weapons)
                if (weapon != null && weapon.gameObject.activeInHierarchy && weapon.enabled) return weapon;
            return weapons.Length > 0 ? weapons[0] : null;
        }

        private static string PathFor(WeaponConfig config) => config == null ? RiflePath : config.weaponClass switch
        {
            WeaponClass.SMG => SmgPath,
            WeaponClass.Marksman => SniperPath,
            WeaponClass.Shotgun => ShotgunPath,
            _ => RiflePath
        };

        private static Vector3 ScaleFor(WeaponConfig config) => Vector3.one * (config == null ? .72f : config.weaponClass switch
        {
            WeaponClass.SMG => .66f,
            WeaponClass.Marksman => .76f,
            WeaponClass.Shotgun => .74f,
            _ => .72f
        });

        private static void UpgradeVehicles()
        {
            var vehicles = FindObjectsByType<StarterProceduralVehicleVisual>(FindObjectsSortMode.None);
            for (int i = 0; i < vehicles.Length; i++)
            {
                var vehicle = vehicles[i];
                if (vehicle == null || vehicle.transform.Find("BMG_Vehicle_Authored") != null) continue;
                string path = (i & 1) == 0 ? BuggyPath : DesertCarPath;
                var prefab = Resources.Load<GameObject>(path) ?? Resources.Load<GameObject>(BuggyPath);
                if (prefab == null) continue;

                var old = vehicle.transform.Find("FSP_ScoutVehicleVisual");
                if (old != null) DisableRenderers(old);
                var model = Instantiate(prefab, vehicle.transform, false);
                model.name = "BMG_Vehicle_Authored";
                model.transform.localScale = Vector3.one * .92f;
            }
        }

        private static void UpgradePlanes()
        {
            var planes = FindObjectsByType<StarterPlaneVisual>(FindObjectsSortMode.None);
            var prefab = Resources.Load<GameObject>(PlanePath);
            if (prefab == null) return;
            foreach (var plane in planes)
            {
                if (plane == null || plane.transform.Find("BMG_TransportPlane_Authored") != null) continue;
                var old = plane.transform.Find("FSP_TransportPlaneVisual");
                if (old != null) DisableRenderers(old);
                var model = Instantiate(prefab, plane.transform, false);
                model.name = "BMG_TransportPlane_Authored";
            }
        }

        private static void Replace(Transform old, string path, string name, Vector3 scale)
        {
            if (old == null || old.parent == null || old.parent.Find(name) != null) return;
            var prefab = Resources.Load<GameObject>(path);
            if (prefab == null) return;

            Transform parent = old.parent;
            Vector3 position = old.localPosition;
            Quaternion rotation = old.localRotation;
            DisableRenderers(old);

            var model = Instantiate(prefab, parent, false);
            model.name = name;
            model.transform.localPosition = position;
            model.transform.localRotation = rotation;
            model.transform.localScale = scale;
            // Preserve authored/imported materials and textures exactly as supplied by the asset.
        }

        private static void DisableRenderers(Transform root)
        {
            if (root == null) return;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
                if (renderer != null) renderer.enabled = false;
        }

        private static Transform FindRecursive(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindRecursive(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
