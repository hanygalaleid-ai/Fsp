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
    /// <summary>Validates authored BMG mesh bounds before production builds, with asset-type-aware limits.</summary>
    public sealed class BmgAuthored3DScaleAudit : IPreprocessBuildWithReport
    {
        private const string Root = "Assets/Fsp/Art/Resources/Models/BMG";
        public int callbackOrder => -840;

        public void OnPreprocessBuild(BuildReport report)
        {
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { Root });
            List<string> bad = new();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model == null) { bad.Add(path + " (not loadable)"); continue; }
                Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0) { bad.Add(path + " (no renderer)"); continue; }

                Bounds bounds = renderers[0].bounds;
                for (int r = 1; r < renderers.Length; r++) bounds.Encapsulate(renderers[r].bounds);
                Vector3 size = bounds.size;
                float max = Mathf.Max(size.x, size.y, size.z);
                float min = Mathf.Min(size.x, size.y, size.z);
                float allowedMax = AllowedMaxDimension(path);

                if (!IsFinite(max) || max <= 0.0001f || max > allowedMax || min < 0f)
                    bad.Add($"{path} (bounds {size}, allowed max {allowedMax:0.#})");
            }

            if (bad.Count > 0)
                throw new BuildFailedException("BMG authored mesh scale/bounds audit failed:\n" + string.Join("\n", bad));

            Debug.Log($"BMG AUTHORED SCALE AUDIT PASSED ({guids.Length} model assets; environment/world assets use large-map bounds). ");
        }

        private static float AllowedMaxDimension(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path)?.ToLowerInvariant() ?? string.Empty;
            if (name.Contains("environment") || name.Contains("world") || name.Contains("island") || name.Contains("sunscar")) return 1000f;
            if (name.Contains("plane") || name.Contains("aircraft")) return 150f;
            if (name.Contains("vehicle") || name.Contains("buggy") || name.Contains("car")) return 40f;
            if (name.Contains("parachute")) return 30f;
            if (name.Contains("character") || name.Contains("soldier")) return 10f;
            if (name.Contains("rifle") || name.Contains("smg") || name.Contains("weapon")) return 8f;
            return 100f;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
#endif
