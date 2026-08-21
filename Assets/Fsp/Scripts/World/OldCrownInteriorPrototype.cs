using Fsp.Inventory;
using UnityEngine;

namespace Fsp.World
{
    /// <summary>
    /// Lightweight procedural interior kit for the Old Crown prototype.
    /// It creates playable rooms, doors, windows, stairs, rooftops, cover and interior loot
    /// entirely from Unity primitives so there are no external asset dependencies.
    /// </summary>
    public sealed class OldCrownInteriorPrototype : MonoBehaviour
    {
        [SerializeField] private int buildingCount = 10;
        [SerializeField] private Vector2 area = new(92f, 76f);
        [SerializeField] private int seed = 7311;

        private Material stone;
        private Material darkStone;
        private Material wood;
        private Material bronze;

        private void Start()
        {
            BuildIfNeeded();
        }

        public void BuildIfNeeded()
        {
            if (transform.Find("GeneratedInteriors") != null) return;
            var root = new GameObject("GeneratedInteriors").transform;
            root.SetParent(transform, false);

            stone = MakeMaterial(Color.white, "World/bmg_fortress_wall_v3", new Vector2(3f, 2f));
            darkStone = MakeMaterial(new Color(.72f, .68f, .62f), "World/bmg_fortress_wall_v3", new Vector2(3f, 2f));
            wood = MakeMaterial(Color.white, "World/bmg_wood_floor_v3", new Vector2(3f, 3f));
            bronze = MakeMaterial(new Color(0.64f, 0.43f, 0.22f), null, Vector2.one);

            var random = new System.Random(seed);
            for (int i = 0; i < Mathf.Max(1, buildingCount); i++)
            {
                float x = Mathf.Lerp(-area.x * 0.5f, area.x * 0.5f, (float)random.NextDouble());
                float z = Mathf.Lerp(-area.y * 0.5f, area.y * 0.5f, (float)random.NextDouble());
                int floors = random.Next(1, 4);
                BuildHouse(root, i, new Vector3(x, 0f, z), floors, random.Next(0, 4) * 90f);
            }
        }

        private void BuildHouse(Transform parent, int index, Vector3 position, int floors, float yaw)
        {
            var house = new GameObject($"OldCrownHouse_{index:00}").transform;
            house.SetParent(parent, false);
            house.localPosition = position;
            house.localRotation = Quaternion.Euler(0f, yaw, 0f);

            const float width = 7.2f;
            const float depth = 6.2f;
            const float floorHeight = 3.1f;
            const float wall = 0.28f;

            for (int floor = 0; floor < floors; floor++)
            {
                float y = floor * floorHeight;
                CreateBlock(house, $"Floor_{floor}", new Vector3(0f, y + 0.08f, 0f), new Vector3(width, 0.16f, depth), darkStone, true);
                CreateWallWithDoor(house, y, width, depth, wall, floor == 0);
                CreateSideWalls(house, y, width, depth, wall);
                AddInteriorCover(house, y, floor);
                AddWindowFrames(house, y, width, depth);

                if (floor < floors - 1)
                    AddStairs(house, y, width, depth);
            }

            float roofY = floors * floorHeight;
            CreateBlock(house, "Roof", new Vector3(0f, roofY, 0f), new Vector3(width, 0.18f, depth), stone, true);
            AddRoofParapets(house, roofY + 0.55f, width, depth);
            AddExteriorRoofAccess(house, roofY, width, depth);

            SpawnInteriorLoot(house, index, floors);
        }

        private void CreateWallWithDoor(Transform house, float y, float width, float depth, float thickness, bool groundFloor)
        {
            float top = y + 1.55f;
            float doorWidth = groundFloor ? 1.35f : 0.95f;
            float segment = (width - doorWidth) * 0.5f;

            CreateBlock(house, "FrontWall_L", new Vector3(-(doorWidth + segment) * 0.5f, top, depth * 0.5f), new Vector3(segment, 3.1f, thickness), stone, true);
            CreateBlock(house, "FrontWall_R", new Vector3((doorWidth + segment) * 0.5f, top, depth * 0.5f), new Vector3(segment, 3.1f, thickness), stone, true);
            CreateBlock(house, "FrontLintel", new Vector3(0f, y + 2.72f, depth * 0.5f), new Vector3(doorWidth, 0.76f, thickness), stone, true);

            CreateBlock(house, "BackWall", new Vector3(0f, top, -depth * 0.5f), new Vector3(width, 3.1f, thickness), stone, true);
        }

        private void CreateSideWalls(Transform house, float y, float width, float depth, float thickness)
        {
            float top = y + 1.55f;
            float gap = 1.45f;
            float segment = (depth - gap) * 0.5f;
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * width * 0.5f;
                CreateBlock(house, "SideWall_A", new Vector3(x, top, -(gap + segment) * 0.5f), new Vector3(thickness, 3.1f, segment), stone, true);
                CreateBlock(house, "SideWall_B", new Vector3(x, top, (gap + segment) * 0.5f), new Vector3(thickness, 3.1f, segment), stone, true);
                CreateBlock(house, "SideLintel", new Vector3(x, y + 2.7f, 0f), new Vector3(thickness, 0.8f, gap), stone, true);
            }
        }

        private void AddWindowFrames(Transform house, float y, float width, float depth)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * (width * 0.5f - 0.02f);
                CreateBlock(house, "WindowSill", new Vector3(x, y + 1.0f, 0f), new Vector3(0.10f, 0.12f, 1.55f), bronze, false);
            }
        }

        private void AddInteriorCover(Transform house, float y, int floor)
        {
            CreateBlock(house, "HalfWall", new Vector3(0f, y + 0.65f, -0.7f), new Vector3(3.4f, 1.3f, 0.22f), darkStone, true);
            if (floor % 2 == 0)
                CreateBlock(house, "TableCover", new Vector3(1.65f, y + 0.55f, 1.2f), new Vector3(1.7f, 1.1f, 0.75f), wood, true);
            else
                CreateBlock(house, "CrateCover", new Vector3(-1.55f, y + 0.62f, 1.25f), new Vector3(1.2f, 1.25f, 1.2f), wood, true);
        }

        private void AddStairs(Transform house, float y, float width, float depth)
        {
            const int steps = 10;
            for (int i = 0; i < steps; i++)
            {
                float t = (i + 0.5f) / steps;
                float stepY = y + t * 3.1f;
                float stepZ = Mathf.Lerp(-depth * 0.28f, depth * 0.27f, t);
                CreateBlock(house, "Stair", new Vector3(-width * 0.31f, stepY, stepZ), new Vector3(2.0f, 0.31f, depth * 0.62f / steps + 0.12f), darkStone, true);
            }
        }

        private void AddRoofParapets(Transform house, float y, float width, float depth)
        {
            CreateBlock(house, "RoofWallN", new Vector3(0f, y, depth * 0.5f), new Vector3(width, 1.0f, 0.22f), stone, true);
            CreateBlock(house, "RoofWallS", new Vector3(0f, y, -depth * 0.5f), new Vector3(width, 1.0f, 0.22f), stone, true);
            CreateBlock(house, "RoofWallE", new Vector3(width * 0.5f, y, 0f), new Vector3(0.22f, 1.0f, depth), stone, true);
            CreateBlock(house, "RoofWallW", new Vector3(-width * 0.5f, y, 0f), new Vector3(0.22f, 1.0f, depth), stone, true);
        }

        private void AddExteriorRoofAccess(Transform house, float roofY, float width, float depth)
        {
            const int steps = 12;
            for (int i = 0; i < steps; i++)
            {
                float t = i / (float)(steps - 1);
                CreateBlock(house, "RoofAccess", new Vector3(width * 0.56f, Mathf.Lerp(0.2f, roofY - 0.15f, t), Mathf.Lerp(-depth * 0.48f, depth * 0.30f, t)), new Vector3(1.0f, 0.28f, 0.62f), darkStone, true);
            }
        }

        private void SpawnInteriorLoot(Transform house, int houseIndex, int floors)
        {
            for (int floor = 0; floor < floors; floor++)
            {
                InventoryItemType type = floor % 3 == 0 ? InventoryItemType.Ammo : floor % 3 == 1 ? InventoryItemType.Medkit : InventoryItemType.Armor;
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"oldcrown_loot_{houseIndex}_{floor}";
                go.transform.SetParent(house, false);
                go.transform.localPosition = new Vector3(1.8f - floor * 0.35f, floor * 3.1f + 0.35f, -1.65f);
                go.transform.localScale = Vector3.one * 0.38f;

                var item = ScriptableObject.CreateInstance<InventoryItem>();
                item.hideFlags = HideFlags.DontSave;
                item.itemId = go.name;
                item.displayName = type.ToString();
                item.type = type;
                item.ammoAmount = type == InventoryItemType.Ammo ? 45 : 0;
                item.armorAmount = type == InventoryItemType.Armor ? 30f : 0f;

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

        private static Material MakeMaterial(Color color, string texturePath, Vector2 scale)
        {
            Shader shader = Resources.Load<Shader>("Shaders/FspMobileSafe");
            if (shader == null) shader = Shader.Find("Fsp/MobileSafeLit");
            Material mat = new Material(shader != null ? shader : Shader.Find("Sprites/Default"));
            mat.color = color;
            if (!string.IsNullOrEmpty(texturePath))
            {
                Texture2D texture = Resources.Load<Texture2D>(texturePath);
                if (texture != null)
                {
                    texture.wrapMode = TextureWrapMode.Repeat;
                    texture.filterMode = FilterMode.Bilinear;
                    texture.anisoLevel = 2;
                    mat.mainTexture = texture;
                    mat.mainTextureScale = scale;
                }
            }
            return mat;
        }
    }
}
