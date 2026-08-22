#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fsp.EditorTools
{
    /// <summary>
    /// Blocks release builds from shipping the old mk1/procedural presentation as final BMG art.
    /// Production meshes must live under Resources/Models/BMG/Production and use the canonical names below.
    /// </summary>
    public sealed class BmgProductionVisualReleaseGuard : IPreprocessBuildWithReport
    {
        private const string ProductionFolder = "Assets/Fsp/Art/Resources/Models/BMG/Production";

        private static readonly string[] RequiredModelNames =
        {
            "bmg_sunscar_environment",
            "bmg_transport_plane",
            "bmg_parachute",
            "bmg_buggy",
            "bmg_assault_rifle",
            "bmg_smg",
            "bmg_character_01",
            "bmg_character_02",
            "bmg_character_03",
            "bmg_character_04",
            "bmg_character_05",
            "bmg_character_06"
        };

        public int callbackOrder => -1200;

        public void OnPreprocessBuild(BuildReport report)
        {
            var missing = new List<string>();
            foreach (string modelName in RequiredModelNames)
            {
                string[] guids = AssetDatabase.FindAssets(modelName + " t:GameObject", new[] { ProductionFolder });
                bool found = false;
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.StartsWith(ProductionFolder + "/", StringComparison.Ordinal)) continue;
                    string filename = System.IO.Path.GetFileNameWithoutExtension(path);
                    if (string.Equals(filename, modelName, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) missing.Add(modelName);
            }

            if (missing.Count > 0)
            {
                throw new BuildFailedException(
                    "BMG PRODUCTION VISUAL GATE: release blocked because genuine production 3D assets are missing. " +
                    "The old *_mk1/procedural assets are not accepted as final art. Missing: " + string.Join(", ", missing));
            }

            const string controllerPath = "Assets/Fsp/Scripts/Presentation/BmgProductionVisualController.cs";
            if (AssetDatabase.LoadMainAssetAtPath(controllerPath) == null)
                throw new BuildFailedException("BMG PRODUCTION VISUAL GATE: production visual controller is missing.");

            Debug.Log($"BMG PRODUCTION VISUAL GATE PASSED ({RequiredModelNames.Length} production models). No mk1 model is accepted as release art.");
        }
    }
}
#endif
