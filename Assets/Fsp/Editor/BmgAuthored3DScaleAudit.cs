#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fsp.EditorTools
{
    /// <summary>Validates authored Build 149 mesh bounds before production builds.</summary>
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
                float max = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                float min = Mathf.Min(bounds.size.x, bounds.size.y, bounds.size.z);
                if (!IsFinite(max) || max <= 0.0001f || max > 100f || min < 0f)
                    bad.Add($"{path} (bounds {bounds.size})");
            }

            if (bad.Count > 0)
                throw new BuildFailedException("BMG authored mesh scale/bounds audit failed:\n" + string.Join("\n", bad));

            Debug.Log($"BMG AUTHORED SCALE AUDIT PASSED ({guids.Length} model assets).");
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
#endif
