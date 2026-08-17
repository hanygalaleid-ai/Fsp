#if UNITY_EDITOR
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
        private const string PrefKey = "Fsp.ProjectBootstrap.v2";

        static FspProjectBootstrap()
        {
            EditorApplication.delayCall += EnsureProject;
        }

        [MenuItem("Fsp/Project/Rebuild Starter Scenes")]
        public static void RebuildStarterScenes()
        {
            EnsureFolder(ScenesFolder);
            CreateLobbyScene(true);
            CreateMatchScene(true);
            ApplyBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Fsp starter scenes rebuilt.");
        }

        private static void EnsureProject()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            EnsureFolder(ScenesFolder);
            if (!File.Exists(LobbyScene)) CreateLobbyScene(false);
            if (!File.Exists(MatchScene)) CreateMatchScene(false);
            ApplyBuildSettings();
            ApplyPlayerDefaults();

            if (!EditorPrefs.GetBool(PrefKey, false))
            {
                EditorPrefs.SetBool(PrefKey, true);
                Debug.Log("Fsp Unity project initialized for Android, iOS and Windows. Starter Lobby and Match scenes are ready.");
            }
        }

        private static void CreateLobbyScene(bool overwrite)
        {
            if (!overwrite && File.Exists(LobbyScene)) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Lobby";

            CreateCamera(new Vector3(0f, 2.2f, -7f), new Vector3(10f, 0f, 0f));
            CreateSun();

            var state = new GameObject("LobbyState");
            state.AddComponent<LobbyState>();

            var controller = new GameObject("LobbyController");
            controller.AddComponent<LobbyController>();

            var stage = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stage.name = "CharacterStage_Placeholder";
            stage.transform.position = Vector3.zero;
            stage.transform.localScale = new Vector3(1.7f, 0.12f, 1.7f);

            EditorSceneManager.SaveScene(scene, LobbyScene);
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
            ground.name = "Ground_Placeholder";
            ground.transform.localScale = new Vector3(20f, 1f, 20f);

            EditorSceneManager.SaveScene(scene, MatchScene);
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
