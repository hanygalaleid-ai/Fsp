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
using Fsp.Lobby;

namespace Fsp.EditorTools
{
    public sealed class FspBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
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

            if (!File.Exists(lobbyPath)) errors.Add("Lobby scene is missing. Use Fsp > Project > Rebuild Starter Scenes.");
            if (!File.Exists(matchPath)) errors.Add("Match scene is missing. Use Fsp > Project > Rebuild Starter Scenes.");

            var enabledScenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (!enabledScenes.Contains(lobbyPath)) errors.Add("Lobby scene is not enabled in Build Settings.");
            if (!enabledScenes.Contains(matchPath)) errors.Add("Match scene is not enabled in Build Settings.");

            if (File.Exists(lobbyPath)) ValidateLobby(lobbyPath, errors);
            if (File.Exists(matchPath)) ValidateMatch(matchPath, errors);

            if (target == BuildTarget.Android && EditorUserBuildSettings.buildAppBundle)
            {
                string id = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
                if (string.IsNullOrWhiteSpace(id) || id.Contains("DefaultCompany") || id == "com.FspStudio.Fsp")
                    errors.Add("Set a unique Android Application Identifier before Google Play AAB release.");

                if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) == 0)
                    errors.Add("Google Play AAB must include ARM64.");

                if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) != ScriptingImplementation.IL2CPP)
                    errors.Add("Google Play AAB must use IL2CPP in the Fsp release pipeline.");

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
                if (!FindInScene<LobbyState>(scene)) errors.Add("Lobby scene has no LobbyState.");
                if (!FindInScene<Camera>(scene)) errors.Add("Lobby scene has no Camera.");
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
                if (!FindInScene<MatchManager>(scene)) errors.Add("Match scene has no MatchManager.");
                if (!FindInScene<MatchSceneAssembler>(scene)) errors.Add("Match scene has no MatchSceneAssembler.");
                if (!FindInScene<Camera>(scene)) errors.Add("Match scene has no Camera.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        private static bool FindInScene<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.GetComponentInChildren<T>(true) != null) return true;
            return false;
        }
    }
}
#endif
