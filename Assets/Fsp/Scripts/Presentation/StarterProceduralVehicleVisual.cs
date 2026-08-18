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

            Material body = MakeMaterial(new Color(0.055f, 0.075f, 0.08f, 1f));
            Material sand = MakeMaterial(new Color(0.39f, 0.31f, 0.20f, 1f));
            Material dark = MakeMaterial(new Color(0.025f, 0.028f, 0.03f, 1f));
            Material bronze = MakeMaterial(FspFixedTheme.Bronze);
            Material glass = MakeMaterial(new Color(0.12f, 0.20f, 0.23f, 1f));

            Transform root = new GameObject("Fsp_ScoutVehicleVisual").transform;
            root.SetParent(transform, false);

            AddCube(root, "LowerBody", new Vector3(0f, 0.05f, 0f), new Vector3(1.55f, 0.45f, 2.75f), body);
            AddCube(root, "Cabin", new Vector3(0f, 0.52f, -0.15f), new Vector3(1.30f, 0.70f, 1.35f), sand);
            AddCube(root, "Windshield", new Vector3(0f, 0.72f, 0.50f), new Vector3(1.05f, 0.38f, 0.05f), glass);
            AddCube(root, "Hood", new Vector3(0f, 0.27f, 1.35f), new Vector3(1.35f, 0.32f, 1.10f), sand);
            AddCube(root, "RearDeck", new Vector3(0f, 0.31f, -1.25f), new Vector3(1.35f, 0.30f, 0.85f), body);
            AddCube(root, "FrontBumper", new Vector3(0f, -0.02f, 1.94f), new Vector3(1.58f, 0.18f, 0.18f), bronze);
            AddCube(root, "RollBarTop", new Vector3(0f, 1.10f, -0.42f), new Vector3(1.25f, 0.08f, 0.08f), dark);
            AddCube(root, "RollBarL", new Vector3(-0.58f, 0.82f, -0.42f), new Vector3(0.08f, 0.65f, 0.08f), dark);
            AddCube(root, "RollBarR", new Vector3(0.58f, 0.82f, -0.42f), new Vector3(0.08f, 0.65f, 0.08f), dark);
            AddCube(root, "HeadlightL", new Vector3(-0.48f, 0.20f, 1.91f), new Vector3(0.22f, 0.16f, 0.06f), bronze);
            AddCube(root, "HeadlightR", new Vector3(0.48f, 0.20f, 1.91f), new Vector3(0.22f, 0.16f, 0.06f), bronze);

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
            Renderer renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
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
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var material = new Material(shader) { color = color, hideFlags = HideFlags.DontSave };
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.22f);
            return material;
        }
    }
}
