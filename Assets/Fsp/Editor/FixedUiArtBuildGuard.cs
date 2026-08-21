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

        private const string AndroidApplicationId = "com.hanygalaleid.fsp";

        private static readonly string[] RequiredArt =
        {
            "Assets/Fsp/Art/Resources/Lobby/fsp_lobby_final.jpg",
            "Assets/Fsp/Art/Resources/BMG/Brand/bmg_logo.jpg",
            "Assets/Fsp/Art/Resources/BMG/Atlases/bmg_characters_atlas.jpg",
            "Assets/Fsp/Art/Resources/BMG/Atlases/bmg_weapons_atlas.jpg",
            "Assets/Fsp/Art/Resources/BMG/UI/bmg_menu_icons_3d.jpg",
            "Assets/Fsp/Art/Resources/BMG/UI/bmg_action_icons_3d.jpg",
            "Assets/Fsp/Art/Resources/UI/bmg_app_icon.png",
            "Assets/Fsp/Art/Resources/UI/bmg_adaptive_foreground.png",
            "Assets/Fsp/Art/Resources/UI/bmg_adaptive_background.png",
            "Assets/Fsp/Art/Resources/UI/google_signin_square.png",
            "Assets/Fsp/Art/Resources/UI/mobile_joystick.png",
            "Assets/Fsp/Art/Resources/UI/language_icons.png",
            "Assets/Fsp/Art/Resources/World/bmg_desert_ground_v3.png",
            "Assets/Fsp/Art/Resources/World/bmg_fortress_wall_v3.png",
            "Assets/Fsp/Art/Resources/World/bmg_wood_floor_v3.png",
            "Assets/Fsp/Art/Resources/World/sunscar_sky_panorama.png"
        };

        public void OnPreprocessBuild(BuildReport report)
        {
            ApplyDeterministicPlayerSettings(report.summary.platform);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            foreach (string path in RequiredArt)
            {
                if (!File.Exists(path))
                    throw new BuildFailedException("Required BMG production art is missing: " + path);

                long bytes = new FileInfo(path).Length;
                if (bytes < 256)
                    throw new BuildFailedException("Required BMG production art looks invalid or empty: " + path);

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture == null || texture.width <= 0 || texture.height <= 0)
                    throw new BuildFailedException("Unity failed to import required BMG production texture: " + path);

                Debug.Log($"BMG PRODUCTION ART OK: {path} ({texture.width}x{texture.height}, {bytes / 1024f:0.0} KB)");
            }

            if (report.summary.platform == BuildTarget.Android)
                FspAndroidIconSetup.Apply();

            AssetDatabase.SaveAssets();
            Debug.Log("BMG PRODUCTION ART GATE PASSED: approved realistic UI, branding and world art imported successfully.");
        }

        private static void ApplyDeterministicPlayerSettings(BuildTarget platform)
        {
            PlayerSettings.companyName = "BMG Studio";
            PlayerSettings.productName = "BMG";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.runInBackground = false;

            if (platform != BuildTarget.Android) return;

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AndroidApplicationId);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

            Debug.Log("BMG CLOUD SETTINGS OK: Android ARM64/IL2CPP, landscape fullscreen, package " + AndroidApplicationId);
        }
    }
}
#endif
