using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>
    /// Legacy compatibility component. Runtime primitive transport-plane generation is disabled.
    /// The authored plane prefab/mesh is authoritative.
    /// </summary>
    public sealed class StarterPlaneVisual : MonoBehaviour
    {
        private void Awake()
        {
            // Intentionally empty. Do not hide or replace the authored renderer.
        }
    }
}
