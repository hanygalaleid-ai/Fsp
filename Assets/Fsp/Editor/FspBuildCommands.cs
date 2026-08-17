#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fsp.EditorTools
{
    public static class FspBuildCommands
    {
        private static readonly string[] Scenes =
        {
            "Assets/Fsp/Scenes/Lobby.unity",
            "Assets/Fsp/Scenes/Match.unity"
        };

        [MenuItem("Fsp/Build/Android/Build APK (Test)")]
        public static void BuildAndroidApk()
        {
            PrepareCommonPlayerSettings();
            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            Build(BuildTarget.Android, "Builds/Android/Fsp-test.apk");
        }

        [MenuItem("Fsp/Build/Android/Build AAB (Google Play)")]
        public static void BuildAndroidAab()
        {
            PrepareCommonPlayerSettings();
            EditorUserBuildSettings.buildAppBundle = true;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            Build(BuildTarget.Android, "Builds/Android/Fsp-release.aab");
        }

        [MenuItem("Fsp/Build/Windows/Build x64")]
        public static void BuildWindows()
        {
            PrepareCommonPlayerSettings();
            Build(BuildTarget.StandaloneWindows64, "Builds/Windows/Fsp/Fsp.exe");
        }

        [MenuItem("Fsp/Build/iOS/Export Xcode Project")]
        public static void BuildIos()
        {
            PrepareCommonPlayerSettings();
            Build(BuildTarget.iOS, "Builds/iOS");
        }

        private static void PrepareCommonPlayerSettings()
        {
            PlayerSettings.companyName = "Fsp Studio";
            PlayerSettings.productName = "Fsp";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.runInBackground = false;
        }

        private static void Build(BuildTarget target, string outputPath)
        {
            EnsureOutputDirectory(outputPath, target == BuildTarget.iOS);

            var options = new BuildPlayerOptions
            {
                scenes = Scenes,
                target = target,
                locationPathName = outputPath,
                options = BuildOptions.CompressWithLz4HC
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception($"Fsp build failed: {report.summary.result} ({report.summary.totalErrors} errors)");

            Debug.Log($"Fsp build succeeded: {outputPath} | {report.summary.totalSize / (1024f * 1024f):0.0} MB");
        }

        private static void EnsureOutputDirectory(string outputPath, bool pathIsDirectory)
        {
            string directory = pathIsDirectory ? outputPath : Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        }
    }
}
#endif
