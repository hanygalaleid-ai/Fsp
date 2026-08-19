#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fsp.BattleRoyale;
using Fsp.UI;

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
                Debug.Log("Fsp pre-build validation passed.");
                return;
            }

            string message = "Fsp build blocked:\n- " + string.Join("\n- ", errors);
            throw new BuildFailedException(message);
        }

        [MenuItem("Fsp/Project/Validate MVP Build")]
        public static void ValidateFromMenu()
        {
            FspProjectBootstrap.EnsureProjectForBuild();
            string[] errors = ValidateProject(EditorUserBuildSettings.activeBuildTarget);
            if (errors.Length == 0)
                Debug.Log("Fsp MVP validation passed.");
            else
                Debug.LogError("Fsp MVP validation failed:\n- " + string.Join("\n- ", errors));
        }

        private static string[] ValidateProject(BuildTarget target)
        {
            var errors = new System.Collections.Generic.List<string>();
            const string lobbyPath = "Assets/Fsp/Scenes/Lobby.unity";
            const string matchPath = "Assets/Fsp/Scenes/Match.unity";

            if (!File.Exists(lobbyPath)) errors.Add("Lobby scene is missing.");
            if (!File.Exists(matchPath)) errors.Add("Match scene is missing.");

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
                    errors.Add("Google Play AAB must target Android 16 / API level 36 in the Fsp release pipeline.");

                if (PlayerSettings.Android.bundleVersionCode < 1)
                    errors.Add("Google Play AAB must use a positive Android versionCode.");

                if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) == 0)
                    errors.Add("Google Play AAB must include ARM64.");

                if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) != ScriptingImplementation.IL2CPP)
                    errors.Add("Google Play AAB must use IL2CPP in the Fsp release pipeline.");

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

                GameObject art = FindNamedRoot(scene, "FSP_FIXED_LOBBY_ART");
                if (art == null)
                    errors.Add("Lobby scene is missing the fixed approved lobby artwork root FSP_FIXED_LOBBY_ART.");
                else
                {
                    SpriteRenderer renderer = art.GetComponent<SpriteRenderer>();
                    if (renderer == null || renderer.sprite == null)
                        errors.Add("Fixed lobby artwork root has no valid SpriteRenderer/sprite.");
                }
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
                if (!FindInScene<MatchManager>(scene)) errors.Add("Match scene has no authored MatchManager.");
                if (!FindInScene<MatchSceneAssembler>(scene)) errors.Add("Match scene has no authored MatchSceneAssembler.");
                if (!FindLocalParticipant(scene)) errors.Add("Match scene has no authored local MatchParticipant. Runtime player generation is disabled.");
                if (!FindInScene<BattleRoyaleHud>(scene)) errors.Add("Match scene has no authored BattleRoyaleHud. Runtime HUD generation is disabled.");

                MatchHudBinding hudBinding = FindComponentInScene<MatchHudBinding>(scene);
                if (hudBinding == null)
                    errors.Add("Match scene has no MatchHudBinding for the approved Sunscar HUD layout.");
                else if (!hudBinding.IsComplete)
                    errors.Add("MatchHudBinding is incomplete. Assign compass, minimap, joystick, action buttons, weapon panel, bars, labels and buttons in Match.unity.");

                if (CountInScene<Renderer>(scene) == 0) errors.Add("Match scene contains no authored renderers/world art.");
                if (CountInScene<Collider>(scene) == 0) errors.Add("Match scene contains no authored collision surfaces.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        private static bool FindLocalParticipant(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MatchParticipant participant in root.GetComponentsInChildren<MatchParticipant>(true))
                    if (participant != null && participant.IsLocalPlayer) return true;
            }
            return false;
        }

        private static int CountInScene<T>(Scene scene) where T : Component
        {
            int count = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
                count += root.GetComponentsInChildren<T>(true).Length;
            return count;
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

        private static GameObject FindNamedRoot(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == name) return root;
            return null;
        }
    }
}
#endif
