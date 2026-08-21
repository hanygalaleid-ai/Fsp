#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.EditorTools
{
    public sealed class FspBuildValidator : IPreprocessBuildWithReport
    {
        private const string ReleaseApplicationId = "com.hanygalaleid.fsp";
        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            FspProjectBootstrap.EnsureProjectForBuild();

            string[] errors = ValidateProject(report.summary.platform);
            if (errors.Length == 0)
            {
                Debug.Log("BMG pre-build validation passed.");
                return;
            }

            string message = "BMG build blocked:\n- " + string.Join("\n- ", errors);
            throw new BuildFailedException(message);
        }

        [MenuItem("Fsp/Project/Validate MVP Build")]
        public static void ValidateFromMenu()
        {
            FspProjectBootstrap.EnsureProjectForBuild();
            string[] errors = ValidateProject(EditorUserBuildSettings.activeBuildTarget);
            if (errors.Length == 0)
                Debug.Log("BMG MVP validation passed.");
            else
                Debug.LogError("BMG MVP validation failed:\n- " + string.Join("\n- ", errors));
        }

        private static string[] ValidateProject(BuildTarget target)
        {
            var errors = new System.Collections.Generic.List<string>();
            const string lobbyPath = "Assets/Fsp/Scenes/Lobby.unity";
            const string matchPath = "Assets/Fsp/Scenes/Match.unity";

            if (!File.Exists(lobbyPath)) errors.Add("Lobby scene is missing.");
            if (!File.Exists(matchPath)) errors.Add("Match scene is missing.");
            if (!File.Exists("Assets/Fsp/Art/Resources/Shaders/FspMobileSafe.shader"))
                errors.Add("Android mobile-safe shader is missing; release could render magenta materials.");

            // Only hard-require production assets that are known-good and required for gameplay/build.
            // Character/weapon/logo preview JPGs are optional presentation assets and must never block a clean build.
            string[] requiredRuntimeArt =
            {
                "Assets/Fsp/Art/Resources/UI/bmg_app_icon.png",
                "Assets/Fsp/Art/Resources/UI/bmg_adaptive_foreground.png",
                "Assets/Fsp/Art/Resources/UI/bmg_adaptive_background.png",
                "Assets/Fsp/Art/Resources/World/bmg_desert_ground_v3.png",
                "Assets/Fsp/Art/Resources/World/bmg_fortress_wall_v3.png",
                "Assets/Fsp/Art/Resources/World/bmg_wood_floor_v3.png",
                "Assets/Fsp/Art/Resources/World/sunscar_sky_panorama.png",
                "Assets/Fsp/Art/Resources/UI/mobile_joystick.png"
            };
            foreach (string artPath in requiredRuntimeArt)
                if (!File.Exists(artPath)) errors.Add("Required BMG runtime art is missing: " + artPath);

            const string oauthManifest = "Assets/Plugins/Android/FspAuth.androidlib/AndroidManifest.xml";
            if (!File.Exists(oauthManifest) || !File.ReadAllText(oauthManifest).Contains("auth-callback"))
                errors.Add("Google OAuth Android callback manifest is missing or invalid.");
            const string oauthGradle = "Assets/Plugins/Android/FspAuth.androidlib/build.gradle";
            if (!File.Exists(oauthGradle) || !File.ReadAllText(oauthGradle).Contains("namespace \"com.hanygalaleid.fsp.auth\""))
                errors.Add("Google OAuth Android library Gradle namespace is missing or invalid.");

            var enabledScenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (!enabledScenes.Contains(lobbyPath)) errors.Add("Lobby scene is not enabled in Build Settings.");
            if (!enabledScenes.Contains(matchPath)) errors.Add("Match scene is not enabled in Build Settings.");

            if (File.Exists(lobbyPath)) ValidateLobby(lobbyPath, errors);
            if (File.Exists(matchPath)) ValidateMatch(matchPath, errors);

            if (target == BuildTarget.Android && EditorUserBuildSettings.buildAppBundle)
            {
                string id = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
                if (id != ReleaseApplicationId)
                    errors.Add($"Google Play AAB application identifier must be {ReleaseApplicationId}; current value is '{id}'.");

                if (PlayerSettings.Android.targetSdkVersion != AndroidSdkVersions.AndroidApiLevel36)
                    errors.Add("Google Play AAB must target Android 16 / API level 36 in the BMG release pipeline.");

                if (PlayerSettings.Android.bundleVersionCode < 1)
                    errors.Add("Google Play AAB must use a positive Android versionCode.");

                if (!File.Exists(FspAndroidIconSetup.IconPath))
                    errors.Add("Google Play AAB requires the checked-in BMG launcher icon: " + FspAndroidIconSetup.IconPath);
                if (!File.Exists(FspAndroidIconSetup.AdaptiveForegroundPath) || !File.Exists(FspAndroidIconSetup.AdaptiveBackgroundPath))
                    errors.Add("Google Play AAB requires both BMG Android adaptive icon layers.");

                if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) == 0)
                    errors.Add("Google Play AAB must include ARM64.");

                if (PlayerSettings.Android.applicationEntry != AndroidApplicationEntry.Activity)
                    errors.Add("Google OAuth callback requires the single Unity Activity application entry point.");

                if (!PlayerSettings.Android.forceInternetPermission)
                    errors.Add("Google Play AAB must include Android INTERNET permission for Supabase, matchmaking and WebRTC voice.");

                if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) != ScriptingImplementation.IL2CPP)
                    errors.Add("Google Play AAB must use IL2CPP in the BMG release pipeline.");

                if (!PlayerSettings.Android.useCustomKeystore)
                    errors.Add("Google Play AAB must use the configured custom upload keystore.");

                if (string.IsNullOrWhiteSpace(PlayerSettings.Android.keystoreName) || !File.Exists(PlayerSettings.Android.keystoreName))
                    errors.Add("Google Play AAB upload keystore file is missing or invalid.");

                if (string.IsNullOrWhiteSpace(PlayerSettings.Android.keyaliasName))
                    errors.Add("Google Play AAB upload-key alias is missing.");

                if (EditorUserBuildSettings.development)
                    errors.Add("Google Play AAB cannot be a Development Build.");
            }

            return errors.ToArray();
        }

        private static void ValidateLobby(string path, System.Collections.Generic.List<string> errors)
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                if (!FindInScene<Camera>(scene)) errors.Add("Lobby scene has no Camera.");
                // Clean BMG lobby is runtime-authored; no legacy FSP lobby bitmap is required.
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        private static void ValidateMatch(string path, System.Collections.Generic.List<string> errors)
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                if (!FindInScene<Camera>(scene)) errors.Add("Match scene has no Camera.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        private static T FindComponentInScene<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null) return component;
            }
            return null;
        }

        private static bool FindInScene<T>(Scene scene) where T : Component
            => FindComponentInScene<T>(scene) != null;
    }
}
#endif
