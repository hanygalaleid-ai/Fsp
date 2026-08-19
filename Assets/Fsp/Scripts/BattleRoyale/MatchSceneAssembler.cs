using Fsp.Bots;
using Fsp.Combat;
using Fsp.Core;
using Fsp.Inventory;
using Fsp.Lobby;
using Fsp.Player;
using Fsp.UI;
using Fsp.Vehicles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fsp.BattleRoyale
{
    public sealed class MatchSceneAssembler : MonoBehaviour
    {
        [SerializeField] private bool createFallbackPlayer = true;
        [SerializeField] private bool createFallbackPopulation = true;
        [SerializeField] private bool createFallbackSafeZone = true;
        [SerializeField] private bool createFallbackPlane = true;
        [SerializeField] private bool createFallbackGameplay = true;
        private MatchParticipant localParticipant;

        private void Awake()
        {
            EnsureMatchManager();
            if (createFallbackSafeZone) EnsureSafeZone();
            if (createFallbackPlayer) localParticipant = EnsureLocalPlayer();

#if UNITY_ANDROID && !UNITY_EDITOR
            // Android release safety: prove a stable grounded match first. Procedural bots and the
            // aircraft are intentionally disabled because they were the source of thousands of
            // CreatePrimitive/collider failures on IL2CPP devices.
            if (createFallbackGameplay && localParticipant != null)
            {
                EnsureStarterLoadout(localParticipant.gameObject);
                EnsureHud(localParticipant.gameObject);
            }
#else
            if (createFallbackPlane) EnsurePlane(localParticipant);
            if (createFallbackPopulation) EnsurePopulation();
            if (createFallbackGameplay && localParticipant != null)
            {
                EnsureStarterLoadout(localParticipant.gameObject);
                EnsureLoot();
                EnsureVehicle();
                EnsureHud(localParticipant.gameObject);
            }
#endif
        }

        private static MatchManager EnsureMatchManager()
        {
            MatchManager existing = FindObjectOfType<MatchManager>();
            return existing != null ? existing : new GameObject("MatchManager").AddComponent<MatchManager>();
        }

        private static MatchParticipant EnsureLocalPlayer()
        {
            foreach (MatchParticipant existingParticipant in FindObjectsOfType<MatchParticipant>())
            {
                if (existingParticipant == null || !existingParticipant.IsLocalPlayer) continue;
                if (existingParticipant.GetComponent<PlayerDamageable>() == null) existingParticipant.gameObject.AddComponent<PlayerDamageable>();
                return existingParticipant;
            }

            var player = new GameObject("LocalPlayer_AndroidSafe");
            player.transform.position = new Vector3(0f, 1.05f, 0f);
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f; controller.radius = 0.35f; controller.center = new Vector3(0f, 0.9f, 0f);
            player.AddComponent<PlayerVitals>();
            var participant = player.AddComponent<MatchParticipant>();
            participant.ConfigureAsLocalPlayer(LobbyState.Instance != null ? LobbyState.Instance.DisplayName : "Player");
            player.AddComponent<PlayerDamageable>();
            player.AddComponent<ThirdPersonMotor>();
            player.AddComponent<ParachuteController>();
            player.AddComponent<Fsp.Presentation.StarterProceduralCharacterVisual>();
            return participant;
        }

        private static void EnsurePopulation()
        {
            BotSpawner spawner = FindObjectOfType<BotSpawner>();
            if (spawner == null)
            {
                var go = new GameObject("BotPopulation");
                spawner = go.AddComponent<BotSpawner>();
                go.AddComponent<MatchPopulationBootstrap>();
            }
            else if (spawner.GetComponent<MatchPopulationBootstrap>() == null) spawner.gameObject.AddComponent<MatchPopulationBootstrap>();
        }

        private static void EnsureSafeZone()
        {
            if (FindObjectOfType<SafeZoneController>() != null) return;
            var root = new GameObject("SafeZone");
            var controller = root.AddComponent<SafeZoneController>();
            var visual = new GameObject("SafeZoneVisual_Anchor").transform;
            visual.SetParent(root.transform, false);
            var plan = ScriptableObject.CreateInstance<SafeZonePlan>();
            plan.hideFlags = HideFlags.DontSave;
            controller.ConfigurePlan(plan, visual);
        }

        private static void EnsurePlane(MatchParticipant local)
        {
            DropPlaneController existing = FindObjectOfType<DropPlaneController>();
            if (existing != null) { WirePassenger(local, existing, existing.transform); return; }
            var start = new GameObject("PlaneRouteStart").transform;
            start.position = new Vector3(-1100f, 260f, -900f);
            start.rotation = Quaternion.LookRotation(new Vector3(1f, 0f, 0.8f).normalized, Vector3.up);
            var end = new GameObject("PlaneRouteEnd").transform;
            end.position = new Vector3(1100f, 260f, 900f); end.rotation = start.rotation;
            var plane = AndroidSafeMesh.CreateBox("DropPlane_Placeholder");
            plane.transform.localScale = new Vector3(8f, 1.4f, 6f);
            var cabin = new GameObject("CabinAnchor").transform;
            cabin.SetParent(plane.transform, false); cabin.localPosition = new Vector3(0f, 0.8f, 0f);
            var controller = plane.AddComponent<DropPlaneController>();
            controller.ConfigureRoute(start, end, 72f, true);
            WirePassenger(local, controller, cabin);
        }

        private static void WirePassenger(MatchParticipant local, DropPlaneController plane, Transform cabin)
        {
            if (local == null || plane == null || cabin == null) return;
            var passenger = local.GetComponent<DropPlanePassenger>() ?? local.gameObject.AddComponent<DropPlanePassenger>();
            passenger.Configure(plane, cabin);
        }

        private static void EnsureStarterLoadout(GameObject player)
        {
            if (player == null) return;
            PlayerInventory inventory = player.GetComponent<PlayerInventory>() ?? player.AddComponent<PlayerInventory>();
            if (inventory.PrimaryWeapon != null) return;
            Camera camera = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
            var weaponObject = AndroidSafeMesh.CreateBox("Dune_AR4_Placeholder", player.transform);
            weaponObject.transform.localPosition = new Vector3(0.45f, 1.15f, 0.45f);
            weaponObject.transform.localScale = new Vector3(0.12f, 0.12f, 0.7f);
            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(weaponObject.transform, false); muzzle.localPosition = new Vector3(0f, 0f, 0.55f);
            WeaponConfig config = ScriptableObject.CreateInstance<WeaponConfig>();
            config.hideFlags = HideFlags.DontSave; config.weaponId = "dune_ar4"; config.displayName = "Dune AR-4";
            config.damage = 29f; config.range = 190f; config.roundsPerSecond = 9.2f; config.magazineSize = 30; config.reloadSeconds = 2.05f; config.spreadDegrees = 0.65f;
            HitscanWeapon weapon = weaponObject.AddComponent<HitscanWeapon>();
            weapon.Configure(config, camera, muzzle, inventory);
            inventory.ConfigureStarterLoadout(weapon, null, 120, 0, 2);
            if (player.GetComponent<StarterCombatInput>() == null) player.AddComponent<StarterCombatInput>();
        }

        private static void EnsureLoot()
        {
            if (FindObjectOfType<LootPickup>() != null) return;
            CreateLoot("loot_ammo_01", InventoryItemType.Ammo, new Vector3(5f, 0.5f, 4f), 60, 0f);
            CreateLoot("loot_medkit_01", InventoryItemType.Medkit, new Vector3(-4f, 0.5f, 6f), 0, 0f);
            CreateLoot("loot_armor_01", InventoryItemType.Armor, new Vector3(7f, 0.5f, -5f), 0, 40f);
        }

        private static void CreateLoot(string id, InventoryItemType type, Vector3 position, int ammo, float armor)
        {
            var go = AndroidSafeMesh.CreateBox(id); go.transform.position = position; go.transform.localScale = Vector3.one * 0.55f;
            var item = ScriptableObject.CreateInstance<InventoryItem>(); item.hideFlags = HideFlags.DontSave;
            item.itemId = id; item.displayName = type.ToString(); item.type = type; item.ammoAmount = ammo; item.armorAmount = armor;
            go.AddComponent<LootPickup>().Configure(item, id, true);
        }

        private static void EnsureVehicle()
        {
            if (FindObjectOfType<SimpleVehicleController>() != null) return;
            var car = AndroidSafeMesh.CreateBox("ScoutVehicle_Placeholder");
            car.transform.position = new Vector3(12f, 0.8f, 8f); car.transform.localScale = new Vector3(1.8f, 0.8f, 3.2f);
            Rigidbody body = car.AddComponent<Rigidbody>(); body.mass = 950f; body.centerOfMass = new Vector3(0f, -0.4f, 0f);
            SimpleVehicleController vehicle = car.AddComponent<SimpleVehicleController>();
            Transform seatPoint = new GameObject("DriverSeat").transform; seatPoint.SetParent(car.transform, false); seatPoint.localPosition = new Vector3(0f, 0.9f, 0.2f);
            Transform exitPoint = new GameObject("ExitPoint").transform; exitPoint.SetParent(car.transform, false); exitPoint.localPosition = new Vector3(1.8f, 0.4f, 0f);
            car.AddComponent<VehicleSeat>().Configure(vehicle, seatPoint, exitPoint);
        }

        private static void EnsureHud(GameObject player)
        {
            if (player == null) return;
            BattleRoyaleHud hud = FindObjectOfType<BattleRoyaleHud>();
            if (hud == null) hud = CreateFallbackHud();
            hud.ConfigureSources(player.GetComponent<PlayerVitals>(), player.GetComponent<PlayerInventory>(), FindObjectOfType<MatchManager>(), FindObjectOfType<SafeZoneController>(), player.transform);
        }

        private static BattleRoyaleHud CreateFallbackHud()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem"); eventSystem.AddComponent<EventSystem>(); eventSystem.AddComponent<StandaloneInputModule>();
            }
            var canvasObject = new GameObject("FallbackHUD");
            Canvas canvas = canvasObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920f, 1080f); scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>(); BattleRoyaleHud hud = canvasObject.AddComponent<BattleRoyaleHud>();
            Text alive = CreateText(canvas.transform, "Alive", "1", new Vector2(42f, -28f), TextAnchor.UpperLeft, 20);
            Text ammo = CreateText(canvas.transform, "Ammo", "30 / 120", new Vector2(-42f, 48f), TextAnchor.LowerRight, 24);
            Text medkits = CreateText(canvas.transform, "Medkits", "2", new Vector2(42f, 48f), TextAnchor.LowerLeft, 20);
            Text warning = CreateText(canvas.transform, "ZoneWarning", "", new Vector2(0f, -72f), TextAnchor.UpperCenter, 22);
            hud.ConfigureWidgets(null, null, ammo, alive, medkits, warning, null, null);
            return hud;
        }

        private static Text CreateText(Transform parent, string name, string value, Vector2 anchoredPosition, TextAnchor alignment, int size)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = alignment == TextAnchor.UpperLeft ? new Vector2(0f, 1f) : alignment == TextAnchor.LowerRight ? new Vector2(1f, 0f) : alignment == TextAnchor.LowerLeft ? Vector2.zero : new Vector2(0.5f, 1f);
            rect.anchorMax = rect.anchorMin; rect.pivot = rect.anchorMin; rect.anchoredPosition = anchoredPosition; rect.sizeDelta = new Vector2(360f, 52f);
            Text text = go.AddComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = alignment; text.text = value; text.color = new Color(0.94f, 0.92f, 0.86f, 1f);
            return text;
        }
    }
}
