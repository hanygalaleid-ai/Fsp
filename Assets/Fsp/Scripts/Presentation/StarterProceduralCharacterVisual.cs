using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>
    /// Creates a lightweight original stylized field-operator visual from Unity primitives.
    /// It is intentionally mobile-friendly and acts as a deterministic fallback when no rigged model is shipped.
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

            Material navy = MakeMaterial("Fsp_Navy_Runtime", new Color(0.045f, 0.065f, 0.085f, 1f));
            Material sand = MakeMaterial("Fsp_Sand_Runtime", new Color(0.40f, 0.31f, 0.20f, 1f));
            Material cloth = MakeMaterial("Fsp_Cloth_Runtime", new Color(0.13f, 0.14f, 0.14f, 1f));
            Material skin = MakeMaterial("Fsp_Skin_Runtime", new Color(0.48f, 0.31f, 0.22f, 1f));
            Material bronze = MakeMaterial("Fsp_Bronze_Runtime", FspFixedTheme.Bronze);
            Material weapon = MakeMaterial("Fsp_Weapon_Runtime", new Color(0.07f, 0.075f, 0.08f, 1f));

            AddPart(PrimitiveType.Capsule, "Torso", new Vector3(0f, 1.18f, 0f), new Vector3(0.56f, 0.58f, 0.38f), Quaternion.identity, sand);
            AddPart(PrimitiveType.Sphere, "Head", new Vector3(0f, 1.83f, 0.02f), new Vector3(0.36f, 0.40f, 0.34f), Quaternion.identity, skin);
            AddPart(PrimitiveType.Sphere, "Helmet", new Vector3(0f, 1.91f, 0f), new Vector3(0.40f, 0.23f, 0.38f), Quaternion.identity, navy);
            AddPart(PrimitiveType.Cube, "FaceWrap", new Vector3(0f, 1.77f, 0.17f), new Vector3(0.32f, 0.14f, 0.08f), Quaternion.identity, cloth);
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

            AddPart(PrimitiveType.Cube, "RifleBody", new Vector3(0.23f, 1.22f, 0.40f), new Vector3(0.12f, 0.12f, 0.82f), Quaternion.Euler(0f, 18f, -18f), weapon);
            AddPart(PrimitiveType.Cube, "RifleStock", new Vector3(0.38f, 1.34f, 0.05f), new Vector3(0.12f, 0.16f, 0.30f), Quaternion.Euler(0f, 18f, -18f), sand);
            AddPart(PrimitiveType.Cylinder, "RifleBarrel", new Vector3(0.03f, 1.08f, 0.82f), new Vector3(0.035f, 0.26f, 0.035f), Quaternion.Euler(90f, 0f, 18f), weapon);
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
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
        }

        private static Material MakeMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var material = new Material(shader) { name = name, color = color };
            material.hideFlags = HideFlags.DontSave;
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.18f);
            return material;
        }
    }
}
