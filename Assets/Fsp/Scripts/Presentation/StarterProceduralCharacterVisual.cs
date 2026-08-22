using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>
    /// Compatibility-only component. Procedural character mesh generation is permanently disabled.
    /// BmgAuthoredVisualRuntime supplies the approved authored BMG character assets.
    /// </summary>
    public sealed class StarterProceduralCharacterVisual : MonoBehaviour
    {
        public void Build() { }
        public void ApplyCharacterIdentity(string characterId) { }
        public void ApplyCosmeticLoadout(CosmeticLoadout loadout) { }
    }
}
