using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class StarterProceduralWeaponVisual : MonoBehaviour
    {
        private bool built;

        private void Awake() => Build();

        public void Build()
        {
            if (built) return;
            built = true;

            Renderer baseRenderer = GetComponent<Renderer>();
            if (baseRenderer != null) baseRenderer.enabled = false;

            Material dark = MakeMaterial(new Color(0.055f, 0.065f, 0.07f, 1f));
            Material sand = MakeMaterial(new Color(0.42f, 0.34f, 0.23f, 1f));
            Material bronze = MakeMaterial(new Color(0.50f, 0.29f, 0.12f, 1f));

            Transform root = new GameObject("Dune_AR4_Visual").transform;
            root.SetParent(transform, false);

            AddCube(root, "Receiver", new Vector3(0f, 0f, 0.05f), new Vector3(0.14f, 0.18f, 0.56f), dark);
            AddCube(root, "Handguard", new Vector3(0f, 0f, 0.45f), new Vector3(0.12f, 0.14f, 0.48f), sand);
            AddCube(root, "Stock", new Vector3(0f, 0.02f, -0.43f), new Vector3(0.12f, 0.19f, 0.34f), sand);
            AddCube(root, "Magazine", new Vector3(0f, -0.16f, 0.03f), new Vector3(0.10f, 0.28f, 0.15f), bronze, Quaternion.Euler(10f, 0f, 0f));
            AddCube(root, "Sight", new Vector3(0f, 0.14f, 0.10f), new Vector3(0.08f, 0.07f, 0.16f), dark);
            AddCube(root, "Barrel", new Vector3(0f, 0f, 0.80f), new Vector3(0.05f, 0.05f, 0.36f), dark);
        }

        private static void AddCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material, Quaternion? rotation = null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.transform.localRotation = rotation ?? Quaternion.identity;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            go.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static Material MakeMaterial(Color color)
        {
            Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
            return new Material(shader) { color = color, hideFlags = HideFlags.DontSave };
        }
    }
}
