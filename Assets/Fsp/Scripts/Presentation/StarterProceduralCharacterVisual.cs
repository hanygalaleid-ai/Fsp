using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>
    /// Legacy compatibility component. Procedural placeholder character generation is disabled.
    /// Authored character meshes/prefabs in the scene are authoritative.
    /// </summary>
    public sealed class StarterProceduralCharacterVisual : MonoBehaviour
    {
        public void Build()
        {
            // Intentionally empty. Never replace authored character art at runtime.
        }
    }
}
