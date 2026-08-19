using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>
    /// Legacy compatibility component. Runtime primitive weapon generation is disabled.
    /// Authored weapon prefabs and meshes are authoritative.
    /// </summary>
    public sealed class StarterProceduralWeaponVisual : MonoBehaviour
    {
        public void Build()
        {
            // Intentionally empty. Never replace authored weapon art at runtime.
        }
    }
}
