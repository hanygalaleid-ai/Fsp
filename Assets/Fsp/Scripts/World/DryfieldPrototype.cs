using UnityEngine;

namespace Fsp.World
{
    /// <summary>
    /// Mobile-safe Dryfield world section used by the minimal checked-in Match scene.
    /// </summary>
    public sealed class DryfieldPrototype : MonoBehaviour
    {
        private void Start() => BuildIfNeeded();

        public void BuildIfNeeded()
        {
            SunscarMissingPoiRuntime.BuildDryfield(transform);
        }
    }
}
