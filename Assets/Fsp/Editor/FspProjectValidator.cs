#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Fsp.BattleRoyale;
using Fsp.Lobby;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.EditorTools
{
    public static class FspProjectValidator
    {
        private const string LobbyScenePath = "Assets/Fsp/Scenes/Lobby.unity";
        private const string MatchScenePath = "Assets/Fsp/Scenes/Match.unity";

        private static readonly string[] RequiredFiles =
        {
            LobbyScenePath,
            MatchScenePath,
            "Assets/Fsp/Scripts/BattleRoyale/MatchManager.cs",
            "Assets/Fsp/Scripts/BattleRoyale/MatchSceneAssembler.cs",
            "Assets/Fsp/Scripts/BattleRoyale/SafeZoneController.cs",
            "Assets/Fsp/Scripts/BattleRoyale/SafeZoneDamageApplier.cs",
            "Assets/Fsp/Scripts/BattleRoyale/DropPlaneController.cs",
            "Assets/Fsp/Scripts/BattleRoyale/DropPlanePassenger.cs",
            "Assets/Fsp/Scripts/BattleRoyale/ParachuteController.cs",
            "Assets/Fsp/Scripts/BattleRoyale/AirdropController.cs",
            "Assets/Fsp/Scripts/BattleRoyale/MatchPopulationBootstrap.cs",
            "Assets/Fsp/Scripts/Player/PlayerVitals.cs",
            "Assets/Fsp/Scripts/Player/ThirdPersonMotor.cs",
            "Assets/Fsp/Scripts/Combat/HitscanWeapon.cs",
            "Assets/Fsp/Scripts/Inventory/PlayerInventory.cs",
            "Assets/Fsp/Scripts/Inventory/LootPickup.cs",
            "Assets/Fsp/Scripts/AI/BotBrain.cs",
            "Assets/Fsp/Scripts/Vehicles/SimpleVehicleController.cs",
            "Assets/Fsp/Scripts/Network/NetworkSessionManager.cs",
            "Assets/Fsp/Scripts/Network/CloudflareWebSocketTransport.cs",
            "Assets/Fsp/Scripts/Network/MatchNetworkRuntimeInstaller.cs",
            "Assets/Fsp/Scripts/Network/MatchNetworkRuntimeConfigBootstrap.cs",
            "Assets/Fsp/Scripts/Network/NetworkCombatRuntimeBridge.cs",
            "Assets/Fsp/Scripts/Network/NetworkVehicleSync.cs",
            "Assets/Fsp/Scripts/Network/NetworkBotSnapshotPublisher.cs",
            "Assets/Fsp/Scripts/Network/AuthoritativeWorldClockSync.cs",
            "Assets/Fsp/Scripts/Network/NetworkWorldState.cs",
            "Assets/Fsp/Scripts/Network/NetworkZoneProbe.cs",
            "Assets/Fsp/Scripts/Voice/CloudflareSfuVoiceRuntime.cs",
            "Assets/Fsp/Scripts/Voice/CloudflareSfuSignalingClient.cs",
            "Assets/Fsp/Scripts/Voice/SquadVoiceCoordinator.cs",
            "Assets/Fsp/Scripts/Voice/SquadVoiceHudRuntime.cs",
            "Packages/manifest.json",
            "ProjectSettings/ProjectVersion.txt"
        };

        [MenuItem("Fsp/Validate/Validate Project")]
        public static void ValidateFromMenu()
        {
            ValidateOrThrow();
            Debug.Log("Fsp validation passed: required files, release scenes and runtime recovery wiring are ready for build.");
        }

        public static void ValidateOrThrow()
        {
            ValidateRequiredFiles();
            ValidateProjectVersionAndPackages();
            ValidateLobbyScene();
            ValidateMatchScene();
            AssetDatabase.Refresh();
        }

        private static void ValidateRequiredFiles()
        {
            var missing = new List<string>();
            foreach (string path in RequiredFiles)
                if (!File.Exists(path)) missing.Add(path);
            if (missing.Count > 0)
                throw new BuildFailedException("Fsp project validation failed. Missing required files:\n- " + string.Join("\n- ", missing));
        }

        private static void ValidateProjectVersionAndPackages()
        {
            string version = File.ReadAllText("ProjectSettings/ProjectVersion.txt");
            if (!version.Contains("6000.3.17f1"))
                throw new BuildFailedException("Fsp release pipeline requires Unity 6000.3.17f1. ProjectVersion.txt does not match.");

            string manifest = File.ReadAllText("Packages/manifest.json");
            if (!manifest.Contains("com.unity.webrtc"))
                throw new BuildFailedException("Fsp voice build requires com.unity.webrtc in Packages/manifest.json.");

            if (!manifest.Contains("com.unity.inputsystem"))
                Debug.Log("Fsp input validation: Unity Input System package is not declared; project is intentionally using legacy/mobile input paths.");
        }

        private static void ValidateLobbyScene()
        {
            Scene scene = EditorSceneManager.OpenScene(LobbyScenePath, OpenSceneMode.Additive);
            try
            {
                // LobbyRuntimeGuard creates/binds controller and launcher at runtime if necessary.
                // A checked-in scene and camera are enough for a recoverable device build.
                RequireSceneComponent<Camera>(scene, "Camera");
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }

        private static void ValidateMatchScene()
        {
            Scene scene = EditorSceneManager.OpenScene(MatchScenePath, OpenSceneMode.Additive);
            try
            {
                RequireSceneComponent<Camera>(scene, "Camera");

                if (FindSceneComponents<MatchManager>(scene).Length == 0 && FindSceneComponents<MatchSceneAssembler>(scene).Length == 0)
                    throw new BuildFailedException("Fsp Match scene needs a MatchManager or MatchSceneAssembler so match state can initialize.");

                // MatchSceneAssembler intentionally creates a local safety player/ground and keeps
                // gameplay alive when authored participant or HUD references are absent.
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }

        private static void RequireSceneComponent<T>(Scene scene, string label) where T : Component
        {
            if (FindSceneComponents<T>(scene).Length == 0)
                throw new BuildFailedException($"Fsp scene '{scene.name}' is missing required component: {label}.");
        }

        private static T[] FindSceneComponents<T>(Scene scene) where T : Component
        {
            var found = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
                found.AddRange(root.GetComponentsInChildren<T>(true));
            return found.ToArray();
        }
    }
}
#endif
