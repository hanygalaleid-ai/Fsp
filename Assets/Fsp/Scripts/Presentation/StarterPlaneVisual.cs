using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>Builds a lightweight original transport-plane visual from Unity primitives.</summary>
    public sealed class StarterPlaneVisual : MonoBehaviour
    {
        private void Awake()
        {
            // The gameplay plane may be a cube fallback. Keep its transform/controller, but never
            // render that cube once the presentation visual is available.
            Renderer fallbackRenderer = GetComponent<Renderer>();
            if (fallbackRenderer != null) fallbackRenderer.enabled = false;

            if (transform.Find("VisualRoot") != null) return;
            Transform root = new GameObject("VisualRoot").transform;
            root.SetParent(transform, false);

            Material navy = MakeMaterial(new Color(0.08f, 0.12f, 0.16f));
            Material sand = MakeMaterial(new Color(0.55f, 0.48f, 0.34f));
            Material bronze = MakeMaterial(new Color(0.47f, 0.31f, 0.16f));

            Part(root, PrimitiveType.Capsule, "Fuselage", new Vector3(0f,0f,0f), new Vector3(1.7f,3.6f,1.7f), Quaternion.Euler(90f,0f,0f), navy);
            Part(root, PrimitiveType.Cube, "Wing", new Vector3(0f,0f,-0.15f), new Vector3(10f,0.22f,2.1f), Quaternion.identity, sand);
            Part(root, PrimitiveType.Cube, "TailWing", new Vector3(0f,0.25f,-3.05f), new Vector3(4f,0.15f,1.15f), Quaternion.identity, sand);
            Part(root, PrimitiveType.Cube, "TailFin", new Vector3(0f,1.0f,-3.0f), new Vector3(0.25f,1.7f,1.2f), Quaternion.identity, bronze);
            Part(root, PrimitiveType.Cube, "NoseStripe", new Vector3(0f,0f,2.65f), new Vector3(1.8f,1.45f,0.25f), Quaternion.identity, bronze);
            Engine(root, new Vector3(-2.35f,-0.12f,0.35f), navy, bronze);
            Engine(root, new Vector3(2.35f,-0.12f,0.35f), navy, bronze);
        }

        private static void Engine(Transform root, Vector3 pos, Material body, Material accent)
        {
            Part(root, PrimitiveType.Cylinder, "Engine", pos, new Vector3(0.7f,0.95f,0.7f), Quaternion.Euler(90f,0f,0f), body);
            Part(root, PrimitiveType.Cylinder, "PropHub", pos + new Vector3(0f,0f,0.7f), new Vector3(0.3f,0.12f,0.3f), Quaternion.Euler(90f,0f,0f), accent);
            Part(root, PrimitiveType.Cube, "PropA", pos + new Vector3(0f,0f,0.86f), new Vector3(0.12f,2.0f,0.08f), Quaternion.identity, accent);
            Part(root, PrimitiveType.Cube, "PropB", pos + new Vector3(0f,0f,0.86f), new Vector3(2.0f,0.12f,0.08f), Quaternion.identity, accent);
        }

        private static GameObject Part(Transform parent, PrimitiveType type, string name, Vector3 pos, Vector3 scale, Quaternion rot, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localRotation = rot; go.transform.localScale = scale;
            Collider c = go.GetComponent<Collider>(); if (c != null) Object.Destroy(c);
            Renderer r = go.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = mat;
            return go;
        }

        private static Material MakeMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            Material mat = new Material(shader);
            mat.color = color;
            mat.hideFlags = HideFlags.DontSave;
            return mat;
        }
    }
}
