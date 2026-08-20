using Fsp.Core;
using UnityEngine;

namespace Fsp.World
{
    /// <summary>
    /// Checked-in runtime construction for the three POIs that previously had empty compatibility
    /// components while Match.unity contained no authored geometry. Uses the bundled mobile shader
    /// and textures, deterministic layouts, and real collision surfaces.
    /// </summary>
    public static class SunscarMissingPoiRuntime
    {
        public static void BuildCopperPort(Transform owner)
        {
            Transform root = Root(owner, "GeneratedCopperPort", new Vector3(62f, 0f, -45f));
            if (root == null) return;
            Material ground = Mat("World/road_dust_v2", new Color(.48f, .38f, .25f), new Vector2(8f, 6f));
            Material wall = Mat("World/fortress_wall_v2", new Color(.54f, .42f, .28f), new Vector2(2f, 2f));
            Material metal = Mat(null, new Color(.22f, .28f, .30f), Vector2.one);
            Material orange = Mat(null, new Color(.86f, .31f, .05f), Vector2.one);
            Material water = Mat(null, new Color(.10f, .34f, .43f), Vector2.one);

            Box(root, "PortGround", new Vector3(0f, -.15f, 0f), new Vector3(96f, .3f, 72f), ground, true);
            Box(root, "HarbourWater", new Vector3(0f, -.45f, -47f), new Vector3(112f, .25f, 28f), water, false);
            for (int i = -2; i <= 2; i++)
            {
                Transform warehouse = Group(root, "Warehouse", new Vector3(i * 17f, 0f, 13f));
                Box(warehouse, "WarehouseBody", new Vector3(0f, 3.2f, 0f), new Vector3(14f, 6.4f, 11f), wall, true);
                Box(warehouse, "WarehouseRoof", new Vector3(0f, 6.65f, 0f), new Vector3(14.8f, .5f, 11.8f), metal, true);
                Box(warehouse, "CargoDoor", new Vector3(0f, 2.1f, -5.58f), new Vector3(5.2f, 4.2f, .12f), metal, false);
            }
            for (int i = 0; i < 22; i++)
                Box(root, "CargoContainer", new Vector3(-42f + (i * 13) % 84, 1.25f + (i % 3) * 2.5f, -18f + (i * 17) % 30), new Vector3(6f, 2.4f, 2.5f), i % 4 == 0 ? orange : metal, true);
            for (int i = -2; i <= 2; i++)
            {
                float x = i * 18f;
                Box(root, "Pier", new Vector3(x, .05f, -42f), new Vector3(9f, .45f, 28f), metal, true);
                Box(root, "CranePost", new Vector3(x, 6f, -31f), new Vector3(.8f, 12f, .8f), orange, true);
                Box(root, "CraneArm", new Vector3(x + 4f, 11.5f, -31f), new Vector3(9f, .55f, .55f), orange, false);
            }
        }

        public static void BuildDryfield(Transform owner)
        {
            Transform root = Root(owner, "GeneratedDryfield", new Vector3(82f, 0f, 78f));
            if (root == null) return;
            Material soil = Mat("World/sand_ground_v2", new Color(.50f, .38f, .20f), new Vector2(10f, 10f));
            Material road = Mat("World/road_dust_v2", Color.white, new Vector2(2f, 8f));
            Material wall = Mat("World/fortress_wall_v2", new Color(.65f, .52f, .34f), new Vector2(2f, 2f));
            Material crop = Mat(null, new Color(.35f, .45f, .17f), Vector2.one);
            Material metal = Mat(null, new Color(.25f, .28f, .28f), Vector2.one);

            Box(root, "FarmGround", new Vector3(0f, -.16f, 0f), new Vector3(105f, .32f, 86f), soil, true);
            Box(root, "FarmRoad", new Vector3(0f, .03f, 0f), new Vector3(10f, .14f, 84f), road, true);
            for (int field = -2; field <= 2; field++)
                for (int row = 0; row < 8; row++)
                    Box(root, "CropRow", new Vector3(field * 17f, .32f, -32f + row * 8.5f), new Vector3(11f, .64f, 1.2f), crop, false);

            Transform barn = Group(root, "Barn", new Vector3(29f, 0f, 20f));
            Box(barn, "BarnBody", new Vector3(0f, 3.2f, 0f), new Vector3(18f, 6.4f, 13f), wall, true);
            Box(barn, "BarnRoof", new Vector3(0f, 6.7f, 0f), new Vector3(19f, .7f, 14f), metal, true);
            Box(barn, "BarnDoor", new Vector3(0f, 2.2f, -6.58f), new Vector3(5.5f, 4.4f, .14f), metal, false);

            Transform tower = Group(root, "WaterTower", new Vector3(-30f, 0f, 24f));
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Box(tower, "TowerLeg", new Vector3(x * 2.2f, 5f, z * 2.2f), new Vector3(.45f, 10f, .45f), metal, true);
            Box(tower, "WaterTank", new Vector3(0f, 10f, 0f), new Vector3(7f, 4.2f, 7f), wall, true);
            Box(tower, "TankCap", new Vector3(0f, 12.3f, 0f), new Vector3(7.5f, .45f, 7.5f), metal, false);
        }

        public static void BuildLanternCoast(Transform owner)
        {
            Transform root = Root(owner, "GeneratedLanternCoast", new Vector3(-132f, 0f, 86f));
            if (root == null) return;
            Material sand = Mat("World/sand_ground_v2", new Color(.60f, .48f, .29f), new Vector2(8f, 8f));
            Material stone = Mat("World/rock_cliff_v2", Color.white, new Vector2(3f, 3f));
            Material wall = Mat("World/fortress_wall_v2", new Color(.64f, .53f, .38f), new Vector2(2f, 2f));
            Material dark = Mat(null, new Color(.18f, .22f, .23f), Vector2.one);
            Material orange = Mat(null, new Color(1f, .38f, .04f), Vector2.one);
            Material water = Mat(null, new Color(.09f, .34f, .44f), Vector2.one);

            Box(root, "CoastGround", new Vector3(0f, -.25f, 0f), new Vector3(82f, .5f, 76f), sand, true);
            Box(root, "CoastWater", new Vector3(-50f, -.55f, 0f), new Vector3(30f, .22f, 92f), water, false);
            for (int i = -2; i <= 2; i++)
                Box(root, "FishingPier", new Vector3(-39f, .08f, i * 13f), new Vector3(25f, .5f, 5.5f), dark, true);

            Transform lighthouse = Group(root, "Lighthouse", new Vector3(23f, 0f, 16f));
            Box(lighthouse, "LighthouseBase", new Vector3(0f, 5f, 0f), new Vector3(7f, 10f, 7f), wall, true);
            Box(lighthouse, "LighthouseUpper", new Vector3(0f, 11f, 0f), new Vector3(5.3f, 4f, 5.3f), wall, true);
            Box(lighthouse, "LanternRoom", new Vector3(0f, 13.6f, 0f), new Vector3(5.8f, 1.2f, 5.8f), orange, false);
            Box(lighthouse, "LighthouseRoof", new Vector3(0f, 14.5f, 0f), new Vector3(6.6f, .45f, 6.6f), dark, true);

            for (int i = 0; i < 6; i++)
            {
                Transform hut = Group(root, "CoastHut", new Vector3(-10f + (i % 3) * 15f, 0f, -25f + (i / 3) * 18f));
                Box(hut, "HutBody", new Vector3(0f, 2.2f, 0f), new Vector3(9f, 4.4f, 7f), i % 2 == 0 ? wall : stone, true);
                Box(hut, "HutRoof", new Vector3(0f, 4.65f, 0f), new Vector3(9.7f, .5f, 7.7f), dark, true);
            }
        }

        private static Transform Root(Transform owner, string name, Vector3 position)
        {
            if (owner == null || owner.Find(name) != null) return null;
            Transform root = new GameObject(name).transform;
            root.SetParent(owner, false);
            root.localPosition = position;
            return root;
        }

        private static Transform Group(Transform parent, string name, Vector3 position)
        {
            Transform group = new GameObject(name).transform;
            group.SetParent(parent, false);
            group.localPosition = position;
            return group;
        }

        private static GameObject Box(Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool collision)
        {
            GameObject go = AndroidSafeMesh.CreateBox(name, parent);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            if (collision) go.AddComponent<BoxCollider>();
            return go;
        }

        private static Material Mat(string resourcePath, Color tint, Vector2 scale)
        {
            Shader shader = Resources.Load<Shader>("Shaders/FspMobileSafe");
            Material material = new(shader) { color = tint, hideFlags = HideFlags.DontSave };
            if (!string.IsNullOrEmpty(resourcePath))
            {
                Texture2D texture = Resources.Load<Texture2D>(resourcePath);
                if (texture != null)
                {
                    material.mainTexture = texture;
                    material.mainTextureScale = scale;
                }
            }
            return material;
        }
    }
}
