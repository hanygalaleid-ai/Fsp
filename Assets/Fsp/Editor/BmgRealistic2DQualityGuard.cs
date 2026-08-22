#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fsp.EditorTools
{
    /// <summary>
    /// Prevents another Android build from shipping tiny/compressed placeholder versions of the
    /// approved realistic BMG 2D artwork. Both imported dimensions and source byte size are checked.
    /// </summary>
    public sealed class BmgRealistic2DQualityGuard : IPreprocessBuildWithReport
    {
        private readonly struct Requirement
        {
            public readonly string Path;
            public readonly int MinWidth;
            public readonly int MinHeight;
            public readonly long MinBytes;

            public Requirement(string path, int minWidth, int minHeight, long minBytes)
            {
                Path = path;
                MinWidth = minWidth;
                MinHeight = minHeight;
                MinBytes = minBytes;
            }
        }

        private static readonly Requirement[] Required =
        {
            new("Assets/Fsp/Art/Resources/BMG/UI/bmg_lobby_modern.jpg", 1280, 720, 80000),
            new("Assets/Fsp/Art/Resources/BMG/Characters/bmg_characters_6_atlas.jpg", 768, 768, 80000),
            new("Assets/Fsp/Art/Resources/BMG/Weapons/bmg_weapons_5_atlas.jpg", 1200, 120, 30000)
        };

        public int callbackOrder => -1250;

        public void OnPreprocessBuild(BuildReport report)
        {
            var errors = new List<string>();
            foreach (Requirement requirement in Required)
            {
                if (!File.Exists(requirement.Path))
                {
                    errors.Add(requirement.Path + " (missing)");
                    continue;
                }

                long bytes = new FileInfo(requirement.Path).Length;
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(requirement.Path);
                if (texture == null)
                {
                    errors.Add(requirement.Path + " (not importable as Texture2D)");
                    continue;
                }

                if (texture.width < requirement.MinWidth || texture.height < requirement.MinHeight || bytes < requirement.MinBytes)
                {
                    errors.Add($"{requirement.Path} ({texture.width}x{texture.height}, {bytes / 1024f:0.0} KB; required >= {requirement.MinWidth}x{requirement.MinHeight}, {requirement.MinBytes / 1024f:0.0} KB)");
                }
                else
                {
                    Debug.Log($"BMG REALISTIC 2D OK: {requirement.Path} ({texture.width}x{texture.height}, {bytes / 1024f:0.0} KB)");
                }
            }

            if (errors.Count > 0)
                throw new BuildFailedException("BMG REALISTIC 2D QUALITY GATE: build blocked so compressed/blurred art cannot ship.\n" + string.Join("\n", errors));

            Debug.Log("BMG REALISTIC 2D QUALITY GATE PASSED: lobby, character atlas and weapon atlas are production-size assets.");
        }
    }
}
#endif
