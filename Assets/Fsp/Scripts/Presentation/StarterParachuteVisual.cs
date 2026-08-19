using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>
    /// Legacy compatibility component. Runtime primitive parachute generation is disabled.
    /// Authored parachute art is authoritative.
    /// </summary>
    public sealed class StarterParachuteVisual : MonoBehaviour
    {
        [SerializeField] private GameObject authoredVisual;

        public void Show(bool visible)
        {
            if (authoredVisual != null) authoredVisual.SetActive(visible);
        }
    }
}
