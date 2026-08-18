#if UNITY_EDITOR
using System;
using System.IO;
using Fsp.BattleRoyale;
using Fsp.Lobby;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.EditorTools
{
    [InitializeOnLoad]
    public static class FspProjectBootstrap
    {
        private const string ScenesFolder = "Assets/Fsp/Scenes";
        private const string LobbyScene = ScenesFolder + "/Lobby.unity";
        private const string MatchScene = ScenesFolder + "/Match.unity";
        private const string PrefKey = "Fsp.ProjectBootstrap.v5";
        private static bool initializing;

        static FspProjectBootstrap()
        {
            if (Application.isBatchMode)
                EnsureProject();
            else
                EditorApplication.delayCall += EnsureProject;
        }

        [InitializeOnLoadMethod]
        private static void InitializeAfterDomainReload()
        {
            if (Application.isBatchMode)
                EnsureProject();
        }

        [MenuItem("Fsp/Project/Rebuild Starter Scenes")]
        public static void RebuildStarterScenes()
        {
            EnsureFolder(ScenesFolder);
            CreateLobbyScene(true);
            CreateMatchScene(true);
            ApplyBuildSettings();
            ApplyPlayerDefaults();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Fsp starter scenes rebuilt and added to Build Settings.");
        }

        public static void EnsureProjectForBuild()
        {
            // Release invariant: never overwrite a checked-in scene during a player build.
            // Only create a starter scene when the corresponding file is genuinely missing.
            if (!File.Exists(LobbyScene) || !File.Exists(MatchScene))
            {
                EnsureProject();
                return;
            }

            ApplyBuildSettings();
            ApplyPlayerDefaults();
        }

        private static void EnsureProject()
        {
            if (initializing || EditorApplication.isPlayingOrWillChangePlaymode) return;
            initializing = true;
            try
            {
                EnsureFolder(ScenesFolder);

                // CI/batch mode must preserve the repository scenes exactly as checked in.
                // The explicit Rebuild Starter Scenes menu command is the only path that overwrites them.
                if (!File.Exists(LobbyScene)) CreateLobbyScene(false);
                if (!File.Exists(MatchScene)) CreateMatchScene(false);

                ApplyBuildSettings();
                ApplyPlayerDefaults();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (!File.Exists(LobbyScene) || !File.Exists(MatchScene))
                    throw new InvalidOperationException("Fsp bootstrap could not create the required Lobby/Match scenes.");

                EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
                if (scenes == null || scenes.Length < 2)
                    throw new InvalidOperationException("Fsp bootstrap could not populate EditorBuildSettings scenes.");

                Debug.Log($"Fsp build bootstrap ready without scene overwrite: {scenes.Length} scene(s) configured; Lobby={File.Exists(LobbyScene)}, Match={File.Exists(MatchScene)}.");

                if (!EditorPrefs.GetBool(PrefKey, false))
                {
                    EditorPrefs.SetBool(PrefKey, true);
                    Debug.Log("Fsp project initialized; checked-in scenes are now preserved in batch/CI builds.");
                }
            }
            finally
            {
                initializing = false;
            }
        }

        private static void CreateLobbyScene(bool overwrite)
        {
            if (!overwrite && File.Exists(LobbyScene)) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Lobby";

            CreateCamera(new Vector3(0f, 2.3f, -7.5f), new Vector3(8f, 0f, 0f));
            CreateSun();

            var state = new GameObject("LobbyState");
            state.AddComponent<LobbyState>();

            var controller = new GameObject("LobbyRuntime");
            controller.AddComponent<LobbyController>();
            controller.AddComponent<LobbyMatchLauncher>();
            controller.AddComponent<StarterLobbyUiInstaller>();

            if (!EditorSceneManager.SaveScene(scene, LobbyScene))
                throw new IOException("Failed to save Lobby scene at " + LobbyScene);
        }

        private static void CreateMatchScene(bool overwrite)
        {
            if (!overwrite && File.Exists(MatchScene)) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Match";

            CreateCamera(new Vector3(0f, 8f, -12f), new Vector3(24f, 0f, 0f));
            CreateSun();

            var systems = new GameObject("MatchSystems");
            systems.AddComponent<MatchManager>();
            systems.AddComponent<MatchSceneAssembler>();

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_Base";
            ground.transform.localScale = new Vector3(20f, 1f, 20f);

            if (!EditorSceneManager.SaveScene(scene, MatchScene))
                throw new IOException("Failed to save Match scene at " + MatchScene);
        }

        private static void CreateCamera(Vector3 position, Vector3 euler)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            var camera = go.AddComponent<Camera>();
            camera.fieldOfView = 65f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 2500f;
            go.transform.position = position;
            go.transform.eulerAngles = euler;
            go.AddComponent<AudioListener>();
        }

        private static void CreateSun()
        {
            var go = new GameObject("Sun");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.05f;
            light.color = new Color(1f, 0.86f, 0.68f);
            light.shadows = LightShadows.Soft;
            go.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
        }

        private static void ApplyBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(LobbyScene, true),
                new EditorBuildSettingsScene(MatchScene, true)
            };
        }

        private static void ApplyPlayerDefaults()
        {
            PlayerSettings.companyName = "Fsp Studio";
            PlayerSettings.productName = "Fsp";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.runInBackground = false;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
