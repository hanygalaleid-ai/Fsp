using UnityEngine;

namespace Fsp.World
{
    /// <summary>
    /// Mobile-safe Copper Port world section used by the minimal checked-in Match scene.
    /// </summary>
    public sealed class CopperPortPrototype : MonoBehaviour
    {
        private void Start() => BuildIfNeeded();

        public void BuildIfNeeded()
        {
            SunscarMissingPoiRuntime.BuildCopperPort(transform);
        }
    }
}
