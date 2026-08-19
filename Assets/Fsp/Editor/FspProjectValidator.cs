#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Fsp.BattleRoyale;
using Fsp.Lobby;
using Fsp.UI;
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
            "Assets/Fsp/Scripts/BattleRoyale/SafeZoneController.cs",
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
            Debug.Log("Fsp validation passed: required files, authored gameplay scenes and runtime online wiring are ready for build.");
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
            {
                if (!File.Exists(path)) missing.Add(path);
            }

            if (missing.Count > 0)
                throw new BuildFailedException("Fsp project validation failed. Missing required files:\n- " + string.Join("\n- ", missing));
        }

        private static void ValidateProjectVersionAndPackages()
        {
            string version = File.ReadAllText("ProjectSettings/ProjectVersion.txt");
            if (!version.Contains("6000.3.17f1"))
                Debug.LogWarning("Fsp was prepared for Unity 6000.3.17f1. Current ProjectVersion.txt differs.");

            string manifest = File.ReadAllText("Packages/manifest.json");
            if (!manifest.Contains("com.unity.webrtc"))
                throw new BuildFailedException("Fsp voice build requires com.unity.webrtc in Packages/manifest.json.");

            if (!manifest.Contains("com.unity.inputsystem"))
                Debug.LogWarning("Unity Input System package is not declared; legacy/mobile controls must remain enabled in Player Settings.");
        }

        private static void ValidateLobbyScene()
        {
            Scene scene = EditorSceneManager.OpenScene(LobbyScenePath, OpenSceneMode.Additive);
            try
            {
                RequireSceneComponent<LobbyController>(scene, "LobbyController");
                RequireSceneComponent<LobbyMatchLauncher>(scene, "LobbyMatchLauncher");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void ValidateMatchScene()
        {
            Scene scene = EditorSceneManager.OpenScene(MatchScenePath, OpenSceneMode.Additive);
            try
            {
                MatchParticipant[] participants = FindSceneComponents<MatchParticipant>(scene);
                int localPlayers = 0;
                foreach (MatchParticipant participant in participants)
                {
                    if (participant != null && participant.IsLocalPlayer) localPlayers++;
                }

                if (localPlayers != 1)
                    throw new BuildFailedException($"Fsp Match scene must contain exactly one authored local MatchParticipant; found {localPlayers}.");

                RequireSceneComponent<BattleRoyaleHud>(scene, "BattleRoyaleHud");
                RequireSceneComponent<SafeZoneController>(scene, "SafeZoneController");

                if (FindSceneComponents<MatchManager>(scene).Length == 0 && FindSceneComponents<MatchSceneAssembler>(scene).Length == 0)
                    throw new BuildFailedException("Fsp Match scene needs an authored MatchManager or MatchSceneAssembler so match state can initialize.");

                // Online transport, combat bridge, vehicle sync and squad voice are intentionally
                // installed at runtime after Supabase auth/match state exists. Do not require them
                // as authored scene components or a hard-coded workers.dev URL.
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
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
