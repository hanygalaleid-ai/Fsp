using Fsp.Bots;
using Fsp.Combat;
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
            if (createFallbackPlane) EnsurePlane(localParticipant);
            if (createFallbackPopulation) EnsurePopulation();
            if (createFallbackGameplay && localParticipant != null)
            {
                EnsureStarterLoadout(localParticipant.gameObject);
                EnsureLoot();
                EnsureVehicle();
                EnsureHud(localParticipant.gameObject);
            }
        }

        private static MatchManager EnsureMatchManager()
        {
            MatchManager existing = FindObjectOfType<MatchManager>();
            return existing != null ? existing : new GameObject("MatchManager").AddComponent<MatchManager>();
        }

        private static MatchParticipant EnsureLocalPlayer()
        {
            MatchParticipant[] participants = FindObjectsOfType<MatchParticipant>();
            foreach (MatchParticipant existingParticipant in participants)
                if (existingParticipant != null && existingParticipant.IsLocalPlayer) return existingParticipant;

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "LocalPlayer_Placeholder";
            player.transform.position = new Vector3(0f, 1.05f, 0f);

            Collider primitiveCollider = player.GetComponent<Collider>();
            if (primitiveCollider != null) Destroy(primitiveCollider);

            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            player.AddComponent<PlayerVitals>();
            var participant = player.AddComponent<MatchParticipant>();
            string displayName = LobbyState.Instance != null ? LobbyState.Instance.DisplayName : "Player";
            participant.ConfigureAsLocalPlayer(displayName);
            player.AddComponent<ThirdPersonMotor>();
            player.AddComponent<ParachuteController>();
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
            else if (spawner.GetComponent<MatchPopulationBootstrap>() == null)
            {
                spawner.gameObject.AddComponent<MatchPopulationBootstrap>();
            }
        }

        private static void EnsureSafeZone()
        {
            if (FindObjectOfType<SafeZoneController>() != null) return;

            var root = new GameObject("SafeZone");
            root.transform.position = Vector3.zero;
            var controller = root.AddComponent<SafeZoneController>();

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "SafeZoneVisual_Placeholder";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            visual.transform.localScale = new Vector3(1f, 0.01f, 1f);
            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null) Destroy(visualCollider);

            var plan = ScriptableObject.CreateInstance<SafeZonePlan>();
            plan.hideFlags = HideFlags.DontSave;
            controller.ConfigurePlan(plan, visual.transform);
        }

        private static void EnsurePlane(MatchParticipant local)
        {
            DropPlaneController existing = FindObjectOfType<DropPlaneController>();
            if (existing != null)
            {
                WirePassenger(local, existing, existing.transform);
                return;
            }

            var start = new GameObject("PlaneRouteStart").transform;
            start.position = new Vector3(-1100f, 260f, -900f);
            start.rotation = Quaternion.LookRotation(new Vector3(1f, 0f, 0.8f).normalized, Vector3.up);

            var end = new GameObject("PlaneRouteEnd").transform;
            end.position = new Vector3(1100f, 260f, 900f);
            end.rotation = start.rotation;

            var plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plane.name = "DropPlane_Placeholder";
            plane.transform.localScale = new Vector3(8f, 1.4f, 6f);
            Collider planeCollider = plane.GetComponent<Collider>();
            if (planeCollider != null) Destroy(planeCollider);

            var cabin = new GameObject("CabinAnchor").transform;
            cabin.SetParent(plane.transform, false);
            cabin.localPosition = new Vector3(0f, 0.8f, 0f);

            var controller = plane.AddComponent<DropPlaneController>();
            controller.ConfigureRoute(start, end, 72f, true);
            WirePassenger(local, controller, cabin);
        }

        private static void WirePassenger(MatchParticipant local, DropPlaneController plane, Transform cabin)
        {
            if (local == null || plane == null || cabin == null) return;
            var passenger = local.GetComponent<DropPlanePassenger>();
            if (passenger == null) passenger = local.gameObject.AddComponent<DropPlanePassenger>();
            passenger.Configure(plane, cabin);
        }

        private static void EnsureStarterLoadout(GameObject player)
        {
            if (player == null) return;
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null) inventory = player.AddComponent<PlayerInventory>();
            if (inventory.PrimaryWeapon != null) return;

            Camera camera = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
            var weaponObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            weaponObject.name = "Dune_AR4_Placeholder";
            weaponObject.transform.SetParent(player.transform, false);
            weaponObject.transform.localPosition = new Vector3(0.45f, 1.15f, 0.45f);
            weaponObject.transform.localScale = new Vector3(0.12f, 0.12f, 0.7f);
            Collider weaponCollider = weaponObject.GetComponent<Collider>();
            if (weaponCollider != null) Destroy(weaponCollider);

            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(weaponObject.transform, false);
            muzzle.localPosition = new Vector3(0f, 0f, 0.55f);

            WeaponConfig config = ScriptableObject.CreateInstance<WeaponConfig>();
            config.hideFlags = HideFlags.DontSave;
            config.weaponId = "dune_ar4";
            config.displayName = "Dune AR-4";
            config.damage = 29f;
            config.range = 190f;
            config.roundsPerSecond = 9.2f;
            config.magazineSize = 30;
            config.reloadSeconds = 2.05f;
            config.spreadDegrees = 0.65f;

            HitscanWeapon weapon = weaponObject.AddComponent<HitscanWeapon>();
            weapon.Configure(config, camera, muzzle, inventory);
            inventory.ConfigureStarterLoadout(weapon, null, 120, 0, 2);

            if (player.GetComponent<StarterCombatInput>() == null)
                player.AddComponent<StarterCombatInput>();
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
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = id;
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.55f;

            var item = ScriptableObject.CreateInstance<InventoryItem>();
            item.hideFlags = HideFlags.DontSave;
            item.itemId = id;
            item.displayName = type.ToString();
            item.type = type;
            item.ammoAmount = ammo;
            item.armorAmount = armor;

            LootPickup pickup = go.AddComponent<LootPickup>();
            pickup.Configure(item, id, true);
        }

        private static void EnsureVehicle()
        {
            if (FindObjectOfType<SimpleVehicleController>() != null) return;
            var car = GameObject.CreatePrimitive(PrimitiveType.Cube);
            car.name = "ScoutVehicle_Placeholder";
            car.transform.position = new Vector3(12f, 0.8f, 8f);
            car.transform.localScale = new Vector3(1.8f, 0.8f, 3.2f);
            Rigidbody body = car.AddComponent<Rigidbody>();
            body.mass = 950f;
            body.centerOfMass = new Vector3(0f, -0.4f, 0f);
            car.AddComponent<SimpleVehicleController>();
        }

        private static void EnsureHud(GameObject player)
        {
            if (player == null) return;
            BattleRoyaleHud hud = FindObjectOfType<BattleRoyaleHud>();
            if (hud == null) hud = CreateFallbackHud();

            hud.ConfigureSources(
                player.GetComponent<PlayerVitals>(),
                player.GetComponent<PlayerInventory>(),
                FindObjectOfType<MatchManager>(),
                FindObjectOfType<SafeZoneController>(),
                player.transform);
        }

        private static BattleRoyaleHud CreateFallbackHud()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            var canvasObject = new GameObject("FallbackHUD");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();
            BattleRoyaleHud hud = canvasObject.AddComponent<BattleRoyaleHud>();

            Text alive = CreateText(canvas.transform, "Alive", "ALIVE 32", new Vector2(18f, -18f), TextAnchor.UpperLeft, 20);
            Text ammo = CreateText(canvas.transform, "Ammo", "30 / 120", new Vector2(-18f, 70f), TextAnchor.LowerRight, 24);
            Text medkits = CreateText(canvas.transform, "Medkits", "MED 2", new Vector2(18f, 70f), TextAnchor.LowerLeft, 20);
            Text warning = CreateText(canvas.transform, "ZoneWarning", "ارجع إلى المنطقة الآمنة", new Vector2(0f, -55f), TextAnchor.UpperCenter, 22);

            Button reload = CreateButton(canvas.transform, "Reload", "RELOAD", new Vector2(-130f, 18f));
            Button heal = CreateButton(canvas.transform, "Heal", "HEAL", new Vector2(18f, 18f));
            hud.ConfigureWidgets(null, null, ammo, alive, medkits, warning, reload, heal);
            return hud;
        }

        private static Text CreateText(Transform parent, string name, string value, Vector2 anchoredPosition, TextAnchor alignment, int size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = alignment == TextAnchor.UpperLeft ? new Vector2(0f, 1f) : alignment == TextAnchor.LowerRight ? new Vector2(1f, 0f) : alignment == TextAnchor.LowerLeft ? Vector2.zero : new Vector2(0.5f, 1f);
            rect.anchorMax = rect.anchorMin;
            rect.pivot = alignment == TextAnchor.UpperLeft ? new Vector2(0f, 1f) : alignment == TextAnchor.LowerRight ? new Vector2(1f, 0f) : alignment == TextAnchor.LowerLeft ? Vector2.zero : new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(320f, 46f);
            Text text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.text = value;
            text.color = new Color(0.94f, 0.92f, 0.86f, 1f);
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(105f, 52f);
            Image image = go.AddComponent<Image>();
            image.color = new Color(0.06f, 0.11f, 0.16f, 0.88f);
            Button button = go.AddComponent<Button>();

            Text text = CreateText(go.transform, "Label", label, Vector2.zero, TextAnchor.MiddleCenter, 16);
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            text.alignment = TextAnchor.MiddleCenter;
            return button;
        }
    }
}
