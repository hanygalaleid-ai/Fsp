using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>
    /// Creates a lightweight original stylized field-operator visual from Unity primitives.
    /// It is intentionally simple and mobile-friendly, and can later be replaced by the final rigged model.
    /// </summary>
    public sealed class StarterProceduralCharacterVisual : MonoBehaviour
    {
        [SerializeField] private bool buildOnAwake = true;
        private Transform visualRoot;

        private void Awake()
        {
            if (buildOnAwake) Build();
        }

        public void Build()
        {
            if (visualRoot != null) return;

            Renderer rootRenderer = GetComponent<Renderer>();
            if (rootRenderer != null) rootRenderer.enabled = false;

            var root = new GameObject("Fsp_CharacterVisual");
            root.transform.SetParent(transform, false);
            visualRoot = root.transform;

            Material navy = MakeMaterial("Fsp_Navy_Runtime", new Color(0.055f, 0.09f, 0.13f, 1f));
            Material sand = MakeMaterial("Fsp_Sand_Runtime", new Color(0.56f, 0.46f, 0.32f, 1f));
            Material cloth = MakeMaterial("Fsp_Cloth_Runtime", new Color(0.24f, 0.25f, 0.23f, 1f));
            Material skin = MakeMaterial("Fsp_Skin_Runtime", new Color(0.55f, 0.37f, 0.27f, 1f));
            Material bronze = MakeMaterial("Fsp_Bronze_Runtime", new Color(0.55f, 0.32f, 0.13f, 1f));

            AddPart(PrimitiveType.Capsule, "Torso", new Vector3(0f, 1.18f, 0f), new Vector3(0.56f, 0.58f, 0.38f), Quaternion.identity, sand);
            AddPart(PrimitiveType.Sphere, "Head", new Vector3(0f, 1.83f, 0.02f), new Vector3(0.36f, 0.40f, 0.34f), Quaternion.identity, skin);
            AddPart(PrimitiveType.Cube, "ChestRig", new Vector3(0f, 1.28f, 0.18f), new Vector3(0.62f, 0.38f, 0.13f), Quaternion.identity, navy);
            AddPart(PrimitiveType.Cube, "Belt", new Vector3(0f, 0.90f, 0f), new Vector3(0.58f, 0.12f, 0.35f), Quaternion.identity, bronze);

            AddPart(PrimitiveType.Capsule, "ArmL", new Vector3(-0.39f, 1.24f, 0f), new Vector3(0.18f, 0.48f, 0.18f), Quaternion.Euler(0f, 0f, -7f), cloth);
            AddPart(PrimitiveType.Capsule, "ArmR", new Vector3(0.39f, 1.24f, 0f), new Vector3(0.18f, 0.48f, 0.18f), Quaternion.Euler(0f, 0f, 7f), cloth);

            AddPart(PrimitiveType.Capsule, "LegL", new Vector3(-0.18f, 0.49f, 0f), new Vector3(0.22f, 0.55f, 0.22f), Quaternion.identity, navy);
            AddPart(PrimitiveType.Capsule, "LegR", new Vector3(0.18f, 0.49f, 0f), new Vector3(0.22f, 0.55f, 0.22f), Quaternion.identity, navy);
            AddPart(PrimitiveType.Cube, "BootL", new Vector3(-0.18f, 0.08f, 0.09f), new Vector3(0.23f, 0.16f, 0.42f), Quaternion.identity, cloth);
            AddPart(PrimitiveType.Cube, "BootR", new Vector3(0.18f, 0.08f, 0.09f), new Vector3(0.23f, 0.16f, 0.42f), Quaternion.identity, cloth);

            AddPart(PrimitiveType.Cube, "Backpack", new Vector3(0f, 1.20f, -0.24f), new Vector3(0.50f, 0.58f, 0.20f), Quaternion.Euler(4f, 0f, 0f), cloth);
            AddPart(PrimitiveType.Cube, "ShoulderAccent", new Vector3(-0.31f, 1.48f, 0.10f), new Vector3(0.18f, 0.11f, 0.20f), Quaternion.Euler(0f, 0f, -10f), bronze);
        }

        private void AddPart(PrimitiveType type, string name, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(visualRoot, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
        }

        private static Material MakeMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader) { name = name, color = color };
            material.hideFlags = HideFlags.DontSave;
            return material;
        }
    }
}
