#if UNITY_EDITOR
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Fsp.EditorTools
{
    /// <summary>Stops builds only when the current clean BMG runtime presentation assets are missing.</summary>
    public sealed class BmgRealisticArtBuildGuard : IPreprocessBuildWithReport
    {
        private static readonly string[] Required =
        {
            "Assets/Fsp/Art/Resources/UI/bmg_app_icon.png",
            "Assets/Fsp/Scripts/Presentation/BmgRealisticArtRuntime.cs",
            "Assets/Fsp/Scripts/Presentation/BmgProductionUiSkinRuntime.cs",
            "Assets/Fsp/Scripts/Presentation/BmgCleanLobbyBackgroundRuntime.cs"
        };

        public int callbackOrder => -790;

        public void OnPreprocessBuild(BuildReport report)
        {
            foreach (string path in Required)
                if (!File.Exists(path))
                    throw new BuildFailedException("Required clean BMG presentation asset is missing: " + path);
        }
    }
}
#endif
