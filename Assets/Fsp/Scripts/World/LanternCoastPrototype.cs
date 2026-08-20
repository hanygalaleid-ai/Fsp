using UnityEngine;

namespace Fsp.World
{
    /// <summary>
    /// Mobile-safe Lantern Coast world section used by the minimal checked-in Match scene.
    /// </summary>
    public sealed class LanternCoastPrototype : MonoBehaviour
    {
        private void Start() => BuildIfNeeded();

        public void BuildIfNeeded()
        {
            SunscarMissingPoiRuntime.BuildLanternCoast(transform);
        }
    }
}
