using Fsp.Inventory;
using UnityEngine;

namespace Fsp.World
{
    /// <summary>
    /// Lightweight Copper Port prototype: warehouses, container lanes, dock cover and loot.
    /// Built from primitives for fast mobile iteration and zero external asset dependencies.
    /// </summary>
    public sealed class CopperPortPrototype : MonoBehaviour
    {
        [SerializeField] private Vector3 origin = new(155f, 0f, -120f);
        [SerializeField] private int warehouseCount = 6;
        [SerializeField] private int containerRows = 5;

        private Material concrete;
        private Material navy;
        private Material rust;
        private Material sand;

        private void Start()
        {
            BuildIfNeeded();
        }

        public void BuildIfNeeded()
        {
            if (transform.Find("GeneratedCopperPort") != null) return;
            var root = new GameObject("GeneratedCopperPort").transform;
            root.SetParent(transform, false);
            root.localPosition = origin;

            concrete = MakeMaterial(new Color(0.42f, 0.43f, 0.42f));
            navy = MakeMaterial(new Color(0.07f, 0.12f, 0.18f));
            rust = MakeMaterial(new Color(0.43f, 0.20f, 0.10f));
            sand = MakeMaterial(new Color(0.64f, 0.56f, 0.42f));

            CreateBlock(root, "DockBase", new Vector3(0f, -0.25f, 0f), new Vector3(120f, 0.5f, 82f), concrete, true);
            BuildWarehouses(root);
            BuildContainers(root);
            BuildCraneLandmark(root);
            BuildDockEdges(root);
            SpawnPortLoot(root);
        }

        private void BuildWarehouses(Transform root)
        {
            for (int i = 0; i < warehouseCount; i++)
            {
                float x = -45f + (i % 3) * 34f;
                float z = 18f + (i / 3) * 28f;
                var warehouse = new GameObject($"Warehouse_{i:00}").transform;
                warehouse.SetParent(root, false);
                warehouse.localPosition = new Vector3(x, 0f, z);
                BuildWarehouse(warehouse, i);
            }
        }

        private void BuildWarehouse(Transform w, int index)
        {
            float width = 24f;
            float depth = 14f;
            float height = 5.5f;
            CreateBlock(w, "Floor", new Vector3(0f, 0.08f, 0f), new Vector3(width, 0.16f, depth), concrete, true);
            CreateBlock(w, "Back", new Vector3(0f, height * 0.5f, -depth * 0.5f), new Vector3(width, height, 0.25f), navy, true);
            CreateBlock(w, "SideL", new Vector3(-width * 0.5f, height * 0.5f, 0f), new Vector3(0.25f, height, depth), navy, true);
            CreateBlock(w, "SideR", new Vector3(width * 0.5f, height * 0.5f, 0f), new Vector3(0.25f, height, depth), navy, true);
            CreateBlock(w, "Roof", new Vector3(0f, height, 0f), new Vector3(width, 0.22f, depth), sand, true);

            float doorGap = 5.5f;
            float side = (width - doorGap) * 0.5f;
            CreateBlock(w, "FrontL", new Vector3(-(doorGap + side) * 0.5f, height * 0.5f, depth * 0.5f), new Vector3(side, height, 0.25f), navy, true);
            CreateBlock(w, "FrontR", new Vector3((doorGap + side) * 0.5f, height * 0.5f, depth * 0.5f), new Vector3(side, height, 0.25f), navy, true);
            CreateBlock(w, "FrontTop", new Vector3(0f, height - 0.65f, depth * 0.5f), new Vector3(doorGap, 1.3f, 0.25f), rust, true);

            for (int c = 0; c < 5; c++)
            {
                float cx = -7.5f + c * 3.75f;
                float cz = c % 2 == 0 ? -2.5f : 2.1f;
                CreateBlock(w, "InteriorCrate", new Vector3(cx, 0.8f, cz), new Vector3(2.0f, 1.6f, 2.0f), c % 2 == 0 ? rust : sand, true);
            }

            AddCatwalk(w, width, depth, height, index);
        }

        private void AddCatwalk(Transform w, float width, float depth, float height, int index)
        {
            float y = 3.0f;
            CreateBlock(w, "Catwalk", new Vector3(0f, y, -depth * 0.32f), new Vector3(width * 0.72f, 0.16f, 1.2f), concrete, true);
            for (int i = 0; i < 9; i++)
            {
                float t = i / 8f;
                CreateBlock(w, "CatwalkStep", new Vector3(-width * 0.38f, Mathf.Lerp(0.2f, y - 0.1f, t), Mathf.Lerp(depth * 0.25f, -depth * 0.27f, t)), new Vector3(1.2f, 0.28f, 0.78f), concrete, true);
            }
        }

        private void BuildContainers(Transform root)
        {
            for (int row = 0; row < containerRows; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    if ((row + col) % 5 == 0) continue;
                    float x = -48f + col * 15.5f;
                    float z = -31f + row * 8.5f;
                    float y = 1.25f;
                    int stack = ((row * 7 + col) % 4 == 0) ? 2 : 1;
                    for (int s = 0; s < stack; s++)
                    {
                        CreateBlock(root, $"Container_{row}_{col}_{s}", new Vector3(x, y + s * 2.55f, z), new Vector3(6.2f, 2.5f, 2.5f), ((row + col) % 2 == 0) ? rust : navy, true);
                    }
                }
            }
        }

        private void BuildCraneLandmark(Transform root)
        {
            var crane = new GameObject("HarborCrane").transform;
            crane.SetParent(root, false);
            crane.localPosition = new Vector3(43f, 0f, -28f);
            CreateBlock(crane, "LegA", new Vector3(-4.5f, 8f, 0f), new Vector3(0.7f, 16f, 0.7f), rust, true);
            CreateBlock(crane, "LegB", new Vector3(4.5f, 8f, 0f), new Vector3(0.7f, 16f, 0.7f), rust, true);
            CreateBlock(crane, "TopBeam", new Vector3(0f, 15.7f, 0f), new Vector3(12f, 0.7f, 0.7f), rust, true);
            CreateBlock(crane, "Boom", new Vector3(-1f, 15.2f, -8f), new Vector3(0.65f, 0.65f, 18f), sand, true);
        }

        private void BuildDockEdges(Transform root)
        {
            for (int i = 0; i < 16; i++)
            {
                float x = -55f + i * 7.3f;
                CreateBlock(root, "DockBollard", new Vector3(x, 0.55f, -39f), new Vector3(0.7f, 1.1f, 0.7f), rust, true);
            }
        }

        private void SpawnPortLoot(Transform root)
        {
            for (int i = 0; i < 18; i++)
            {
                InventoryItemType type = i % 4 == 0 ? InventoryItemType.Medkit : i % 4 == 1 ? InventoryItemType.Armor : InventoryItemType.Ammo;
                float x = -48f + (i % 6) * 18f;
                float z = -24f + (i / 6) * 25f;
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"copperport_loot_{i:00}";
                go.transform.SetParent(root, false);
                go.transform.localPosition = new Vector3(x, 0.32f, z);
                go.transform.localScale = Vector3.one * 0.42f;

                var item = ScriptableObject.CreateInstance<InventoryItem>();
                item.hideFlags = HideFlags.DontSave;
                item.itemId = go.name;
                item.displayName = type.ToString();
                item.type = type;
                item.ammoAmount = type == InventoryItemType.Ammo ? 50 : 0;
                item.armorAmount = type == InventoryItemType.Armor ? 35f : 0f;

                LootPickup pickup = go.AddComponent<LootPickup>();
                pickup.Configure(item, go.name, true);
            }
        }

        private static GameObject CreateBlock(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            if (!collider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) Destroy(c);
            }
            return go;
        }

        private static Material MakeMaterial(Color color)
        {
            Shader shader = Shader.Find("Standard");
            Material mat = new Material(shader != null ? shader : Shader.Find("Sprites/Default"));
            mat.color = color;
            return mat;
        }
    }
}
