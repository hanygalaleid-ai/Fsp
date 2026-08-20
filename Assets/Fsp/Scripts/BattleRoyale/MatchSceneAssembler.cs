using System;
using Fsp.Backend;
using Fsp.Bots;
using Fsp.Combat;
using Fsp.Core;
using Fsp.Inventory;
using Fsp.Player;
using Fsp.UI;
using Fsp.Vehicles;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.BattleRoyale
{
    /// <summary>
    /// Gameplay safety assembler for the checked-in Match scene.
    /// Prefer authored scene objects, but guarantee a usable local player on device builds
    /// so the match cannot open as an empty, non-interactive scene.
    /// </summary>
    public sealed class MatchSceneAssembler : MonoBehaviour
    {
        private MatchParticipant localParticipant;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallForMatchScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<MatchSceneAssembler>() != null) return;

            new GameObject("MatchSceneAssembler").AddComponent<MatchSceneAssembler>();
            Debug.Log("FSP Match: runtime safety assembler installed.");
        }

        private void Awake()
        {
            EnsureMatchManager();
            localParticipant = FindLocalPlayer();
            if (localParticipant == null)
            {
                Debug.LogWarning("FSP Match: no authored local MatchParticipant found. Creating runtime safety player so the match remains playable.");
                localParticipant = CreateRuntimeSafetyPlayer();
            }

            if (localParticipant == null)
            {
                Debug.LogError("FSP Match: failed to create a local player.");
                return;
            }

            EnsureGameplayComponents(localParticipant.gameObject);
            EnsureStarterCombatLoadout(localParticipant.gameObject);

            // Do these explicitly here instead of depending only on RuntimeInitialize ordering.
            // Unity does not guarantee ordering between multiple AfterSceneLoad callbacks, so the
            // world and Android controls must be installed after the local participant exists.
            StarterWorldGameplayInstaller.EnsureInstalled();
            MobileMatchControlsInstaller.Install();
            EnsureOfflineOpponent();

            WireExistingHud(localParticipant.gameObject);
            Debug.Log("FSP Match: runtime path ready (manager, player, starter weapon, world, mobile controls and offline opponent fallback).");
        }

        private static MatchManager EnsureMatchManager()
        {
            MatchManager existing = FindFirstObjectByType<MatchManager>();
            return existing != null ? existing : new GameObject("MatchManager").AddComponent<MatchManager>();
        }

        private static MatchParticipant FindLocalPlayer()
        {
            foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
            {
                if (participant != null && participant.IsLocalPlayer)
                    return participant;
            }
            return null;
        }

        private static MatchParticipant CreateRuntimeSafetyPlayer()
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "RuntimeLocalPlayer";
            player.transform.position = new Vector3(0f, 2f, 0f);

            CapsuleCollider primitiveCollider = player.GetComponent<CapsuleCollider>();
            if (primitiveCollider != null) Destroy(primitiveCollider);

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.center = new Vector3(0f, 1f, 0f);

            Renderer renderer = player.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Standard");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader != null)
                {
                    Material material = new Material(shader);
                    material.color = new Color(0.19f, 0.28f, 0.18f, 1f);
                    renderer.sharedMaterial = material;
                }
            }

            MatchParticipant participant = player.AddComponent<MatchParticipant>();
            participant.ConfigureAsLocalPlayer("Player");

            if (GameObject.Find("RuntimeSafetyGround") == null)
            {
                GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "RuntimeSafetyGround";
                ground.transform.position = Vector3.zero;
                ground.transform.localScale = new Vector3(12f, 1f, 12f);
            }

            return participant;
        }

        private static void EnsureGameplayComponents(GameObject player)
        {
            if (player == null) return;
            if (player.GetComponent<PlayerDamageable>() == null) player.AddComponent<PlayerDamageable>();
            if (player.GetComponent<PlayerVitals>() == null) player.AddComponent<PlayerVitals>();
            if (player.GetComponent<ThirdPersonMotor>() == null) player.AddComponent<ThirdPersonMotor>();
            if (player.GetComponent<ParachuteController>() == null) player.AddComponent<ParachuteController>();
            if (player.GetComponent<PlayerInventory>() == null) player.AddComponent<PlayerInventory>();
            if (player.GetComponent<SafeZoneDamageApplier>() == null) player.AddComponent<SafeZoneDamageApplier>();
            if (player.GetComponent<StarterThirdPersonRig>() == null) player.AddComponent<StarterThirdPersonRig>();
            if (player.GetComponent<StarterVehicleInput>() == null) player.AddComponent<StarterVehicleInput>();
            if (player.GetComponent<StarterInteractInput>() == null) player.AddComponent<StarterInteractInput>();
        }

        private static void EnsureStarterCombatLoadout(GameObject player)
        {
            if (player == null) return;
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null) return;

            // Preserve an authored loadout when one exists.
            if (inventory.PrimaryWeapon != null || inventory.SecondaryWeapon != null) return;

            Camera aimCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            if (aimCamera == null)
            {
                Debug.LogError("FSP Match: starter weapon could not be created because no gameplay camera exists.");
                return;
            }

            GameObject weaponObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            weaponObject.name = "RuntimeStarterRifle";
            weaponObject.transform.SetParent(player.transform, false);
            weaponObject.transform.localPosition = new Vector3(0.42f, 1.25f, 0.42f);
            weaponObject.transform.localRotation = Quaternion.identity;
            weaponObject.transform.localScale = new Vector3(0.12f, 0.12f, 0.62f);

            Collider weaponCollider = weaponObject.GetComponent<Collider>();
            if (weaponCollider != null) Destroy(weaponCollider);

            GameObject muzzleObject = new GameObject("Muzzle");
            muzzleObject.transform.SetParent(weaponObject.transform, false);
            muzzleObject.transform.localPosition = new Vector3(0f, 0f, 0.58f);

            WeaponConfig config = ScriptableObject.CreateInstance<WeaponConfig>();
            config.weaponId = "runtime_rifle";
            config.displayName = "Starter Rifle";
            config.weaponClass = WeaponClass.Assault;
            config.ammoClass = AmmoClass.Medium;
            config.damage = 28f;
            config.range = 180f;
            config.roundsPerSecond = 9f;
            config.magazineSize = 30;
            config.reloadSeconds = 2.0f;
            config.spreadDegrees = 0.8f;

            HitscanWeapon weapon = weaponObject.AddComponent<HitscanWeapon>();
            weapon.Configure(config, aimCamera, muzzleObject.transform, inventory);
            inventory.ConfigureStarterLoadout(weapon, null, 90, 0, 2);

            Debug.Log("FSP Match: runtime starter rifle and reserve ammo installed.");
        }

        private static void EnsureOfflineOpponent()
        {
            // Never synthesize opponents inside an online room; networking owns population there.
            if (MatchRoomState.HasMatch) return;

            MatchParticipant[] participants = FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None);
            foreach (MatchParticipant participant in participants)
                if (participant != null && participant.IsBot) return;

            GameObject spawnerObject = GameObject.Find("RuntimeOfflineBotSpawner") ?? new GameObject("RuntimeOfflineBotSpawner");
            BotSpawner spawner = spawnerObject.GetComponent<BotSpawner>();
            if (spawner == null) spawner = spawnerObject.AddComponent<BotSpawner>();

            if (!spawner.TrySpawnOne())
            {
                Debug.LogError("FSP Match: failed to create the offline opponent fallback.");
                return;
            }

            Debug.Log("FSP Match: offline opponent fallback created; local match can leave Waiting state.");
        }

        private static void WireExistingHud(GameObject player)
        {
            if (player == null) return;
            BattleRoyaleHud hud = FindFirstObjectByType<BattleRoyaleHud>();
            if (hud == null)
            {
                Debug.LogWarning("FSP Match: authored BattleRoyaleHud not found; mobile combat controls remain active and gameplay continues.");
                return;
            }

            hud.ConfigureSources(
                player.GetComponent<PlayerVitals>(),
                player.GetComponent<PlayerInventory>(),
                FindFirstObjectByType<MatchManager>(),
                FindFirstObjectByType<SafeZoneController>(),
                player.transform);
        }
    }
}
