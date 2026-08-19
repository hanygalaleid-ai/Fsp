using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>
    /// Legacy compatibility component. Runtime primitive vehicle generation is disabled.
    /// Authored vehicle prefabs and meshes are authoritative.
    /// </summary>
    public sealed class StarterProceduralVehicleVisual : MonoBehaviour
    {
        public void Build()
        {
            // Intentionally empty. Never replace authored vehicle art at runtime.
        }
    }
}
