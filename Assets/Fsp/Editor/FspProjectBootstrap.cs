#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Fsp.EditorTools
{
    [InitializeOnLoad]
    public static class FspProjectBootstrap
    {
        private const string LobbyScene = "Assets/Fsp/Scenes/Lobby.unity";
        private const string MatchScene = "Assets/Fsp/Scenes/Match.unity";
        private static bool validating;

        static FspProjectBootstrap()
        {
            if (Application.isBatchMode)
                EditorApplication.delayCall += ValidateProjectForRelease;
        }

        [MenuItem("Fsp/Project/Validate Fixed Release Scenes")]
        public static void ValidateProjectForRelease()
        {
            if (validating) return;
            validating = true;
            try
            {
                RequireScene(LobbyScene, "Lobby");
                RequireScene(MatchScene, "Match");

                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(LobbyScene, true),
                    new EditorBuildSettingsScene(MatchScene, true)
                };

                ApplyPlayerDefaults();
                Debug.Log("FSP RELEASE SCENES VALIDATED: checked-in Lobby and Match scenes are the only build source; scene generation is disabled.");
            }
            finally
            {
                validating = false;
            }
        }

        public static void EnsureProjectForBuild()
        {
            ValidateProjectForRelease();
        }

        private static void RequireScene(string path, string sceneName)
        {
            if (!File.Exists(path))
            {
                throw new InvalidOperationException(
                    $"FSP RELEASE BLOCKED: required checked-in {sceneName} scene is missing at '{path}'. " +
                    "Placeholder/procedural scene generation is intentionally disabled. Restore the real release scene before building.");
            }
        }

        private static void ApplyPlayerDefaults()
        {
            PlayerSettings.companyName = "Fsp Studio";
            PlayerSettings.productName = "Fsp";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.runInBackground = false;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
    }
}
#endif
