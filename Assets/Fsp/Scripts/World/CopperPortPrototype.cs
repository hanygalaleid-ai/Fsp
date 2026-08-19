using UnityEngine;

namespace Fsp.World
{
    /// <summary>
    /// Legacy compatibility component. Runtime Copper Port prototype generation is disabled.
    /// Authored Copper Port scene geometry and loot placement are authoritative.
    /// </summary>
    public sealed class CopperPortPrototype : MonoBehaviour
    {
        public void BuildIfNeeded()
        {
            // Intentionally empty. Never generate primitive world art in release builds.
        }
    }
}
