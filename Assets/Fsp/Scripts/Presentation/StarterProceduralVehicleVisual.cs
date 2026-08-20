using Fsp.Core;
using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>Original lightweight scout vehicle visual for the runtime mobile fleet.</summary>
    public sealed class StarterProceduralVehicleVisual : MonoBehaviour
    {
        private void Awake() => Build();

        public void Build()
        {
            if (transform.Find("FSP_ScoutVehicleVisual") != null) return;
            Transform root = new GameObject("FSP_ScoutVehicleVisual").transform;
            root.SetParent(transform, false);
            Material body = Mat(new Color(.17f, .25f, .14f));
            Material dark = Mat(new Color(.035f, .045f, .04f));
            Material glass = Mat(new Color(.08f, .18f, .22f));
            Material light = Mat(new Color(1f, .55f, .12f));
            Part(root, "Chassis", new Vector3(0f, .45f, 0f), new Vector3(2.1f, .45f, 3.8f), body);
            Part(root, "Cabin", new Vector3(0f, 1.05f, -.25f), new Vector3(1.75f, .85f, 1.75f), body);
            Part(root, "Windshield", new Vector3(0f, 1.16f, .66f), new Vector3(1.48f, .48f, .05f), glass);
            Part(root, "Bumper", new Vector3(0f, .42f, 2.02f), new Vector3(2.2f, .22f, .16f), dark);
            Part(root, "Rear", new Vector3(0f, .62f, -1.64f), new Vector3(1.9f, .65f, .55f), body);
            Part(root, "HeadlightL", new Vector3(-.66f, .62f, 2.03f), new Vector3(.34f, .20f, .08f), light);
            Part(root, "HeadlightR", new Vector3(.66f, .62f, 2.03f), new Vector3(.34f, .20f, .08f), light);
            Wheel(root, new Vector3(-1.03f, .37f, 1.25f), dark);
            Wheel(root, new Vector3(1.03f, .37f, 1.25f), dark);
            Wheel(root, new Vector3(-1.03f, .37f, -1.25f), dark);
            Wheel(root, new Vector3(1.03f, .37f, -1.25f), dark);
        }

        private static void Wheel(Transform root, Vector3 position, Material material)
            => Part(root, "Wheel", position, new Vector3(.32f, .66f, .66f), material);

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
