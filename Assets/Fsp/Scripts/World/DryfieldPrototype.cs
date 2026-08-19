using UnityEngine;

namespace Fsp.World
{
    /// <summary>
    /// Legacy compatibility component. Runtime Dryfield prototype generation is disabled.
    /// Authored Dryfield scene geometry and loot placement are authoritative.
    /// </summary>
    public sealed class DryfieldPrototype : MonoBehaviour
    {
        public void BuildIfNeeded()
        {
            // Intentionally empty. Never generate primitive world art in release builds.
        }
    }
}
