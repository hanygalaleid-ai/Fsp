using UnityEngine;

namespace Fsp.World
{
    /// <summary>
    /// Mobile-safe distant horizon. The checked-in cylinder is visual only and follows the
    /// gameplay camera horizontally so neither the drop route nor the island edge exposes a
    /// flat clear colour.
    /// </summary>
    public sealed class SunscarSkyBackdrop : MonoBehaviour
    {
        private Transform cameraTransform;
        private Transform cylinder;

        private void Awake()
        {
            cameraTransform = Camera.main != null ? Camera.main.transform : null;
            Build();
        }

        private void LateUpdate()
        {
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
            if (cameraTransform == null || cylinder == null) return;
            Vector3 cameraPosition = cameraTransform.position;
            cylinder.position = new Vector3(cameraPosition.x, 0f, cameraPosition.z);
        }

        private void Build()
        {
            if (transform.Find("SunscarSkyCylinder") != null) return;
            Texture2D panorama = Resources.Load<Texture2D>("World/sunscar_sky_panorama");
            Shader shader = Resources.Load<Shader>("Shaders/FspMobileBackdrop");
            if (panorama == null || shader == null)
            {
                Debug.LogError("BMG Match: checked-in Sunscar sky panorama or backdrop shader is missing.");
                return;
            }

            GameObject go = new("SunscarSkyCylinder", typeof(MeshFilter), typeof(MeshRenderer));
            cylinder = go.transform;
            cylinder.SetParent(transform, false);
            cylinder.localPosition = Vector3.zero;

            go.GetComponent<MeshFilter>().sharedMesh = BuildCylinder(390f, -55f, 430f, 64);
            Material material = new(shader) { name = "BMG_SunscarSky", hideFlags = HideFlags.DontSave };
            material.mainTexture = panorama;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static Mesh BuildCylinder(float radius, float bottom, float top, int segments)
        {
            Vector3[] vertices = new Vector3[(segments + 1) * 2];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[segments * 6];
            for (int i = 0; i <= segments; i++)
            {
                float u = i / (float)segments;
                float angle = u * Mathf.PI * 2f;
                float x = Mathf.Sin(angle) * radius;
                float z = Mathf.Cos(angle) * radius;
                vertices[i * 2] = new Vector3(x, bottom, z);
                vertices[i * 2 + 1] = new Vector3(x, top, z);
                uv[i * 2] = new Vector2(u, 0f);
                uv[i * 2 + 1] = new Vector2(u, 1f);
                if (i == segments) continue;
                int v = i * 2;
                int t = i * 6;
                triangles[t] = v;
                triangles[t + 1] = v + 1;
                triangles[t + 2] = v + 2;
                triangles[t + 3] = v + 2;
                triangles[t + 4] = v + 1;
                triangles[t + 5] = v + 3;
            }

            Mesh mesh = new() { name = "BMG_SunscarSkyCylinder", hideFlags = HideFlags.DontSave };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
