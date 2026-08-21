using System.Collections.Generic;
using UnityEngine;

namespace Fsp.Core
{
    /// <summary>
    /// Creates collider-free meshes without GameObject.CreatePrimitive.
    /// This avoids Android IL2CPP trying to resolve stripped primitive collider classes.
    /// </summary>
    public static class AndroidSafeMesh
    {
        private static Mesh cube;
        private static Mesh sphere;
        private static Mesh cylinder;

        public static GameObject CreateBox(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = CubeMesh();
            go.AddComponent<MeshRenderer>();
            return go;
        }

        public static GameObject CreateSphere(string name, Transform parent = null)
            => Create(name, SphereMesh(), parent);

        public static GameObject CreateCylinder(string name, Transform parent = null)
            => Create(name, CylinderMesh(), parent);

        private static GameObject Create(string name, Mesh mesh, Transform parent)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>();
            return go;
        }

        public static Mesh CubeMesh()
        {
            if (cube != null) return cube;
            cube = new Mesh { name = "FSP_RuntimeCube_NoCollider", hideFlags = HideFlags.DontSave };
            Vector3[] v =
            {
                new(-.5f,-.5f,-.5f), new(.5f,-.5f,-.5f), new(.5f,.5f,-.5f), new(-.5f,.5f,-.5f),
                new(-.5f,-.5f,.5f),  new(.5f,-.5f,.5f),  new(.5f,.5f,.5f),  new(-.5f,.5f,.5f),
                new(-.5f,-.5f,-.5f), new(-.5f,.5f,-.5f), new(-.5f,.5f,.5f), new(-.5f,-.5f,.5f),
                new(.5f,-.5f,-.5f),  new(.5f,.5f,-.5f),  new(.5f,.5f,.5f),  new(.5f,-.5f,.5f),
                new(-.5f,.5f,-.5f),  new(.5f,.5f,-.5f),  new(.5f,.5f,.5f),  new(-.5f,.5f,.5f),
                new(-.5f,-.5f,-.5f), new(.5f,-.5f,-.5f), new(.5f,-.5f,.5f), new(-.5f,-.5f,.5f)
            };
            int[] t =
            {
                0,2,1,0,3,2, 4,5,6,4,6,7,
                8,9,10,8,10,11, 12,14,13,12,15,14,
                16,18,17,16,19,18, 20,21,22,20,22,23
            };
            Vector3[] n = new Vector3[24];
            for (int i=0;i<4;i++) n[i]=Vector3.back;
            for (int i=4;i<8;i++) n[i]=Vector3.forward;
            for (int i=8;i<12;i++) n[i]=Vector3.left;
            for (int i=12;i<16;i++) n[i]=Vector3.right;
            for (int i=16;i<20;i++) n[i]=Vector3.up;
            for (int i=20;i<24;i++) n[i]=Vector3.down;
            Vector2[] uv = new Vector2[24];
            for (int f=0; f<6; f++) { int i=f*4; uv[i]=Vector2.zero; uv[i+1]=Vector2.right; uv[i+2]=Vector2.one; uv[i+3]=Vector2.up; }
            cube.vertices=v; cube.triangles=t; cube.normals=n; cube.uv=uv; cube.RecalculateBounds();
            return cube;
        }

        public static Mesh SphereMesh()
        {
            if (sphere != null) return sphere;
            const int segments = 16;
            const int rings = 10;
            var vertices = new List<Vector3>((segments + 1) * (rings + 1));
            var normals = new List<Vector3>(vertices.Capacity);
            var uv = new List<Vector2>(vertices.Capacity);
            var triangles = new List<int>(segments * rings * 6);

            for (int ring = 0; ring <= rings; ring++)
            {
                float v = ring / (float)rings;
                float latitude = Mathf.PI * v;
                float y = Mathf.Cos(latitude) * .5f;
                float radius = Mathf.Sin(latitude) * .5f;
                for (int segment = 0; segment <= segments; segment++)
                {
                    float u = segment / (float)segments;
                    float longitude = u * Mathf.PI * 2f;
                    Vector3 point = new(Mathf.Sin(longitude) * radius, y, Mathf.Cos(longitude) * radius);
                    vertices.Add(point);
                    normals.Add(point.normalized);
                    uv.Add(new Vector2(u, 1f - v));
                }
            }

            for (int ring = 0; ring < rings; ring++)
            {
                int row = segments + 1;
                for (int segment = 0; segment < segments; segment++)
                {
                    int a = ring * row + segment;
                    int b = a + row;
                    triangles.Add(a); triangles.Add(b); triangles.Add(a + 1);
                    triangles.Add(a + 1); triangles.Add(b); triangles.Add(b + 1);
                }
            }

            sphere = new Mesh { name = "FSP_RuntimeSphere_NoCollider", hideFlags = HideFlags.DontSave };
            sphere.SetVertices(vertices);
            sphere.SetNormals(normals);
            sphere.SetUVs(0, uv);
            sphere.SetTriangles(triangles, 0);
            sphere.RecalculateBounds();
            return sphere;
        }

        public static Mesh CylinderMesh()
        {
            if (cylinder != null) return cylinder;
            const int segments = 16;
            var vertices = new List<Vector3>(segments * 4 + 2);
            var normals = new List<Vector3>(segments * 4 + 2);
            var uv = new List<Vector2>(segments * 4 + 2);
            var triangles = new List<int>(segments * 12);

            for (int i = 0; i <= segments; i++)
            {
                float u = i / (float)segments;
                float angle = u * Mathf.PI * 2f;
                Vector3 normal = new(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                vertices.Add(new Vector3(normal.x * .5f, -.5f, normal.z * .5f));
                vertices.Add(new Vector3(normal.x * .5f, .5f, normal.z * .5f));
                normals.Add(normal); normals.Add(normal);
                uv.Add(new Vector2(u, 0f)); uv.Add(new Vector2(u, 1f));
            }
            for (int i = 0; i < segments; i++)
            {
                int a = i * 2;
                triangles.Add(a); triangles.Add(a + 2); triangles.Add(a + 1);
                triangles.Add(a + 2); triangles.Add(a + 3); triangles.Add(a + 1);
            }

            int bottomCenter = vertices.Count;
            vertices.Add(new Vector3(0f, -.5f, 0f)); normals.Add(Vector3.down); uv.Add(new Vector2(.5f, .5f));
            int topCenter = vertices.Count;
            vertices.Add(new Vector3(0f, .5f, 0f)); normals.Add(Vector3.up); uv.Add(new Vector2(.5f, .5f));
            int rimStart = vertices.Count;
            for (int i = 0; i < segments; i++)
            {
                float angle = i / (float)segments * Mathf.PI * 2f;
                float x = Mathf.Sin(angle) * .5f;
                float z = Mathf.Cos(angle) * .5f;
                vertices.Add(new Vector3(x, -.5f, z)); normals.Add(Vector3.down); uv.Add(new Vector2(x + .5f, z + .5f));
                vertices.Add(new Vector3(x, .5f, z)); normals.Add(Vector3.up); uv.Add(new Vector2(x + .5f, z + .5f));
            }
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                triangles.Add(bottomCenter); triangles.Add(rimStart + next * 2); triangles.Add(rimStart + i * 2);
                triangles.Add(topCenter); triangles.Add(rimStart + i * 2 + 1); triangles.Add(rimStart + next * 2 + 1);
            }

            cylinder = new Mesh { name = "FSP_RuntimeCylinder_NoCollider", hideFlags = HideFlags.DontSave };
            cylinder.SetVertices(vertices);
            cylinder.SetNormals(normals);
            cylinder.SetUVs(0, uv);
            cylinder.SetTriangles(triangles, 0);
            cylinder.RecalculateBounds();
            return cylinder;
        }
    }
}
