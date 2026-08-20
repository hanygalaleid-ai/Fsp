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
            Material body = Mat(new Color(.20f, .25f, .22f));
            Material dark = Mat(new Color(.05f, .065f, .06f));
            Material accent = Mat(new Color(1f, .30f, .015f));
            Part(root, "Fuselage", Vector3.zero, new Vector3(2.7f, 2.1f, 12f), body);
            Part(root, "Wing", new Vector3(0f, 0f, .3f), new Vector3(17f, .32f, 3f), body);
            Part(root, "TailWing", new Vector3(0f, .35f, -4.8f), new Vector3(6.8f, .22f, 1.7f), body);
            Part(root, "TailFin", new Vector3(0f, 1.8f, -4.7f), new Vector3(.28f, 3.2f, 2.1f), body);
            Part(root, "Nose", new Vector3(0f, 0f, 6.2f), new Vector3(2.3f, 1.8f, 1.0f), dark);
            Part(root, "Stripe", new Vector3(0f, -.88f, .5f), new Vector3(.42f, .08f, 8.5f), accent);
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
