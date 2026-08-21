#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fsp.EditorTools
{
    /// <summary>
    /// Visual integrity gate for the clean BMG Android build.
    /// Legacy FSP lobby artwork is intentionally not required.
    /// </summary>
    public sealed class FspVisualAssetBuildGuard : IPreprocessBuildWithReport
    {
        private static readonly string[] RequiredVisuals =
        {
            "Assets/Fsp/Art/Resources/UI/bmg_app_icon.png",
            "Assets/Fsp/Art/Resources/UI/bmg_adaptive_foreground.png",
            "Assets/Fsp/Art/Resources/UI/bmg_adaptive_background.png",
            "Assets/Fsp/Art/Resources/UI/mobile_joystick.png",
            "Assets/Fsp/Art/Resources/World/bmg_desert_ground_v3.png",
            "Assets/Fsp/Art/Resources/World/bmg_fortress_wall_v3.png",
            "Assets/Fsp/Art/Resources/World/bmg_wood_floor_v3.png"
        };

        public int callbackOrder => -900;

        public void OnPreprocessBuild(BuildReport report)
        {
            foreach (string path in RequiredVisuals)
            {
                if (!File.Exists(path))
                    throw new BuildFailedException("Required clean BMG visual asset is missing: " + path);

                if (new FileInfo(path).Length < 256)
                    throw new BuildFailedException("Required clean BMG visual asset is empty or invalid: " + path);

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture == null || texture.width <= 0 || texture.height <= 0)
                    throw new BuildFailedException("Unity failed to import required clean BMG visual asset: " + path);
            }

            Debug.Log("BMG CLEAN VISUAL GATE PASSED: no legacy FSP lobby bitmap dependency.");
        }
    }
}
#endif
