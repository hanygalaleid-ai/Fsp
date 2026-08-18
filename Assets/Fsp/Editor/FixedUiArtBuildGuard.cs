#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fsp.EditorTools
{
    public sealed class FixedUiArtBuildGuard : IPreprocessBuildWithReport
    {
        public int callbackOrder => -950;

        private static readonly string[] RequiredArt =
        {
            "Assets/Fsp/Art/Resources/Lobby/lobby_reference.jpg",
            "Assets/Fsp/Art/Resources/UI/joystick_base.png",
            "Assets/Fsp/Art/Resources/UI/ui_panel_dark.png",
            "Assets/Fsp/Art/Resources/UI/ui_button_primary.png",
            "Assets/Fsp/Art/Resources/UI/ui_button_secondary.png",
            "Assets/Fsp/Art/Resources/UI/action_icons.png",
            "Assets/Fsp/Art/Resources/World/sand_ground.png",
            "Assets/Fsp/Art/Resources/World/rock_cliff.png",
            "Assets/Fsp/Art/Resources/World/road_dust.png",
            "Assets/Fsp/Art/Resources/World/fortress_wall.png"
        };

        public void OnPreprocessBuild(BuildReport report)
        {
            // Cloud Build checks the repository out into a clean workspace. These image files are
            // intentionally runtime Resources, so never allow an APK/AAB to continue until Unity has
            // imported every required asset synchronously. This prevents a build that contains the
            // scripts but silently misses the Lobby/UI/World textures at runtime.
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            foreach (string path in RequiredArt)
            {
                if (!File.Exists(path))
                    throw new BuildFailedException("Required fixed FSP art is missing: " + path);

                long bytes = new FileInfo(path).Length;
                if (bytes < 256)
                    throw new BuildFailedException("Required fixed FSP art looks invalid or empty: " + path);

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture == null || texture.width <= 0 || texture.height <= 0)
                    throw new BuildFailedException("Unity failed to import required FSP texture before build: " + path);

                Debug.Log($"FSP ART IMPORT OK: {path} ({texture.width}x{texture.height}, {bytes / 1024f:0.0} KB)");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("FSP ART IMPORT GATE PASSED: all required Lobby/UI/World textures are imported and build-visible.");
        }
    }
}
#endif
