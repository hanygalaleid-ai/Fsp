#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fsp.EditorTools
{
    /// <summary>
    /// Compatibility guard retained for older build configurations.
    /// The previous guard incorrectly treated *_mk1 prototype meshes as release-quality authored 3D.
    /// Production release validation is now owned by BmgProductionVisualReleaseGuard.
    /// </summary>
    public sealed class BmgAuthored3DAssetBuildGuard : IPreprocessBuildWithReport
    {
        public int callbackOrder => -850;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("BMG authored-3D compatibility guard: mk1 prototype assets are NOT considered production art. Strict validation is handled by BmgProductionVisualReleaseGuard.");
        }
    }
}
#endif
