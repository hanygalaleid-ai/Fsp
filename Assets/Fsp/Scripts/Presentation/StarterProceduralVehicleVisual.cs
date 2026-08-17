using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class StarterProceduralVehicleVisual : MonoBehaviour
    {
        private bool built;

        private void Awake() => Build();

        public void Build()
        {
            if (built) return;
            built = true;

            Renderer baseRenderer = GetComponent<Renderer>();
            if (baseRenderer != null) baseRenderer.enabled = false;

            Material body = MakeMaterial(new Color(0.08f, 0.12f, 0.13f, 1f));
            Material sand = MakeMaterial(new Color(0.46f, 0.38f, 0.27f, 1f));
            Material dark = MakeMaterial(new Color(0.035f, 0.04f, 0.045f, 1f));
            Material bronze = MakeMaterial(new Color(0.50f, 0.28f, 0.11f, 1f));

            Transform root = new GameObject("Fsp_ScoutVehicleVisual").transform;
            root.SetParent(transform, false);

            AddCube(root, "LowerBody", new Vector3(0f, 0.05f, 0f), new Vector3(1.55f, 0.45f, 2.75f), body);
            AddCube(root, "Cabin", new Vector3(0f, 0.52f, -0.15f), new Vector3(1.30f, 0.70f, 1.35f), sand);
            AddCube(root, "Hood", new Vector3(0f, 0.27f, 1.35f), new Vector3(1.35f, 0.32f, 1.10f), sand);
            AddCube(root, "RearDeck", new Vector3(0f, 0.31f, -1.25f), new Vector3(1.35f, 0.30f, 0.85f), body);
            AddCube(root, "FrontBumper", new Vector3(0f, -0.02f, 1.94f), new Vector3(1.58f, 0.18f, 0.18f), bronze);

            AddWheel(root, new Vector3(-0.92f, -0.18f, 1.18f), dark);
            AddWheel(root, new Vector3(0.92f, -0.18f, 1.18f), dark);
            AddWheel(root, new Vector3(-0.92f, -0.18f, -1.18f), dark);
            AddWheel(root, new Vector3(0.92f, -0.18f, -1.18f), dark);
        }

        private static void AddCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            RemoveCollider(go);
            go.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void AddWheel(Transform parent, Vector3 position, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Wheel";
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            go.transform.localScale = new Vector3(0.42f, 0.18f, 0.42f);
            RemoveCollider(go);
            go.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void RemoveCollider(GameObject go)
        {
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
        }

        private static Material MakeMaterial(Color color)
        {
            Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader) { color = color, hideFlags = HideFlags.DontSave };
            return material;
        }
    }
}
