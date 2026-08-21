#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fsp.EditorTools
{
    /// <summary>Stops Android/production builds if Build 149 authored 3D assets were dropped from source control.</summary>
    public sealed class BmgAuthored3DAssetBuildGuard : IPreprocessBuildWithReport
    {
        private static readonly string[] RequiredAssets =
        {
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_assault_rifle_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_helmet_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_backpack_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_loot_crate_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_buggy_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_watchtower_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_transport_plane_mk1.obj"
        };

        public int callbackOrder => -850;

        public void OnPreprocessBuild(BuildReport report)
        {
            for (int i = 0; i < RequiredAssets.Length; i++)
            {
                UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(RequiredAssets[i]);
                if (asset == null)
                    throw new BuildFailedException("BMG authored 3D asset is missing: " + RequiredAssets[i]);
            }

            Debug.Log($"BMG AUTHORED 3D GATE PASSED ({RequiredAssets.Length} required meshes).");
        }
    }
}
#endif
