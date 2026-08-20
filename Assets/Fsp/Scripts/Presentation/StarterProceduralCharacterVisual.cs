using Fsp.BattleRoyale;
using Fsp.Core;
using Fsp.Player;
using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>Original low-poly military character used when no authored character prefab is present.</summary>
    public sealed class StarterProceduralCharacterVisual : MonoBehaviour
    {
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftLeg;
        private Transform rightLeg;
        private DropPlanePassenger planePassenger;
        private ParachuteController parachute;
        private Vector3 previousPosition;
        private float stride;
        private Transform visualRoot;

        private void Awake() => Build();

        private void Update()
        {
            Vector3 movement = transform.position - previousPosition;
            previousPosition = transform.position;
            if (planePassenger == null) planePassenger = GetComponent<DropPlanePassenger>();
            if (parachute == null) parachute = GetComponent<ParachuteController>();
            if (visualRoot != null) visualRoot.gameObject.SetActive(planePassenger == null || !planePassenger.IsAboard);
            bool locomotionEnabled = (planePassenger == null || !planePassenger.IsAboard) &&
                                     (parachute == null || !parachute.IsActive);
            float speed = Time.deltaTime > 0f ? Vector3.ProjectOnPlane(movement, Vector3.up).magnitude / Time.deltaTime : 0f;
            if (!locomotionEnabled) speed = 0f;
            stride += Time.deltaTime * Mathf.Clamp(speed * 4.5f, 0f, 11f);
            float swing = speed > 0.08f ? Mathf.Sin(stride) * Mathf.Clamp(speed * 5f, 8f, 32f) : 0f;
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(swing, 0f, 0f);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(-swing - 18f, 0f, 0f);
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(-swing, 0f, 0f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(swing, 0f, 0f);
        }

        public void Build()
        {
            if (transform.Find("FSP_CharacterVisual") != null) return;
            previousPosition = transform.position;

            MatchParticipant participant = GetComponent<MatchParticipant>();
            bool bot = participant != null && participant.IsBot;
            Color cloth = bot ? new Color(0.52f, 0.24f, 0.14f) : new Color(0.23f, 0.42f, 0.27f);
            Material uniform = MaterialFor(cloth);
            Material armor = MaterialFor(bot ? new Color(0.30f, 0.11f, 0.07f) : new Color(0.13f, 0.18f, 0.14f));
            Material skin = MaterialFor(new Color(0.58f, 0.39f, 0.28f));
            Material metal = MaterialFor(new Color(0.12f, 0.14f, 0.15f));
            Material accent = MaterialFor(bot ? new Color(0.78f, 0.12f, 0.05f) : new Color(1f, 0.36f, 0.015f));

            Transform root = new GameObject("FSP_CharacterVisual").transform;
            root.SetParent(transform, false);
            visualRoot = root;
            root.localScale = Vector3.one * 1.06f;

            Part(root, "Torso", new Vector3(0f, 1.18f, 0f), new Vector3(.62f, .72f, .34f), uniform);
            Part(root, "Vest", new Vector3(0f, 1.22f, .20f), new Vector3(.68f, .55f, .13f), armor);
            Part(root, "Belt", new Vector3(0f, .82f, 0f), new Vector3(.66f, .12f, .35f), metal);
            Part(root, "Neck", new Vector3(0f, 1.62f, 0f), new Vector3(.19f, .16f, .18f), skin);
            Part(root, "Head", new Vector3(0f, 1.84f, 0f), new Vector3(.38f, .38f, .36f), skin);
            Part(root, "Helmet", new Vector3(0f, 2.04f, -.01f), new Vector3(.46f, .20f, .42f), armor);
            Part(root, "HelmetVisor", new Vector3(0f, 1.94f, .22f), new Vector3(.30f, .07f, .08f), metal);
            Part(root, "FaceMask", new Vector3(0f, 1.79f, .195f), new Vector3(.31f, .14f, .06f), armor);
            Part(root, "Backpack", new Vector3(0f, 1.22f, -.27f), new Vector3(.50f, .58f, .22f), armor);
            Part(root, "TeamMark", new Vector3(0f, 1.35f, .28f), new Vector3(.18f, .08f, .03f), accent);
            Part(root, "ChestPouchL", new Vector3(-.19f, 1.12f, .285f), new Vector3(.18f, .23f, .08f), armor);
            Part(root, "ChestPouchR", new Vector3(.19f, 1.12f, .285f), new Vector3(.18f, .23f, .08f), armor);

            leftArm = Limb(root, "LeftArm", new Vector3(-.42f, 1.46f, 0f), new Vector3(.18f, .66f, .18f), uniform);
            rightArm = Limb(root, "RightArm", new Vector3(.42f, 1.46f, 0f), new Vector3(.18f, .66f, .18f), uniform);
            leftLeg = Limb(root, "LeftLeg", new Vector3(-.18f, .75f, 0f), new Vector3(.24f, .82f, .27f), uniform);
            rightLeg = Limb(root, "RightLeg", new Vector3(.18f, .75f, 0f), new Vector3(.24f, .82f, .27f), uniform);
            Part(leftLeg, "LeftBoot", new Vector3(0f, -.45f, .08f), new Vector3(.28f, .18f, .42f), metal);
            Part(rightLeg, "RightBoot", new Vector3(0f, -.45f, .08f), new Vector3(.28f, .18f, .42f), metal);
            Part(leftLeg, "LeftKnee", new Vector3(0f, -.14f, .16f), new Vector3(.26f, .18f, .09f), armor);
            Part(rightLeg, "RightKnee", new Vector3(0f, -.14f, .16f), new Vector3(.26f, .18f, .09f), armor);

            Transform rifle = new GameObject("RifleVisual").transform;
            rifle.SetParent(rightArm, false);
            rifle.localPosition = new Vector3(-.18f, -.18f, .34f);
            rifle.localRotation = Quaternion.Euler(72f, 0f, 0f);
            Part(rifle, "Receiver", Vector3.zero, new Vector3(.16f, .16f, .62f), metal);
            Part(rifle, "Barrel", new Vector3(0f, 0f, .46f), new Vector3(.07f, .07f, .45f), metal);
            Part(rifle, "Magazine", new Vector3(0f, -.12f, -.05f), new Vector3(.12f, .25f, .16f), armor);

            if (!bot)
            {
                ApplyCharacterIdentity(Fsp.Lobby.LobbyState.Instance != null ? Fsp.Lobby.LobbyState.Instance.SelectedCharacterId : "soldier_01");
                ApplyCosmeticLoadout(StarterWardrobeRuntime.LoadLocal());
            }
        }

        public void ApplyCharacterIdentity(string characterId)
        {
            Color skin;
            Color mark;
            switch (characterId)
            {
                case "soldier_02":
                    skin = new Color(.30f, .18f, .12f);
                    mark = new Color(.12f, .55f, .85f);
                    break;
                case "soldier_03":
                    skin = new Color(.62f, .43f, .29f);
                    mark = new Color(.78f, .72f, .18f);
                    break;
                default:
                    skin = new Color(.47f, .31f, .22f);
                    mark = new Color(1f, .36f, .015f);
                    break;
            }
            SetPartColor("Head", skin);
            SetPartColor("Neck", skin);
            SetPartColor("TeamMark", mark);
        }

        public void ApplyCosmeticLoadout(CosmeticLoadout loadout)
        {
            if (loadout == null) return;
            SetPartColor("Helmet", StarterCosmeticCatalog.Find(loadout.headItemId, CosmeticSlot.Head).Color);
            SetPartColor("HelmetVisor", StarterCosmeticCatalog.Find(loadout.faceItemId, CosmeticSlot.Face).Color);

            Color torso = StarterCosmeticCatalog.Find(loadout.torsoItemId, CosmeticSlot.Torso).Color;
            SetPartColor("Torso", torso);
            SetPartColor("LeftArm", torso);
            SetPartColor("RightArm", torso);

            Color legs = StarterCosmeticCatalog.Find(loadout.legsItemId, CosmeticSlot.Legs).Color;
            SetPartColor("LeftLeg", legs);
            SetPartColor("RightLeg", legs);
            SetPartColor("Backpack", StarterCosmeticCatalog.Find(loadout.backpackItemId, CosmeticSlot.Backpack).Color);
        }

        private void SetPartColor(string partName, Color color)
        {
            Transform root = transform.Find("FSP_CharacterVisual");
            if (root == null) return;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null || !string.Equals(renderer.gameObject.name, partName, System.StringComparison.Ordinal)) continue;
                renderer.sharedMaterial = MaterialFor(color);
            }
        }

        private static Transform Limb(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            Transform pivot = new GameObject(name + "Pivot").transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = position;
            Part(pivot, name, new Vector3(0f, -scale.y * .42f, 0f), scale, material);
            return pivot;
        }

        private static GameObject Part(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = AndroidSafeMesh.CreateBox(name, parent);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            return go;
        }

        private static Material MaterialFor(Color color)
        {
            Shader shader = Resources.Load<Shader>("Shaders/FspMobileSafe");
            if (shader == null) shader = Shader.Find("Fsp/MobileSafeLit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            Material material = new(shader) { color = color, hideFlags = HideFlags.DontSave };
            return material;
        }
    }
}
