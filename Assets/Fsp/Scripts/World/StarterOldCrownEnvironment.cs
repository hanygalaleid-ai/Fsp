using UnityEngine;

namespace Fsp.World
{
    /// <summary>Small prototype slice of Old Crown built entirely from reusable Unity primitives.</summary>
    public sealed class StarterOldCrownEnvironment : MonoBehaviour
    {
        [SerializeField] private Vector2 areaSize = new Vector2(150f, 150f);
        [SerializeField] private int buildingCount = 18;
        [SerializeField] private int coverCount = 28;

        private void Awake()
        {
            if (transform.Find("GeneratedOldCrown") != null) return;
            Build();
        }

        private void Build()
        {
            Transform root = new GameObject("GeneratedOldCrown").transform;
            root.SetParent(transform, false);

            Material stone = MakeMaterial(new Color(0.48f, 0.43f, 0.35f));
            Material plaster = MakeMaterial(new Color(0.68f, 0.62f, 0.51f));
            Material navy = MakeMaterial(new Color(0.08f, 0.12f, 0.17f));
            Material bronze = MakeMaterial(new Color(0.45f, 0.30f, 0.17f));

            GameObject ground = Part(root, PrimitiveType.Cube, "Ground", new Vector3(0f,-0.5f,0f), new Vector3(areaSize.x,1f,areaSize.y), Quaternion.identity, stone, true);

            var rng = new System.Random(2701);
            for (int i = 0; i < buildingCount; i++)
            {
                float angle = (float)(rng.NextDouble() * Mathf.PI * 2f);
                float radius = Mathf.Lerp(22f, 62f, (float)rng.NextDouble());
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                int floors = 1 + rng.Next(0, 3);
                float width = 7f + (float)rng.NextDouble() * 5f;
                float depth = 7f + (float)rng.NextDouble() * 5f;
                BuildHouse(root, "House_" + i, pos, width, depth, floors, i % 2 == 0 ? plaster : stone, navy, bronze);
            }

            // central monument / landmark
            Part(root, PrimitiveType.Cylinder, "CrownTower", new Vector3(0f,5f,0f), new Vector3(4.2f,5f,4.2f), Quaternion.identity, stone, true);
            Part(root, PrimitiveType.Cube, "CrownTop", new Vector3(0f,10.4f,0f), new Vector3(7f,0.8f,7f), Quaternion.identity, bronze, true);

            for (int i = 0; i < coverCount; i++)
            {
                float x = Mathf.Lerp(-62f,62f,(float)rng.NextDouble());
                float z = Mathf.Lerp(-62f,62f,(float)rng.NextDouble());
                Vector3 scale = (i % 3 == 0) ? new Vector3(3.2f,1.2f,0.65f) : new Vector3(1.2f,1.0f,1.2f);
                Part(root, PrimitiveType.Cube, "Cover_" + i, new Vector3(x,scale.y*0.5f,z), scale, Quaternion.Euler(0f,(float)rng.NextDouble()*180f,0f), stone, true);
            }

            // roads forming a cross and ring-like movement lanes
            Part(root, PrimitiveType.Cube, "Road_NS", new Vector3(0f,0.02f,0f), new Vector3(9f,0.04f,130f), Quaternion.identity, navy, false);
            Part(root, PrimitiveType.Cube, "Road_EW", new Vector3(0f,0.021f,0f), new Vector3(130f,0.04f,9f), Quaternion.identity, navy, false);
        }

        private static void BuildHouse(Transform root, string name, Vector3 pos, float width, float depth, int floors, Material wall, Material accent, Material roof)
        {
            Transform house = new GameObject(name).transform;
            house.SetParent(root, false);
            house.localPosition = pos;
            float totalHeight = floors * 3.2f;
            Part(house, PrimitiveType.Cube, "Body", new Vector3(0f,totalHeight*0.5f,0f), new Vector3(width,totalHeight,depth), Quaternion.identity, wall, true);
            Part(house, PrimitiveType.Cube, "Roof", new Vector3(0f,totalHeight+0.35f,0f), new Vector3(width+0.4f,0.7f,depth+0.4f), Quaternion.identity, roof, true);
            Part(house, PrimitiveType.Cube, "Door", new Vector3(0f,1.15f,depth*0.5f+0.04f), new Vector3(1.4f,2.3f,0.12f), Quaternion.identity, accent, false);
            if (floors > 1)
                Part(house, PrimitiveType.Cube, "WindowBand", new Vector3(0f,4.1f,depth*0.5f+0.05f), new Vector3(width*0.55f,0.7f,0.10f), Quaternion.identity, accent, false);
        }

        private static GameObject Part(Transform parent, PrimitiveType type, string name, Vector3 pos, Vector3 scale, Quaternion rot, Material mat, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localRotation = rot; go.transform.localScale = scale;
            Collider c = go.GetComponent<Collider>(); if (!collider && c != null) Object.Destroy(c);
            Renderer r = go.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = mat;
            return go;
        }

        private static Material MakeMaterial(Color color)
        {
            Shader shader = Shader.Find("Standard");
            Material mat = new Material(shader != null ? shader : Shader.Find("Sprites/Default"));
            mat.color = color; return mat;
        }
    }
}
