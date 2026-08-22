using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>
    /// Compatibility shell. Legacy vehicle light meshes based on *_mk1 assets are disabled.
    /// Production vehicle lighting/materials must ship with the approved production vehicle model.
    /// </summary>
    public sealed class BmgVehicleLightingRuntime : MonoBehaviour
    {
        private void Awake()
        {
            enabled = false;
        }
    }
}
