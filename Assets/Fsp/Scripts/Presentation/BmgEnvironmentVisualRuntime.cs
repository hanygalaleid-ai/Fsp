using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>
    /// Compatibility shell. The previous environment runtime loaded *_mk1 models and is disabled.
    /// BmgProductionVisualController is the only production environment visual authority.
    /// </summary>
    public sealed class BmgEnvironmentVisualRuntime : MonoBehaviour
    {
        private void Awake()
        {
            enabled = false;
        }
    }
}
