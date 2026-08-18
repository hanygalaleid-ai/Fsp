#if UNITY_EDITOR
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Fsp.EditorTools
{
    public sealed class FixedUiArtBuildGuard : IPreprocessBuildWithReport
    {
        public int callbackOrder => -900;

        private static readonly string[] RequiredArt =
        {
            "Assets/Fsp/Art/Resources/Lobby/lobby_reference.jpg",
            "Assets/Fsp/Art/Resources/UI/joystick_base.png",
            "Assets/Fsp/Art/Resources/UI/ui_panel_dark.png",
            "Assets/Fsp/Art/Resources/UI/ui_button_primary.png",
            "Assets/Fsp/Art/Resources/UI/ui_button_secondary.png",
            "Assets/Fsp/Art/Resources/World/sand_ground.png",
            "Assets/Fsp/Art/Resources/World/rock_cliff.png",
            "Assets/Fsp/Art/Resources/World/road_dust.png",
            "Assets/Fsp/Art/Resources/World/fortress_wall.png"
        };

        public void OnPreprocessBuild(BuildReport report)
        {
            foreach (string path in RequiredArt)
            {
                if (!File.Exists(path))
                    throw new BuildFailedException("Required fixed FSP art is missing: " + path);

                long bytes = new FileInfo(path).Length;
                if (bytes < 256)
                    throw new BuildFailedException("Required fixed FSP art looks invalid or empty: " + path);
            }
        }
    }
}
#endif
