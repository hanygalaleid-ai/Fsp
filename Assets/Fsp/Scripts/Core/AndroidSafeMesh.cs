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

        public static GameObject CreateBox(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = CubeMesh();
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
    }
}
