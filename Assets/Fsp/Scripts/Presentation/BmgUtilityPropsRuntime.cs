using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>
    /// Compatibility shell. Legacy utility props based on *_mk1 assets are disabled.
    /// Production utility props must be part of the approved production environment kit.
    /// </summary>
    public sealed class BmgUtilityPropsRuntime : MonoBehaviour
    {
        private void Awake()
        {
            enabled = false;
        }
    }
}
