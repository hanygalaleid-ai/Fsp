#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Fsp.EditorTools
{
    public static class FspProjectValidator
    {
        private static readonly string[] RequiredFiles =
        {
            "Assets/Fsp/Scenes/Lobby.unity",
            "Assets/Fsp/Scenes/Match.unity",
            "Assets/Fsp/Scripts/BattleRoyale/MatchManager.cs",
            "Assets/Fsp/Scripts/BattleRoyale/SafeZoneController.cs",
            "Assets/Fsp/Scripts/Player/PlayerVitals.cs",
            "Assets/Fsp/Scripts/Player/ThirdPersonMotor.cs",
            "Assets/Fsp/Scripts/Combat/HitscanWeapon.cs",
            "Assets/Fsp/Scripts/Inventory/PlayerInventory.cs",
            "Assets/Fsp/Scripts/AI/BotBrain.cs",
            "Assets/Fsp/Scripts/Vehicles/SimpleVehicleController.cs",
            "Packages/manifest.json",
            "ProjectSettings/ProjectVersion.txt"
        };

        [MenuItem("Fsp/Validate/Validate Project")]
        public static void ValidateFromMenu()
        {
            ValidateOrThrow();
            Debug.Log("Fsp validation passed: required gameplay/build files are present.");
        }

        public static void ValidateOrThrow()
        {
            var missing = new List<string>();
            foreach (string path in RequiredFiles)
            {
                if (!File.Exists(path)) missing.Add(path);
            }

            if (missing.Count > 0)
                throw new BuildFailedException("Fsp project validation failed. Missing required files:\n- " + string.Join("\n- ", missing));

            string version = File.ReadAllText("ProjectSettings/ProjectVersion.txt");
            if (!version.Contains("6000.3.17f1"))
                Debug.LogWarning("Fsp was prepared for Unity 6000.3.17f1. Current ProjectVersion.txt differs.");

            string manifest = File.ReadAllText("Packages/manifest.json");
            if (!manifest.Contains("com.unity.inputsystem"))
                Debug.LogWarning("Unity Input System package is not declared; mobile/PC controls may not initialize as intended.");

            AssetDatabase.Refresh();
        }
    }
}
#endif
