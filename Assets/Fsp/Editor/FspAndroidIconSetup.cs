#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Fsp.EditorTools
{
    public static class FspAndroidIconSetup
    {
        // Checked-in launcher icon used by every Android release.
        public const string IconPath = "Assets/Fsp/Art/Resources/UI/fsp_app_icon.png";

        public static void Apply()
        {
            if (!File.Exists(IconPath))
            {
                throw new BuildFailedException("Required Android launcher icon was not found: " + IconPath);
            }

            AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (icon == null || icon.width <= 0 || icon.height <= 0)
                throw new BuildFailedException("Android launcher icon exists but could not be imported: " + IconPath);

            int[] sizes = PlayerSettings.GetIconSizes(NamedBuildTarget.Android, IconKind.Application);
            Texture2D[] icons = new Texture2D[sizes.Length];
            for (int i = 0; i < icons.Length; i++) icons[i] = icon;
            PlayerSettings.SetIcons(NamedBuildTarget.Android, icons, IconKind.Application);

            AssetDatabase.SaveAssets();
            Debug.Log($"[FSP] Android launcher icon applied: {IconPath} ({icon.width}x{icon.height})");
        }
    }
}
#endif
