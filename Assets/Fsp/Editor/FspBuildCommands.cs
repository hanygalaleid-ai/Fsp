#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fsp.EditorTools
{
    public static class FspBuildCommands
    {
        private const string AndroidApplicationId = "com.hanygalaleid.fsp";

        private static readonly string[] Scenes =
        {
            "Assets/Fsp/Scenes/Lobby.unity",
            "Assets/Fsp/Scenes/Match.unity"
        };

        [MenuItem("Fsp/Build/Android/Build APK (Test)")]
        public static void BuildAndroidApk()
        {
            FspProjectBootstrap.EnsureProjectForBuild();
            FspProjectValidator.ValidateOrThrow();
            PrepareCommonPlayerSettings();
            PrepareAndroidSettings(false);
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.development = false;
            Build(BuildTarget.Android, "Builds/Android/Fsp-test.apk", requireZeroWarnings: false);
        }

        [MenuItem("Fsp/Build/Android/Build AAB (Google Play)")]
        public static void BuildAndroidAab()
        {
            FspProjectBootstrap.EnsureProjectForBuild();
            FspProjectValidator.ValidateOrThrow();
            PrepareCommonPlayerSettings();
            PrepareAndroidSettings(true);
            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.development = false;
            Build(BuildTarget.Android, "Builds/Android/Fsp-release.aab", requireZeroWarnings: true);
        }

        [MenuItem("Fsp/Build/Windows/Build x64")]
        public static void BuildWindows()
        {
            FspProjectBootstrap.EnsureProjectForBuild();
            FspProjectValidator.ValidateOrThrow();
            PrepareCommonPlayerSettings();
            Build(BuildTarget.StandaloneWindows64, "Builds/Windows/Fsp/Fsp.exe", requireZeroWarnings: false);
        }

        [MenuItem("Fsp/Build/iOS/Export Xcode Project")]
        public static void BuildIos()
        {
            FspProjectBootstrap.EnsureProjectForBuild();
            FspProjectValidator.ValidateOrThrow();
            PrepareCommonPlayerSettings();
            Build(BuildTarget.iOS, "Builds/iOS", requireZeroWarnings: false);
        }

        private static void PrepareCommonPlayerSettings()
        {
            PlayerSettings.companyName = "Fsp Studio";
            PlayerSettings.productName = "Fsp";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.runInBackground = false;
        }

        private static void PrepareAndroidSettings(bool release)
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AndroidApplicationId);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            FspAndroidIconSetup.Apply();

            if (!release) return;

            EditorUserBuildSettings.development = false;
            ConfigureReleaseSigning();
        }

        private static void ConfigureReleaseSigning()
        {
            string keystorePath = Environment.GetEnvironmentVariable("FSP_ANDROID_KEYSTORE_PATH");
            string keystorePassword = Environment.GetEnvironmentVariable("FSP_ANDROID_KEYSTORE_PASSWORD");
            string aliasName = Environment.GetEnvironmentVariable("FSP_ANDROID_KEYALIAS_NAME");
            string aliasPassword = Environment.GetEnvironmentVariable("FSP_ANDROID_KEYALIAS_PASSWORD");

            if (string.IsNullOrWhiteSpace(keystorePath) || !File.Exists(keystorePath))
                throw new BuildFailedException("FSP release signing is not configured: FSP_ANDROID_KEYSTORE_PATH is missing or invalid.");
            if (string.IsNullOrWhiteSpace(keystorePassword))
                throw new BuildFailedException("FSP release signing is not configured: FSP_ANDROID_KEYSTORE_PASSWORD is missing.");
            if (string.IsNullOrWhiteSpace(aliasName))
                throw new BuildFailedException("FSP release signing is not configured: FSP_ANDROID_KEYALIAS_NAME is missing.");
            if (string.IsNullOrWhiteSpace(aliasPassword))
                throw new BuildFailedException("FSP release signing is not configured: FSP_ANDROID_KEYALIAS_PASSWORD is missing.");

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = Path.GetFullPath(keystorePath);
            PlayerSettings.Android.keystorePass = keystorePassword;
            PlayerSettings.Android.keyaliasName = aliasName;
            PlayerSettings.Android.keyaliasPass = aliasPassword;
        }

        private static void Build(BuildTarget target, string outputPath, bool requireZeroWarnings)
        {
            foreach (string scene in Scenes)
            {
                if (!File.Exists(scene))
                    throw new BuildFailedException("Required build scene is missing: " + scene);
            }

            EnsureOutputDirectory(outputPath, target == BuildTarget.iOS);

            var options = new BuildPlayerOptions
            {
                scenes = Scenes,
                target = target,
                targetGroup = BuildPipeline.GetBuildTargetGroup(target),
                locationPathName = outputPath,
                options = BuildOptions.CompressWithLz4HC
            };

            Debug.Log($"Fsp build starting: target={target}, output={outputPath}, scenes={string.Join(", ", Scenes)}");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"Fsp build failed: {report.summary.result} ({report.summary.totalErrors} errors, {report.summary.totalWarnings} warnings)");

            if (requireZeroWarnings && report.summary.totalWarnings > 0)
                throw new BuildFailedException($"Fsp release build rejected: {report.summary.totalWarnings} warning(s) remain. Google Play AAB release requires 0 warnings.");

            Debug.Log($"Fsp build succeeded: {outputPath} | {report.summary.totalSize / (1024f * 1024f):0.0} MB | {report.summary.totalWarnings} warning(s)");
        }

        private static void EnsureOutputDirectory(string outputPath, bool pathIsDirectory)
        {
            string directory = pathIsDirectory ? outputPath : Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        }
    }
}
#endif
