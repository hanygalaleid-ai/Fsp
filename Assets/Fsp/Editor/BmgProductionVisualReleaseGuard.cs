#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fsp.EditorTools
{
    /// <summary>
    /// Prevents release builds from silently falling back to legacy mk1/procedural visuals.
    /// The checked-in BMG Production pack must contain the canonical static model set with
    /// minimum source sizes so a tiny placeholder cannot pass the gate by filename alone.
    /// </summary>
    public sealed class BmgProductionVisualReleaseGuard : IPreprocessBuildWithReport
    {
        private const string ProductionFolder = "Assets/Fsp/Art/Resources/Models/BMG/Production";

        private static readonly Dictionary<string, long> RequiredModels = new()
        {
            { "bmg_sunscar_environment", 200_000 },
            { "bmg_transport_plane", 15_000 },
            { "bmg_parachute", 10_000 },
            { "bmg_buggy", 10_000 },
            { "bmg_assault_rifle", 4_000 },
            { "bmg_smg", 4_000 },
            { "bmg_character_01", 20_000 },
            { "bmg_character_02", 20_000 },
            { "bmg_character_03", 20_000 },
            { "bmg_character_04", 20_000 },
            { "bmg_character_05", 20_000 },
            { "bmg_character_06", 20_000 }
        };

        public int callbackOrder => -1200;

        public void OnPreprocessBuild(BuildReport report)
        {
            var failures = new List<string>();
            foreach (var entry in RequiredModels)
            {
                string modelName = entry.Key;
                long minimumBytes = entry.Value;
                string[] guids = AssetDatabase.FindAssets(modelName + " t:GameObject", new[] { ProductionFolder });
                string acceptedPath = null;

                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.StartsWith(ProductionFolder + "/", StringComparison.Ordinal)) continue;
                    if (path.IndexOf("_mk1", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                    if (!string.Equals(Path.GetFileNameWithoutExtension(path), modelName, StringComparison.OrdinalIgnoreCase)) continue;
                    acceptedPath = path;
                    break;
                }

                if (string.IsNullOrEmpty(acceptedPath))
                {
                    failures.Add(modelName + " (missing)");
                    continue;
                }

                var info = new FileInfo(acceptedPath);
                if (!info.Exists || info.Length < minimumBytes)
                    failures.Add($"{modelName} ({(info.Exists ? info.Length : 0)} bytes; minimum {minimumBytes})");
            }

            if (failures.Count > 0)
            {
                throw new BuildFailedException(
                    "BMG PRODUCTION VISUAL GATE: static production 3D pack is incomplete or below the approved density floor. " +
                    "Legacy *_mk1/procedural assets are never accepted. Failed: " + string.Join(", ", failures));
            }

            const string controllerPath = "Assets/Fsp/Scripts/Presentation/BmgProductionVisualController.cs";
            if (AssetDatabase.LoadMainAssetAtPath(controllerPath) == null)
                throw new BuildFailedException("BMG PRODUCTION VISUAL GATE: production visual controller is missing.");

            Debug.Log($"BMG PRODUCTION VISUAL GATE PASSED ({RequiredModels.Count} static production models with density validation). No mk1 model is accepted.");
        }
    }
}
#endif
