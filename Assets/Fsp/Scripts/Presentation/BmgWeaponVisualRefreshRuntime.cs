using System.Collections;
using Fsp.Combat;
using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>Keeps the Build 149 authored weapon mesh synchronized with the currently active gameplay weapon.</summary>
    public sealed class BmgWeaponVisualRefreshRuntime : MonoBehaviour
    {
        private const string RiflePath = "Models/BMG/bmg_assault_rifle_mk1";
        private const string SmgPath = "Models/BMG/bmg_smg_mk1";
        private const string SniperPath = "Models/BMG/bmg_sniper_mk1";
        private const string ShotgunPath = "Models/BMG/bmg_shotgun_mk1";
        private const string OpticPath = "Models/BMG/bmg_weapon_optic_mk1";
        private static BmgWeaponVisualRefreshRuntime instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            GameObject host = new("BMG_WeaponVisualRefreshRuntime");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BmgWeaponVisualRefreshRuntime>();
            instance.StartCoroutine(instance.RefreshLoop());
        }

        private IEnumerator RefreshLoop()
        {
            WaitForSeconds wait = new(.25f);
            while (true)
            {
                RefreshCharacters();
                yield return wait;
            }
        }

        private static void RefreshCharacters()
        {
            StarterProceduralCharacterVisual[] characters = FindObjectsByType<StarterProceduralCharacterVisual>(FindObjectsSortMode.None);
            for (int i = 0; i < characters.Length; i++)
            {
                StarterProceduralCharacterVisual character = characters[i];
                if (character == null) continue;
                Transform root = character.transform.Find("FSP_CharacterVisual");
                Transform fallback = FindRecursive(root, "RifleVisual");
                if (fallback == null || fallback.parent == null) continue;

                HitscanWeapon weapon = FindActiveWeapon(character);
                WeaponConfig config = weapon != null ? weapon.Config : null;
                string desiredPath = PathFor(config);
                string desiredName = NameFor(config);
                Transform current = FindCurrentAuthoredWeapon(fallback.parent);
                if (current != null && current.name == desiredName) continue;
                if (current != null) Destroy(current.gameObject);

                GameObject prefab = Resources.Load<GameObject>(desiredPath);
                if (prefab == null) continue;
                GameObject model = Instantiate(prefab, fallback.parent, false);
                model.name = desiredName;
                model.transform.localPosition = fallback.localPosition;
                model.transform.localRotation = fallback.localRotation;
                model.transform.localScale = ScaleFor(config);
                ApplyMaterial(model, new Color(.10f, .11f, .11f));
                AddOptic(model, config);
            }
        }

        private static void AddOptic(GameObject weaponModel, WeaponConfig config)
        {
            if (weaponModel == null || config == null) return;
            if (config.weaponClass != WeaponClass.Assault && config.weaponClass != WeaponClass.Marksman) return;
            GameObject opticPrefab = Resources.Load<GameObject>(OpticPath);
            if (opticPrefab == null) return;
            GameObject optic = Instantiate(opticPrefab, weaponModel.transform, false);
            optic.name = "BMG_Optic_Authored";
            optic.transform.localPosition = config.weaponClass == WeaponClass.Marksman ? new Vector3(0f, .20f, .22f) : new Vector3(0f, .17f, .08f);
            optic.transform.localRotation = Quaternion.identity;
            optic.transform.localScale = config.weaponClass == WeaponClass.Marksman ? Vector3.one * .42f : Vector3.one * .30f;
            ApplyMaterial(optic, new Color(.055f, .06f, .06f));
        }

        private static Transform FindCurrentAuthoredWeapon(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == "BMG_Weapon_Authored" || child.name.StartsWith("BMG_Weapon_Authored_")) return child;
            }
            return null;
        }

        private static HitscanWeapon FindActiveWeapon(Component owner)
        {
            HitscanWeapon[] weapons = owner.GetComponentsInChildren<HitscanWeapon>(true);
            for (int i = 0; i < weapons.Length; i++)
                if (weapons[i] != null && weapons[i].enabled && weapons[i].gameObject.activeInHierarchy) return weapons[i];
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

        private static string NameFor(WeaponConfig config)
        {
            string suffix = config == null ? "Assault" : config.weaponClass.ToString();
            return "BMG_Weapon_Authored_" + suffix;
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
