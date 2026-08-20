#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fsp.EditorTools
{
    /// <summary>Stops release builds when an original BMG sound is missing, empty or not importable.</summary>
    public sealed class FspAudioAssetBuildGuard : IPreprocessBuildWithReport
    {
        public int callbackOrder => -940;

        public static readonly string[] RequiredAudio =
        {
            "bmg_lobby_theme", "bmg_match_ambience", "bmg_ui_click", "bmg_ui_confirm", "bmg_ui_back",
            "bmg_rifle_shot", "bmg_reload", "bmg_empty", "bmg_damage", "bmg_footstep_sand_01",
            "bmg_footstep_sand_02", "bmg_jump", "bmg_land", "bmg_pickup", "bmg_heal",
            "bmg_zone_warning", "bmg_victory", "bmg_defeat", "bmg_plane_engine_loop",
            "bmg_parachute_wind_loop", "bmg_parachute_open", "bmg_vehicle_engine_loop"
        };

        public void OnPreprocessBuild(BuildReport report)
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            foreach (string clipName in RequiredAudio)
            {
                string path = $"Assets/Fsp/Art/Resources/Audio/{clipName}.wav";
                if (!File.Exists(path) || new FileInfo(path).Length < 8192)
                    throw new BuildFailedException("Required original BMG audio is missing or invalid: " + path);

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null || clip.length < .05f || clip.samples <= 0)
                    throw new BuildFailedException("Unity failed to import required original BMG audio: " + path);
            }

            Debug.Log($"BMG AUDIO GATE PASSED: {RequiredAudio.Length} original clips imported successfully.");
        }
    }
}
#endif
