using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>
    /// Compatibility shell. Legacy POI landmark replacements based on *_mk1 assets are disabled.
    /// The production environment kit is owned by BmgProductionVisualController.
    /// </summary>
    public sealed class BmgPoiLandmarkVisualRuntime : MonoBehaviour
    {
        private void Awake()
        {
            enabled = false;
        }
    }
}
