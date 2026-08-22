using Fsp.BattleRoyale;
using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>Strict BMG parachute presentation. No generated FSP canopy fallback is allowed.</summary>
    public sealed class StarterParachuteVisual : MonoBehaviour
    {
        [SerializeField] private GameObject authoredVisual;
        private const string AuthoredPath = "Models/BMG/bmg_parachute_mk1";

        private void Awake() => Build();

        public void Build()
        {
            if (authoredVisual != null) return;

            GameObject authored = Resources.Load<GameObject>(AuthoredPath);
            if (authored == null)
            {
                Debug.LogError("BMG parachute authored asset is missing. Procedural legacy fallback is disabled.");
                return;
            }

            authoredVisual = Instantiate(authored, transform, false);
            authoredVisual.name = "BMG_Parachute_Authored";
            authoredVisual.transform.localPosition = new Vector3(0f, 4.2f, 0f);
            authoredVisual.transform.localRotation = Quaternion.identity;
            authoredVisual.transform.localScale = Vector3.one;

            // Keep the authored model's own materials/textures. Do not flatten it to one generated color.
            authoredVisual.SetActive(false);
            GetComponent<ParachuteController>()?.ConfigureVisual(authoredVisual);
        }

        public void Show(bool visible)
        {
            if (authoredVisual == null) Build();
            if (authoredVisual != null) authoredVisual.SetActive(visible);
        }
    }
}
