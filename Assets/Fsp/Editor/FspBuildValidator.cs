#if UNITY_EDITOR
using System;
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
            string[] errors = ValidateProject();
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
            string[] errors = ValidateProject();
            if (errors.Length == 0)
                Debug.Log("Fsp MVP validation passed.");
            else
                Debug.LogError("Fsp MVP validation failed:\n- " + string.Join("\n- ", errors));
        }

        private static string[] ValidateProject()
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
