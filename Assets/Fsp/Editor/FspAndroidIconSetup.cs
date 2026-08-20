#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Fsp.EditorTools
{
    public static class FspAndroidIconSetup
    {
        // Optional checked-in launcher icon. Do not depend on removed procedural build-art code.
        public const string IconPath = "Assets/Fsp/Art/Resources/UI/fsp_app_icon.png";

        public static void Apply()
        {
            if (!File.Exists(IconPath))
            {
                Debug.LogWarning("[FSP] Optional Android launcher icon was not found; preserving current PlayerSettings icons: " + IconPath);
                return;
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
