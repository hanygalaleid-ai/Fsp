using Fsp.BattleRoyale;
using Fsp.Core;
using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>Original lightweight parachute canopy shown by ParachuteController.</summary>
    public sealed class StarterParachuteVisual : MonoBehaviour
    {
        [SerializeField] private GameObject authoredVisual;

        private void Awake() => Build();

        public void Build()
        {
            if (authoredVisual != null) return;
            authoredVisual = new GameObject("FSP_ParachuteVisual");
            authoredVisual.transform.SetParent(transform, false);
            authoredVisual.transform.localPosition = new Vector3(0f, 4.2f, 0f);
            Color selectedCanopy = StarterCosmeticCatalog.Find(
                StarterWardrobeRuntime.LoadLocal().parachuteItemId, CosmeticSlot.Parachute).Color;
            Material canopy = Mat(selectedCanopy);
            Material accent = Mat(new Color(1f, .34f, .015f));
            for (int i = -3; i <= 3; i++)
            {
                GameObject strip = AndroidSafeMesh.CreateBox("CanopyStrip", authoredVisual.transform);
                strip.transform.localPosition = new Vector3(i * .62f, -Mathf.Abs(i) * .13f, 0f);
                strip.transform.localScale = new Vector3(.66f, .16f, 2.3f);
                strip.GetComponent<MeshRenderer>().sharedMaterial = i == 0 ? accent : canopy;
            }
            authoredVisual.SetActive(false);
            GetComponent<ParachuteController>()?.ConfigureVisual(authoredVisual);
        }

        public void Show(bool visible)
        {
            if (authoredVisual != null) authoredVisual.SetActive(visible);
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
