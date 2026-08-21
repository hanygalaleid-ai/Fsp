using System;
using Fsp.Backend;
using Fsp.Bots;
using Fsp.Combat;
using Fsp.Core;
using Fsp.Inventory;
using Fsp.Lobby;
using Fsp.Networking;
using Fsp.Player;
using Fsp.Presentation;
using Fsp.UI;
using Fsp.Vehicles;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.BattleRoyale
{
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
            EnsureSafeZone();

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
            EnsureDropFlow(localParticipant.gameObject);
            StarterWorldGameplayInstaller.EnsureInstalled();
            AndroidMaterialRecovery.EnsureInstalled();
            MobileMatchControlsInstaller.Install();
            EnsureOfflineOpponent();

            MatchNetworkRuntimeInstaller.EnsureInstalled();
            NetworkMatchStateInstaller.EnsureInstalled();
            NetworkCombatRuntimeInstaller.EnsureInstalled();
            NetworkEliminationInstaller.EnsureInstalled();
            MatchPopulationBootstrap.EnsureInstalled();
            NetworkBotSnapshotPublisherInstaller.EnsureInstalled();

            StarterResultsUiInstaller.EnsureInstalled();
            RuntimeStatusHudInstaller.EnsureInstalled();
            MatchProgressReporter.EnsureInstalled();
            WireExistingHud(localParticipant.gameObject);

            Debug.Log("FSP Match: runtime path ready (manager, safe zone, player, weapon, world, mobile controls, authoritative network combat, bot replication, status HUD, results and progress saving).");
        }

        private static MatchManager EnsureMatchManager()
        {
            MatchManager existing = FindFirstObjectByType<MatchManager>();
            return existing != null ? existing : new GameObject("MatchManager").AddComponent<MatchManager>();
        }

        private static SafeZoneController EnsureSafeZone()
        {
            SafeZoneController existing = FindFirstObjectByType<SafeZoneController>();
            if (existing != null) return existing;

            GameObject zoneObject = new GameObject("RuntimeSafeZone");
            SafeZoneController zone = zoneObject.AddComponent<SafeZoneController>();
            SafeZonePlan plan = ScriptableObject.CreateInstance<SafeZonePlan>();
            zone.ConfigurePlan(plan);
            Debug.Log("FSP Match: runtime safe zone installed.");
            return zone;
        }

        private static MatchParticipant FindLocalPlayer()
        {
            foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
                if (participant != null && participant.IsLocalPlayer) return participant;
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
                renderer.enabled = false;
            }

            MatchParticipant participant = player.AddComponent<MatchParticipant>();
            participant.ConfigureAsLocalPlayer("Player");
            player.AddComponent<StarterProceduralCharacterVisual>();

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
            if (player.GetComponent<StarterParachuteVisual>() == null) player.AddComponent<StarterParachuteVisual>();
            if (player.GetComponent<DropPlanePassenger>() == null) player.AddComponent<DropPlanePassenger>();
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
            if (inventory == null || inventory.PrimaryWeapon != null || inventory.SecondaryWeapon != null) return;

            Camera aimCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            if (aimCamera == null)
            {
                Debug.LogError("FSP Match: starter weapon could not be created because no gameplay camera exists.");
                return;
            }

            WeaponConfig config = ScriptableObject.CreateInstance<WeaponConfig>();
            config.weaponId = "runtime_rifle";
            int loadout = LobbyGameplayProgress.LoadoutIndex;
            config.displayName = loadout == 1 ? "Scout Rifle" : loadout == 2 ? "Heavy Rifle" : "Assault Rifle";
            config.weaponClass = WeaponClass.Assault;
            config.ammoClass = AmmoClass.Medium;
            config.damage = loadout == 1 ? 34f : loadout == 2 ? 31f : 28f;
            config.range = loadout == 1 ? 240f : loadout == 2 ? 165f : 180f;
            config.roundsPerSecond = loadout == 1 ? 6.5f : loadout == 2 ? 7.2f : 9f;
            config.magazineSize = loadout == 2 ? 40 : 30;
            config.reloadSeconds = loadout == 1 ? 1.7f : loadout == 2 ? 2.4f : 2.0f;
            config.spreadDegrees = loadout == 1 ? 0.35f : loadout == 2 ? 1.0f : 0.8f;

            WeaponConfig secondaryConfig = ScriptableObject.CreateInstance<WeaponConfig>();
            secondaryConfig.weaponId = "runtime_smg";
            secondaryConfig.displayName = "Viper SMG";
            secondaryConfig.weaponClass = WeaponClass.SMG;
            secondaryConfig.ammoClass = AmmoClass.Light;
            secondaryConfig.damage = 19f;
            secondaryConfig.range = 95f;
            secondaryConfig.roundsPerSecond = 13f;
            secondaryConfig.magazineSize = 32;
            secondaryConfig.reloadSeconds = 1.55f;
            secondaryConfig.spreadDegrees = 1.25f;

            HitscanWeapon primary = CreateRuntimeWeapon(player.transform, inventory, aimCamera, config,
                "RuntimePrimaryRifle", new Vector3(.42f, 1.25f, .42f), new Vector3(.12f, .12f, .62f));
            HitscanWeapon secondary = CreateRuntimeWeapon(player.transform, inventory, aimCamera, secondaryConfig,
                "RuntimeSecondarySmg", new Vector3(.38f, 1.18f, .34f), new Vector3(.10f, .10f, .46f));
            inventory.ConfigureStarterLoadout(primary, secondary, loadout == 2 ? 120 : 90, 128, loadout == 1 ? 3 : 2);
            Debug.Log("FSP Match: primary rifle, secondary SMG and independent reserve ammo installed.");
        }

        private static HitscanWeapon CreateRuntimeWeapon(Transform owner, PlayerInventory inventory, Camera aimCamera,
            WeaponConfig config, string objectName, Vector3 localPosition, Vector3 localScale)
        {
            GameObject weaponObject = new(objectName);
            weaponObject.transform.SetParent(owner, false);
            weaponObject.transform.localPosition = localPosition;
            weaponObject.transform.localRotation = Quaternion.Euler(0f, 0f, -8f);
            weaponObject.transform.localScale = Vector3.one * Mathf.Clamp(localScale.z / .62f, .78f, 1.05f);
            bool smg = config.weaponClass == WeaponClass.SMG;
            Material metal = RuntimeWeaponMaterial(smg ? new Color(.16f, .20f, .22f, 1f) : new Color(.30f, .35f, .27f, 1f));
            Material dark = RuntimeWeaponMaterial(new Color(.08f, .10f, .11f, 1f));
            Material accent = RuntimeWeaponMaterial(new Color(.92f, .29f, .025f, 1f));
            float length = smg ? .48f : .66f;
            WeaponBox(weaponObject.transform, "Receiver", Vector3.zero, new Vector3(.16f, .17f, length), metal);
            WeaponBox(weaponObject.transform, "Handguard", new Vector3(0f, 0f, length * .52f), new Vector3(.14f, .15f, length * .42f), dark);
            WeaponBox(weaponObject.transform, "Stock", new Vector3(0f, .01f, -length * .64f), new Vector3(.15f, .20f, length * .34f), dark);
            WeaponBox(weaponObject.transform, "Magazine", new Vector3(0f, -.18f, -.02f), new Vector3(.12f, .28f, .16f), dark);
            WeaponBox(weaponObject.transform, "Sight", new Vector3(0f, .15f, .04f), new Vector3(.08f, .08f, .15f), accent);
            GameObject barrel = AndroidSafeMesh.CreateCylinder("Barrel", weaponObject.transform);
            barrel.transform.localPosition = new Vector3(0f, 0f, length * .98f);
            barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            barrel.transform.localScale = new Vector3(.055f, length * .58f, .055f);
            barrel.GetComponent<MeshRenderer>().sharedMaterial = dark;

            GameObject muzzleObject = new GameObject("Muzzle");
            muzzleObject.transform.SetParent(weaponObject.transform, false);
            muzzleObject.transform.localPosition = new Vector3(0f, 0f, length * 1.28f);

            HitscanWeapon weapon = weaponObject.AddComponent<HitscanWeapon>();
            weapon.Configure(config, aimCamera, muzzleObject.transform, inventory);
            return weapon;
        }

        private static void WeaponBox(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = AndroidSafeMesh.CreateBox(name, parent);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static Material RuntimeWeaponMaterial(Color color)
        {
            Shader shader = Resources.Load<Shader>("Shaders/FspMobileSafe");
            if (shader == null) shader = Shader.Find("Fsp/MobileSafeLit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            return new Material(shader) { color = color, hideFlags = HideFlags.DontSave };
        }

        private static void EnsureDropFlow(GameObject player)
        {
            if (player == null) return;
            DropPlaneController plane = FindFirstObjectByType<DropPlaneController>();
            Transform cabin;
            if (plane == null)
            {
                GameObject planeObject = new("SunscarTransportPlane");
                plane = planeObject.AddComponent<DropPlaneController>();
                planeObject.AddComponent<StarterPlaneVisual>();
                cabin = new GameObject("CabinAnchor").transform;
                cabin.SetParent(planeObject.transform, false);
                cabin.localPosition = new Vector3(0f, 0f, -1.5f);

                Transform start = new GameObject("DropRouteStart").transform;
                Transform end = new GameObject("DropRouteEnd").transform;
                start.position = new Vector3(-270f, 145f, -240f);
                // Forced jump at route end must still happen above the playable island,
                // never over the non-collidable ocean.
                end.position = new Vector3(170f, 145f, 170f);
                Vector3 route = end.position - start.position;
                start.rotation = Quaternion.LookRotation(route.normalized, Vector3.up);
                end.rotation = start.rotation;
                plane.ConfigureRoute(start, end, 65f, true);
            }
            else
            {
                cabin = plane.transform.Find("CabinAnchor");
                if (cabin == null)
                {
                    cabin = new GameObject("CabinAnchor").transform;
                    cabin.SetParent(plane.transform, false);
                }
            }

            DropPlanePassenger passenger = player.GetComponent<DropPlanePassenger>();
            passenger?.Configure(plane, cabin);
            if (FindFirstObjectByType<DropPhaseCoordinator>() == null)
                new GameObject("DropPhaseCoordinator").AddComponent<DropPhaseCoordinator>();
        }

        private static void EnsureOfflineOpponent()
        {
            if (MatchRoomState.HasMatch) return;
            MatchParticipant[] participants = FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None);
            foreach (MatchParticipant participant in participants)
                if (participant != null && participant.IsBot) return;

            GameObject spawnerObject = GameObject.Find("RuntimeOfflineBotSpawner") ?? new GameObject("RuntimeOfflineBotSpawner");
            BotSpawner spawner = spawnerObject.GetComponent<BotSpawner>();
            if (spawner == null) spawner = spawnerObject.AddComponent<BotSpawner>();

            GameObject spawnObject = GameObject.Find("RuntimeOfflineBotSpawn") ?? new GameObject("RuntimeOfflineBotSpawn");
            // Never derive this from the passenger while it is parented to the route start:
            // that position is intentionally outside the island. Keep the fallback opponent
            // near the forced-drop side of the playable land so an offline match can finish.
            Vector3 spawn = new Vector3(145f, 1.1f, 142f);
            spawnObject.transform.position = spawn;
            spawner.ConfigureSpawnPoints(new[] { spawnObject.transform });

            if (!spawner.TrySpawnOne())
            {
                Debug.LogError("FSP Match: failed to create the offline opponent fallback.");
                return;
            }
            Debug.Log("FSP Match: offline opponent fallback created inside opening combat range.");
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
