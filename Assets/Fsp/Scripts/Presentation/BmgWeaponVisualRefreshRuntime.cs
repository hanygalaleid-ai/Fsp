using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>
    /// Compatibility shell. Legacy weapon refresh based on *_mk1 assets is disabled.
    /// Production weapons are synchronized by BmgProductionVisualController.
    /// </summary>
    public sealed class BmgWeaponVisualRefreshRuntime : MonoBehaviour
    {
        private void Awake()
        {
            enabled = false;
        }
    }
}
