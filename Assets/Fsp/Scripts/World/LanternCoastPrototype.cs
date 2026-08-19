using UnityEngine;

namespace Fsp.World
{
    /// <summary>
    /// Legacy compatibility component. Runtime Lantern Coast prototype generation is disabled.
    /// Authored Lantern Coast scene geometry and loot placement are authoritative.
    /// </summary>
    public sealed class LanternCoastPrototype : MonoBehaviour
    {
        public void BuildIfNeeded()
        {
            // Intentionally empty. Never generate primitive world art in release builds.
        }
    }
}
