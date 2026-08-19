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
            "Assets/Fsp/Art/Resources/Lobby/fsp_lobby_final.jpg",
            "Assets/Fsp/Art/Resources/UI/ui_panel_dark.png",
            "Assets/Fsp/Art/Resources/UI/ui_button_primary.png",
            "Assets/Fsp/Art/Resources/World/sand_ground.png",
            "Assets/Fsp/Art/Resources/World/rock_cliff.png",
            "Assets/Fsp/Art/Resources/World/road_dust.png",
            "Assets/Fsp/Art/Resources/World/fortress_wall.png"
        };

        public void OnPreprocessBuild(BuildReport report)
        {
            // Release builds must use checked-in production art only.
            // No placeholder/procedural art generation is allowed here.
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            foreach (string path in RequiredArt)
            {
                if (!File.Exists(path))
                    throw new BuildFailedException("Required production FSP art is missing: " + path);

                long bytes = new FileInfo(path).Length;
                if (bytes < 256)
                    throw new BuildFailedException("Required production FSP art looks invalid or empty: " + path);

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture == null || texture.width <= 0 || texture.height <= 0)
                    throw new BuildFailedException("Unity failed to import required production FSP texture: " + path);

                Debug.Log($"FSP PRODUCTION ART OK: {path} ({texture.width}x{texture.height}, {bytes / 1024f:0.0} KB)");
            }

            if (report.summary.platform == BuildTarget.Android)
                FspAndroidIconSetup.Apply();

            AssetDatabase.SaveAssets();
            Debug.Log("FSP PRODUCTION ART GATE PASSED: fixed checked-in art preserved; no art generation executed.");
        }
    }
}
#endif
