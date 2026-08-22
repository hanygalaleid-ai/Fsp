using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>
    /// Compatibility shell retained for scene/script references.
    /// Legacy *_mk1 authored visuals are intentionally disabled.
    /// Production presentation is owned exclusively by BmgProductionVisualController.
    /// </summary>
    public sealed class BmgAuthoredVisualRuntime : MonoBehaviour
    {
        private void Awake()
        {
            enabled = false;
        }
    }
}
