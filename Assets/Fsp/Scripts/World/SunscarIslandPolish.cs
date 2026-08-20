using UnityEngine;

namespace Fsp.World
{
    public sealed class SunscarIslandPolish : MonoBehaviour
    {
        private Material sand;
        private Material rock;
        private Material green;
        private Material water;
        private Material roadSign;

        private void Start() => Build();

        private void Build()
        {
            if (transform.Find("GeneratedIslandPolish") != null) return;

            Transform root = new GameObject("GeneratedIslandPolish").transform;
            root.SetParent(transform, false);

            sand = Mat(new Color(.48f, .40f, .28f), "World/sand_ground", new Vector2(18f, 18f));
            rock = Mat(new Color(.38f, .36f, .32f), "World/rock_cliff", new Vector2(5f, 5f));
            green = Mat(new Color(.24f, .31f, .20f), null, Vector2.one);
            water = Mat(new Color(.16f, .38f, .48f), null, Vector2.one);
            roadSign = Mat(new Color(.10f, .16f, .20f), "World/fortress_wall", new Vector2(1f, 1f));

            Block(root, "IslandBase", new Vector3(0, -2.2f, 10), new Vector3(410, 4, 410), sand, true);
            BuildSea(root);
            BuildRidges(root);
            BuildNature(root);
            BuildSigns(root);
        }

        private void BuildSea(Transform root)
        {
            Block(root, "OceanNorth", new Vector3(0, -1.1f, 250), new Vector3(620, .4f, 170), water, false);
            Block(root, "OceanSouth", new Vector3(0, -1.1f, -240), new Vector3(620, .4f, 170), water, false);
            Block(root, "OceanEast", new Vector3(250, -1.1f, 5), new Vector3(170, .4f, 330), water, false);
            Block(root, "OceanWest", new Vector3(-250, -1.1f, 5), new Vector3(170, .4f, 330), water, false);
        }

        private void BuildRidges(Transform root)
        {
            for (int i = 0; i < 32; i++)
            {
                GameObject ridge = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ridge.name = "TerrainRidge";
                ridge.transform.SetParent(root, false);
                float a = i / 32f * Mathf.PI * 2f;
                float radius = 185f + (i % 4) * 5f;
                ridge.transform.localPosition = new Vector3(Mathf.Cos(a) * radius, -.6f, Mathf.Sin(a) * radius + 8f);
                ridge.transform.localScale = new Vector3(22f + (i % 5) * 4f, 4f + (i % 3) * 2f, 16f + (i % 4) * 3f);
                Renderer renderer = ridge.GetComponent<Renderer>();
                if (renderer != null && (i % 3 == 0 ? rock : sand) != null)
                    renderer.sharedMaterial = i % 3 == 0 ? rock : sand;
            }
        }

        private void BuildNature(Transform root)
        {
            for (int i = 0; i < 85; i++)
            {
                float x = -175f + (i * 47) % 350;
                float z = -170f + (i * 73) % 350;
                if (Mathf.Abs(x) < 20f && Mathf.Abs(z) < 20f) continue;

                if (i % 3 == 0)
                {
                    Block(root, "TreeTrunk", new Vector3(x, 1.5f, z), new Vector3(.45f, 3f, .45f), rock, true);
                    GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    crown.name = "TreeCrown";
                    crown.transform.SetParent(root, false);
                    crown.transform.localPosition = new Vector3(x, 3.7f, z);
                    crown.transform.localScale = new Vector3(2.7f, 2.2f, 2.7f);
                    Renderer renderer = crown.GetComponent<Renderer>();
                    if (renderer != null && green != null) renderer.sharedMaterial = green;
                }
                else
                {
                    GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    stone.name = "WorldRock";
                    stone.transform.SetParent(root, false);
                    stone.transform.localPosition = new Vector3(x, .6f, z);
                    float s = .8f + (i % 5) * .3f;
                    stone.transform.localScale = new Vector3(s * 1.4f, s, s);
                    Renderer renderer = stone.GetComponent<Renderer>();
                    if (renderer != null && rock != null) renderer.sharedMaterial = rock;
                }
            }
        }

        private void BuildSigns(Transform root)
        {
            Sign(root, "OLD CROWN", new Vector3(-76, 1.7f, 18), 25);
            Sign(root, "COPPER PORT", new Vector3(55, 1.7f, -35), -20);
            Sign(root, "DRYFIELD", new Vector3(82, 1.7f, 76), 15);
            Sign(root, "WHITE QUARRY", new Vector3(-104, 1.7f, -63), 35);
            Sign(root, "REDLINE", new Vector3(127, 1.7f, -98), -25);
            Sign(root, "SALTWORKS", new Vector3(-10, 1.7f, 138), 10);
            Sign(root, "LANTERN COAST", new Vector3(-130, 1.7f, 86), 30);
        }

        private void Sign(Transform root, string name, Vector3 position, float yaw)
        {
            Transform signRoot = new GameObject("Sign_" + name).transform;
            signRoot.SetParent(root, false);
            signRoot.localPosition = position;
            signRoot.localRotation = Quaternion.Euler(0, yaw, 0);
            Block(signRoot, "PostL", new Vector3(-1.5f, -.2f, 0), new Vector3(.18f, 2.7f, .18f), rock, true);
            Block(signRoot, "PostR", new Vector3(1.5f, -.2f, 0), new Vector3(.18f, 2.7f, .18f), rock, true);
            Block(signRoot, name, new Vector3(0, .8f, 0), new Vector3(4.2f, 1.25f, .2f), roadSign, true);
        }

        private static GameObject Block(Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null && material != null) renderer.sharedMaterial = material;
            if (!collider)
            {
                Collider existing = go.GetComponent<Collider>();
                if (existing != null) Destroy(existing);
            }
            return go;
        }

        private static Material Mat(Color fallbackColor, string resourcePath, Vector2 textureScale)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Debug.LogWarning("FSP world polish: no compatible runtime shader was found; keeping Unity default primitive materials.");
                return null;
            }

            Material material = new Material(shader);
            material.color = fallbackColor;

            if (!string.IsNullOrWhiteSpace(resourcePath))
            {
                Texture2D texture = Resources.Load<Texture2D>(resourcePath);
                if (texture != null)
                {
                    if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
                    if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
                    if (material.HasProperty("_MainTex")) material.SetTextureScale("_MainTex", textureScale);
                    if (material.HasProperty("_BaseMap")) material.SetTextureScale("_BaseMap", textureScale);
                }
            }

            return material;
        }
    }
}
