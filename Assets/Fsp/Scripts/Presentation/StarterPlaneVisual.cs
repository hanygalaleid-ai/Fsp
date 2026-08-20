using Fsp.Core;
using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>Original low-poly transport plane visual for the battle-royale drop route.</summary>
    public sealed class StarterPlaneVisual : MonoBehaviour
    {
        private void Awake()
        {
            if (transform.Find("FSP_TransportPlaneVisual") != null) return;
            Transform root = new GameObject("FSP_TransportPlaneVisual").transform;
            root.SetParent(transform, false);
            Material body = Mat(new Color(.38f, .45f, .40f));
            Material bodyLight = Mat(new Color(.48f, .53f, .47f));
            Material dark = Mat(new Color(.10f, .14f, .14f));
            Material glass = Mat(new Color(.12f, .34f, .45f));
            Material accent = Mat(new Color(1f, .30f, .015f));
            Part(root, "Fuselage", Vector3.zero, new Vector3(2.8f, 2.2f, 12.5f), body);
            Part(root, "UpperFuselage", new Vector3(0f, .92f, .5f), new Vector3(2.15f, .45f, 8.8f), bodyLight);
            Part(root, "Wing", new Vector3(0f, .05f, .3f), new Vector3(18f, .34f, 3.2f), body);
            Part(root, "TailWing", new Vector3(0f, .35f, -4.8f), new Vector3(6.8f, .22f, 1.7f), body);
            Part(root, "TailFin", new Vector3(0f, 1.8f, -4.7f), new Vector3(.28f, 3.2f, 2.1f), body);
            Part(root, "Nose", new Vector3(0f, 0f, 6.2f), new Vector3(2.3f, 1.8f, 1.0f), dark);
            Part(root, "CockpitGlass", new Vector3(0f, .58f, 6.65f), new Vector3(1.7f, .48f, .12f), glass);
            Part(root, "Stripe", new Vector3(0f, -.88f, .5f), new Vector3(.42f, .08f, 8.5f), accent);
            Engine(root, -5.2f, .15f, 1.2f, dark, accent);
            Engine(root, 5.2f, .15f, 1.2f, dark, accent);
            Engine(root, -7.1f, .10f, -.15f, dark, accent);
            Engine(root, 7.1f, .10f, -.15f, dark, accent);
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < 5; i++)
                    Part(root, "CabinWindow", new Vector3(side * 1.43f, .55f, 3.6f - i * 1.45f), new Vector3(.08f, .30f, .58f), glass);
        }

        private static void Engine(Transform root, float x, float y, float z, Material dark, Material accent)
        {
            Part(root, "Engine", new Vector3(x, y, z), new Vector3(1.35f, 1.25f, 2.5f), dark);
            Part(root, "EngineIntake", new Vector3(x, y, z + 1.27f), new Vector3(.82f, .82f, .10f), accent);
        }

        private static void Part(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = AndroidSafeMesh.CreateBox(name, parent);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
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
