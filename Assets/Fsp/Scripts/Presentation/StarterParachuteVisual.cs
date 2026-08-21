using Fsp.BattleRoyale;
using Fsp.Core;
using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>Original curved low-poly parachute canopy shown by ParachuteController.</summary>
    public sealed class StarterParachuteVisual : MonoBehaviour
    {
        [SerializeField] private GameObject authoredVisual;

        private void Awake() => Build();

        public void Build()
        {
            if (authoredVisual != null) return;
            authoredVisual = new GameObject("FSP_ParachuteVisual");
            authoredVisual.transform.SetParent(transform, false);
            authoredVisual.transform.localPosition = new Vector3(0f, 4.2f, 0f);
            Color selectedCanopy = StarterCosmeticCatalog.Find(
                StarterWardrobeRuntime.LoadLocal().parachuteItemId, CosmeticSlot.Parachute).Color;
            Material canopy = Mat(selectedCanopy);
            Material accent = Mat(new Color(1f, .34f, .015f));
            CreateCanopy("Canopy", authoredVisual.transform, 5.4f, 2.8f, 0f, canopy);
            CreateCanopy("CanopyStripe", authoredVisual.transform, .62f, 2.82f, .025f, accent);

            Material lines = Mat(new Color(.76f, .72f, .62f));
            CreateLine(authoredVisual.transform, new Vector3(-2.35f, -.50f, 1.05f), new Vector3(-.34f, -4.08f, .18f), lines);
            CreateLine(authoredVisual.transform, new Vector3(2.35f, -.50f, 1.05f), new Vector3(.34f, -4.08f, .18f), lines);
            CreateLine(authoredVisual.transform, new Vector3(-2.35f, -.50f, -1.05f), new Vector3(-.34f, -4.08f, -.18f), lines);
            CreateLine(authoredVisual.transform, new Vector3(2.35f, -.50f, -1.05f), new Vector3(.34f, -4.08f, -.18f), lines);
            authoredVisual.SetActive(false);
            GetComponent<ParachuteController>()?.ConfigureVisual(authoredVisual);
        }

        public void Show(bool visible)
        {
            if (authoredVisual != null) authoredVisual.SetActive(visible);
        }

        private static void CreateCanopy(string name, Transform parent, float width, float depth, float lift, Material material)
        {
            const int across = 14;
            const int along = 7;
            Vector3[] vertices = new Vector3[(across + 1) * (along + 1)];
            Vector2[] uv = new Vector2[vertices.Length];
            // Two-sided triangles keep the canopy visible both from the player below and cameras above.
            int[] triangles = new int[across * along * 12];
            for (int z = 0; z <= along; z++)
            {
                float vz = z / (float)along;
                for (int x = 0; x <= across; x++)
                {
                    float vx = x / (float)across;
                    float nx = vx * 2f - 1f;
                    float nz = vz * 2f - 1f;
                    float arch = .72f * (1f - nx * nx) + .12f * (1f - nz * nz);
                    int index = z * (across + 1) + x;
                    vertices[index] = new Vector3(nx * width * .5f, arch + lift, nz * depth * .5f);
                    uv[index] = new Vector2(vx, vz);
                }
            }
            int triangle = 0;
            for (int z = 0; z < along; z++)
            for (int x = 0; x < across; x++)
            {
                int a = z * (across + 1) + x;
                int b = a + across + 1;
                triangles[triangle++] = a; triangles[triangle++] = b; triangles[triangle++] = a + 1;
                triangles[triangle++] = a + 1; triangles[triangle++] = b; triangles[triangle++] = b + 1;
                triangles[triangle++] = a + 1; triangles[triangle++] = b; triangles[triangle++] = a;
                triangles[triangle++] = b + 1; triangles[triangle++] = b; triangles[triangle++] = a + 1;
            }
            Mesh mesh = new() { name = "FSP_CurvedParachute", hideFlags = HideFlags.DontSave };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            GameObject go = new(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void CreateLine(Transform parent, Vector3 start, Vector3 end, Material material)
        {
            Vector3 delta = end - start;
            GameObject line = AndroidSafeMesh.CreateCylinder("SuspensionLine", parent);
            line.transform.localPosition = (start + end) * .5f;
            line.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
            line.transform.localScale = new Vector3(.025f, delta.magnitude, .025f);
            line.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static Material Mat(Color color)
        {
            Shader shader = Resources.Load<Shader>("Shaders/FspMobileSafe");
            if (shader == null) shader = Shader.Find("Fsp/MobileSafeLit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            return new Material(shader) { color = color, hideFlags = HideFlags.DontSave };
        }
    }
}
