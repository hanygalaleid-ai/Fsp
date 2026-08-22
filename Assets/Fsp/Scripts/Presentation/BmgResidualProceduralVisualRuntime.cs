using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>
    /// Compatibility shell. Legacy residual *_mk1 replacements are disabled.
    /// Gameplay colliders/logic remain in their original systems; production visuals are owned by BmgProductionVisualController.
    /// </summary>
    public sealed class BmgResidualProceduralVisualRuntime : MonoBehaviour
    {
        private void Awake()
        {
            enabled = false;
        }
    }
}
