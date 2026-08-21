#if UNITY_EDITOR
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Fsp.EditorTools
{
    /// <summary>Stops Android/desktop builds if the approved realistic BMG menu artwork is missing.</summary>
    public sealed class BmgRealisticArtBuildGuard : IPreprocessBuildWithReport
    {
        private static readonly string[] Required =
        {
            "Assets/Fsp/Art/Resources/UI/bmg_app_icon.png",
            "Assets/Fsp/Art/Resources/BMG/Atlases/bmg_characters_atlas.jpg",
            "Assets/Fsp/Art/Resources/BMG/Atlases/bmg_weapons_atlas.jpg",
            "Assets/Fsp/Scripts/Presentation/BmgRealisticArtRuntime.cs",
            "Assets/Fsp/Scripts/Presentation/BmgProductionUiSkinRuntime.cs"
        };

        public int callbackOrder => -790;

        public void OnPreprocessBuild(BuildReport report)
        {
            foreach (string path in Required)
                if (!File.Exists(path))
                    throw new BuildFailedException("BMG realistic production art is missing: " + path);
        }
    }
}
#endif
